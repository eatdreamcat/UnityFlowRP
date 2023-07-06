using System;
using System.Collections.Generic;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RendererUtils;



// Resove the ambiguity in the RendererList name (pick the in-engine version)
using RendererList = UnityEngine.Rendering.RendererUtils.RendererList;
using RendererListDesc = UnityEngine.Rendering.RendererUtils.RendererListDesc;



namespace UnityEngine.Rendering.FlowPipeline
{
    public partial class FlowRenderPipeline : RenderPipeline
    {
        /// <summary>
        /// Shader Tag for the High Definition Render Pipeline.
        /// </summary>
        public const string k_ShaderTagName = "FlowRenderPipeline";
         
        // RenderPipeline Config Asset
        public static FlowRenderPipelineAsset asset
        {
            get => GraphicsSettings.currentRenderPipeline as FlowRenderPipelineAsset;
        }

        #region PRIVATE

        private RenderGraph m_RenderGraph = new RenderGraph("FlowRenderGraph");
        
        RenderTexture m_TemporaryTargetForCubemaps;

        #endregion

        #region READONLY

        readonly CameraSystem m_CameraSystem;
        readonly FlowRenderPipelineAsset m_Asset;

        #endregion


        #region STRUCT

        /**
         * Store the cullingResult
         */
        struct FlowRPCullingResults
        {
            public CullingResults cullingResults;

            // TODO: DecalCullResults

            internal void Reset()
            {
            }
        }


        /**
         *  Store render data
         */
        struct RenderRequest
        {
            public FlowRPCullingResults cullingResults;
            public int index;
            public FRPCamera frpCamera;
            
            public FlowRenderGraphData.NodeType nodeType;
            public string nodeID;
            public string name;

            public FlowRenderGraphData currentGraphData;
        }

        #endregion

        public FlowRenderPipeline(FlowRenderPipelineAsset asset)
        {
            //// Asign readonly properties
            m_Asset = asset;
            m_CameraSystem = new CameraSystem();
            //// end of  Asign readonly properties
        }


#if UNITY_2021_1_OR_NEWER
        protected override void Render(ScriptableRenderContext renderContext, Camera[] cameras)
        {
            Render(renderContext, new List<Camera>(cameras));
        }

#endif

#if UNITY_2021_1_OR_NEWER
        // Only for internal use, outside of SRP people can call Camera.Render()
        // is invoked by CompositionManager in HDRP 
        internal void InternalRender(ScriptableRenderContext renderContext, List<Camera> cameras)
        {
            Render(renderContext, cameras);
        }

#endif


        /// <summary>
        /// Render Entry
        /// </summary>
        /// <param name="renderContext"></param>
        /// <param name="cameras"></param>
#if UNITY_2021_1_OR_NEWER
        protected override void Render(ScriptableRenderContext renderContext, List<Camera> cameras)
#else
        protected override void Render(ScriptableRenderContext renderContext, Camera[] cameras)
#endif
        {

            #region preparation and validation

            /// Doing some preparation and validation work. 

#if UNITY_2021_1_OR_NEWER
            if (cameras.Count == 0)
#else
            if (cameras.Length == 0)
#endif
                return;

            
            #endregion

            #region Before Rendering, Check XR? Special Camera Handle (Preview , SceneView... )
            
            //// !!!!!!!!!!!!!!!!!!!!!!!!!! ATTENTION ! !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! //// 
            ///   ------------------------ Begin Rendering -------------------------------- ////


            /// 1.  Invoke BeginRendering Delegate
#if UNITY_2021_1_OR_NEWER
            BeginContextRendering(renderContext, cameras);
#else
            BeginFrameRendering(renderContext, cameras);
#endif

            /// TODO: we should deal with preview camera
            ///
            ///


            /// TODO: fucking XR ??
            /// With XR multi-pass enabled, each camera can be rendered multiple times with different parameters
            

            #endregion

            #region Rendering

              // This syntax is awful and hostile to debugging, please don't use it... !!! So why you using it !!!??????????
            using (ListPool<RenderRequest>.Get(out List<RenderRequest> renderRequests))
            {

                #region CameraList Construct and Sort

                /// Note that HDRP dont support multiple cameras, but we need to support it in my POWERFUL FlowRP!!!!!!!!!!!!!!!Awsome!!!!!!!!!
                var finalCameras = m_CameraSystem.SetupFrameAndSort(cameras);

#if UNITY_EDITOR
                // See comment below about the preview camera workaround
                bool hasGameViewCamera = false;
                foreach (var c in finalCameras)
                {
                    if (c.cameraType == CameraType.Game)
                    {
                        hasGameViewCamera = true;
                        break;
                    }
                }
#endif
                

                #endregion


                #region Culling Loop

                 // Culling loop 
                foreach (Camera camera in finalCameras)
                {
#if UNITY_EDITOR
                    ///////
                    /// //      WATCH OUT THE SHIT!!!!!!!!!!!!!!!!!!!!!!
                    /// //////
                    // We selecting a camera in the editor, we have a preview that is drawn.
                    // For legacy reasons, Unity will render all preview cameras when rendering the GameView
                    // Actually, we don't need this here because we call explicitly Camera.Render when we
                    // need a preview
                    //
                    // This is an issue, because at some point, you end up with 2 cameras to render:
                    // - Main Camera (game view)
                    // - Preview Camera (preview)
                    // If the preview camera is rendered last, it will alter the "GameView RT" RenderTexture
                    // that was previously rendered by the Main Camera.
                    // This is an issue.
                    //
                    // Meanwhile, skipping all preview camera when rendering the game views is sane,
                    // and will workaround the aformentionned issue.
                    if (hasGameViewCamera && camera.cameraType == CameraType.Preview)
                        continue;
#endif

                    var cullingResults = UnsafeGenericPool<FlowRPCullingResults>.Get();
                    cullingResults.Reset();
                    var skipRequest = false;
                    
                    // get culling params
                    skipRequest = !TryCalculateFrameParameters(camera, out var additionalCameraData, out var cullingParams, out var currentGraphData, out var frpCamera);

                    /// Skip !!! Off duty !!!!!!!!!!!!!!!!!!
                    if (skipRequest)
                    {
                        // Submit render context and free pooled resources for this request
                        renderContext.Submit();
                        UnsafeGenericPool<FlowRPCullingResults>.Release(cullingResults);
                        EndCameraRendering(renderContext, camera);
                        continue;
                    }

                    skipRequest = !TryCull(camera, renderContext, cullingParams, m_Asset, ref cullingResults);
                    
                    /// Skip !!! Off duty !!!!!!!!!!!!!!!!!!
                    if (skipRequest)
                    {
                        // Submit render context and free pooled resources for this request
                        renderContext.Submit();
                        UnsafeGenericPool<FlowRPCullingResults>.Release(cullingResults);
                        EndCameraRendering(renderContext, camera);
                        continue;
                    }
                    
                    /// oops!!!! we need to work more
                 
                    // Add render request
                    var flowNode = currentGraphData.TryGetFlowNode(currentGraphData.EntryID);
                    if (flowNode != null && flowNode.flowOut.Count > 0)
                    {
                        // entry node has next pass
                        flowNode = currentGraphData.TryGetFlowNode(flowNode.flowOut[0]);
                    }
                    else
                    {
                        flowNode = null;
                    }
                    bool canBreakLoop = false;
                    int loopCount = 0;
                    const int MaxLoopCount = 1000;
                    
                    while (flowNode != null && canBreakLoop == false && ++loopCount <= MaxLoopCount)
                    {
                        if (!string.IsNullOrEmpty(flowNode.dataID))
                        {
                            switch (flowNode.dataType)
                            {
                                case FlowRenderGraphData.NodeType.DrawRendererNode:
                                case FlowRenderGraphData.NodeType.DrawFullScreenNode:
                                {
                                    // draw flow edge
                                    if (flowNode.flowOut.Count > 0)
                                    {
                                        var targetNodeID = flowNode.flowOut[0];
                                        Debug.Assert(!string.IsNullOrEmpty(targetNodeID), $"[GraphView.Draw] Node {flowNode.guid} flow out connection target {targetNodeID} is null ");
                                        // pass node only has one flow output.
                                        flowNode = currentGraphData.TryGetFlowNode(flowNode.flowOut[0]);
                                        
                                        renderRequests.Add(new RenderRequest()
                                        {
                                            cullingResults = cullingResults,
                                            index = renderRequests.Count,
                                            frpCamera = frpCamera, 
                                
                                            nodeType = flowNode.dataType,
                                            nodeID = flowNode.dataID,
                                            name = flowNode.name,
                                            
                                            currentGraphData = currentGraphData
                                        });
                                        
                                    }
                                    else
                                    {
                                        canBreakLoop = true;
                                    }
                                }
                                    break;
                                
                                case FlowRenderGraphData.NodeType.BranchNode:
                                {
                                    // TODO:
                                    canBreakLoop = true;
                                }
                                    break;
                            
                                case FlowRenderGraphData.NodeType.LoopNode:
                                {
                                    // TODO: 
                                    canBreakLoop = true;
                                }
                                    break;
                            }
                        }
                    }
                    
                    Debug.Assert(loopCount <= MaxLoopCount, "A deadloop occured while draw connections, please check is there a new Node type not be managed.");
                } // end of culling loop

                #endregion

                #region Render the render requset
                /// render all the render request
                    using (new ProfilingScope(null, ProfilingSampler.Get(ProfileId.FlowPipelineRenderAllRenderRequest)))
                    {
                       
                        
                        // Execute render request graph
                        for (int i = 0; i < renderRequests.Count; ++i)
                        {
                            var renderRequest = renderRequests[i];
                            var cmd = CommandBufferPool.Get(renderRequest.name);
                            
                            #region Render Requset
                            
                            using (new ProfilingScope(cmd, renderRequest.frpCamera.profilingSampler))
                            {
                                // TODO: camera settings
                                // cmd.SetInvertCulling(renderRequest.cameraSettings.invertFaceCulling);
                                ExecuteRenderRequest(renderRequest, renderContext, cmd);
                                cmd.SetInvertCulling(false);
                            }
                            

                            #endregion
                            

                            #region After Render Requset

                            // Let's make sure to keep track of lights that will generate screen space shadows.
                            CollectScreenSpaceShadowData();

                            renderContext.ExecuteCommandBuffer(cmd);
                            CommandBufferPool.Release(cmd);
                            renderContext.Submit();
                            
                            //  EndCameraRendering callback should be executed outside of any profiling scope in case user code submits the renderContext
                            EndCameraRendering(renderContext, renderRequest.frpCamera.camera);
                            
                            #endregion
                            
                        }
                    
                }

                #endregion
            }


            #endregion

            #region After render all cameras

            // TODO: Now that all cameras have been rendered, let's make sure to keep track of update the screen space shadow data

            m_RenderGraph.EndFrame();
            m_CameraSystem.ReleaseFrame();

#if UNITY_2021_1_OR_NEWER
            EndContextRendering(renderContext, cameras);
#else
            EndFrameRendering(renderContext, cameras);
#endif

            #endregion
        }

        void InitializeGlobalResources(ScriptableRenderContext renderContext)
        {
            
        }

        void ExecuteRenderRequest(
            RenderRequest renderRequest,
            ScriptableRenderContext renderContext,
            CommandBuffer cmd/*,
            AOVRequestData aovRequest*/
        )
        {
            
            InitializeGlobalResources(renderContext);
            
            var frpCamera = renderRequest.frpCamera;
            var camera = frpCamera.camera;
            var cullingResults = renderRequest.cullingResults.cullingResults;
           
            
            // Updates RTHandle
            frpCamera.BeginRender(cmd);
            
            // TODO: Raytracing Support
            
            
            // Apparently scissor states can leak from editor code. As it is not used currently in HDRP (apart from VR). We disable scissor at the beginning of the frame.
            cmd.DisableScissorRect();
            
            SetupCameraProperties(camera, renderContext, cmd);
            
            
            try
            {
                ExecuteWithRenderGraph(renderRequest, /*aovRequest, aovBuffers, aovCustomPassBuffers,*/ renderContext, cmd);
            }
            catch (Exception e)
            {
                Debug.LogError("Error while building Render Graph.");
                Debug.LogException(e);
            }
            
            
            
            // This is required so that all commands up to here are executed before EndCameraRendering is called for the user.
            // Otherwise command would not be rendered in order.
            renderContext.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            
        }

        
        void SetupCameraProperties(Camera camera, ScriptableRenderContext renderContext, CommandBuffer cmd)
        {
            // The next 2 functions are required to flush the command buffer before calling functions directly on the render context.
            // This way, the commands will execute in the order specified by the C# code.
            renderContext.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            renderContext.SetupCameraProperties(camera, false);
        }
        
        
        /// <summary>
        /// TODO: CollectScreenSpaceShadowData
        /// </summary>
        void CollectScreenSpaceShadowData()
        {
            
        }

        bool TryCalculateFrameParameters(
            Camera camera,
            out FlowRPAdditionalCameraData additionalCameraData,
            out ScriptableCullingParameters cullingParams,
            out FlowRenderGraphData currentGraphData,
            out FRPCamera frpCamera
        )
        {
            // First, get aggregate of frame settings base on global settings, camera frame settings and debug settings
            // Note: the SceneView camera will never have additionalCameraData
            additionalCameraData = FlowUtility.TryGetAdditionalCameraDataOrDefault(camera);
            cullingParams = default;
            
            currentGraphData = additionalCameraData.renderGraphData;
            
            frpCamera = FRPCamera.GetOrCreate(camera);
            // From this point, we should only use frame settings from the camera
            frpCamera.Update(this);

            if (!camera.TryGetCullingParameters(camera.stereoEnabled, out cullingParams) || currentGraphData == null || !currentGraphData.HasEntry() || string.IsNullOrEmpty(currentGraphData.Entry.startPoint))
            {
                return false;
            }
            return true;
        }

        /////////////// Dispose !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        protected override void Dispose(bool disposing)
        {
            m_CameraSystem.Cleanup();
        }

        internal GraphicsFormat GetColorBufferFormat(FlowRenderGraphData.TextureBufferNode textureBufferNode)
        {
            if (CoreUtils.IsSceneFilteringEnabled())
                return GraphicsFormat.R16G16B16A16_SFloat;

            return textureBufferNode.colorFormat;
        }

        #region Static functions

        static bool TryCull(
            Camera camera,
            ScriptableRenderContext renderContext,
            ScriptableCullingParameters cullingParams,
            FlowRenderPipelineAsset hdrp,
            ref FlowRPCullingResults cullingResults
        )
        {
            /// deal with UI
            if (camera.cameraType == CameraType.Reflection || camera.cameraType == CameraType.Preview)
            {
#if UNITY_2020_2_OR_NEWER
                ScriptableRenderContext.EmitGeometryForCamera(camera);
#endif
            }
#if UNITY_EDITOR
            // emit scene view UI
            else if (camera.cameraType == CameraType.SceneView)
            {
                ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
            }
#endif

            // This needs to be called before culling, otherwise in the case where users generate intermediate renderers, it can provoke crashes.
            BeginCameraRendering(renderContext, camera);

            // TODO: Decal Culling

            // TODO: Probe Culling 

            // TODO: RayTracing Culling

            using (new ProfilingScope(null, ProfilingSampler.Get(ProfileId.CullResultsCull)))
            {
                cullingResults.cullingResults = renderContext.Cull(ref cullingParams);
            }

            return true;
        }

        
        /// -------------------------------------------- Draw ----------------------------------------------
        ///
        static void DrawOpaqueRendererList(in ScriptableRenderContext renderContext, CommandBuffer cmd/*, in FrameSettings frameSettings*/, RendererList rendererList)
        {
            // if (!frameSettings.IsEnabled(FrameSettingsField.OpaqueObjects))
            //     return;

            CoreUtils.DrawRendererList(renderContext, cmd, rendererList);
        }


        /// Create Renderer List Desc

        static RendererListDesc CreateOpaqueRendererListDesc(
            CullingResults cull,
            Camera camera,
            ShaderTagId[] passNames,
            PerObjectData rendererConfiguration = 0,
            RenderQueueRange? renderQueueRange = null,
            RenderStateBlock? stateBlock = null,
            Material overrideMaterial = null,
            bool excludeObjectMotionVectors = false
        )
        {
            var result = new RendererListDesc(passNames, cull, camera)
            {
                rendererConfiguration = rendererConfiguration,
                renderQueueRange = renderQueueRange != null ? renderQueueRange.Value : FlowRenderQueue.k_RenderQueue_AllOpaque,
                sortingCriteria = SortingCriteria.CommonOpaque,
                stateBlock = stateBlock,
                overrideMaterial = overrideMaterial,
                excludeObjectMotionVectors = excludeObjectMotionVectors
            };
            return result;
        }

        #endregion
    }
}