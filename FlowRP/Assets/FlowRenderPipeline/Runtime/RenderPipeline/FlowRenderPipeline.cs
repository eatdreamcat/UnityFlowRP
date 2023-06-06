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

        // Renderer Bake configuration can vary depends on if shadow mask is enabled or no
        PerObjectData m_CurrentRendererConfigurationBakedLighting = FlowUtility.k_RendererConfigurationBakedLighting;
        
        
        #region ShaderTagId

        // NOTE: The pass "SRPDefaultUnlit" is a fall back to legacy unlit rendering and is required to support unity 2d + unity UI that render in the scene.
        ShaderTagId[] m_ForwardAndForwardOnlyPassNames = { FlowShaderPassNames.s_ForwardOnlyName, FlowShaderPassNames.s_ForwardName, FlowShaderPassNames.s_SRPDefaultUnlitName, FlowShaderPassNames.s_DecalMeshForwardEmissiveName };
        ShaderTagId[] m_ForwardOnlyPassNames = { FlowShaderPassNames.s_ForwardOnlyName, FlowShaderPassNames.s_SRPDefaultUnlitName, FlowShaderPassNames.s_DecalMeshForwardEmissiveName };

        #endregion
        
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
            public struct Target
            {
                public RenderTargetIdentifier id;
                public CubemapFace face;
                public RTHandle copyToTarget;
                public RTHandle targetDepth;
            }

            public FRPCamera frpCamera;
            public FlowRPCullingResults cullingResults;
            public int index;
            public Target target;
            
            // Indices of render request to render before this one
            public List<int> dependsOnRenderRequestIndices;
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
            using (ListPool<int>.Get(out List<int> rootRenderRequestIndices))
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
                    skipRequest = !TryCalculateFrameParameters(camera, out var additionalCameraData, out var cullingParams);

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
                    ///
                    // Select render target
                    RenderTargetIdentifier targetId = camera.targetTexture ?? new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);
                    // Add render request
                    var request = new RenderRequest
                    {
                        cullingResults = cullingResults,
                        target = new RenderRequest.Target
                        {
                            id = targetId,
                            face = CubemapFace.Unknown,
                        },
                        frpCamera = FRPCamera.GetOrCreate(camera),
                        dependsOnRenderRequestIndices = ListPool<int>.Get(),
                        index = renderRequests.Count,
                    };
                    renderRequests.Add(request);
                    // This is a root render request
                    rootRenderRequestIndices.Add(request.index);
                    
                    

                } // end of culling loop

                #endregion

                #region setup Temporary Target for Cubemaps

                 // TODO: Refactor into a method. If possible remove the intermediate target
                // Find max size for Cubemap face targets and resize/allocate if required the intermediate render target
                {
                    var size = Vector2Int.zero;
                    for (int i = 0; i < renderRequests.Count; ++i)
                    {
                        var renderRequest = renderRequests[i];
                        var isCubemapFaceTarget = renderRequest.target.face != CubemapFace.Unknown;
                        if (!isCubemapFaceTarget)
                            continue;

                        // var width = renderRequest..actualWidth;
                        // var height = renderRequest.flowRPCamera.actualHeight;
                        // size.x = Mathf.Max(width, size.x);
                        // size.y = Mathf.Max(height, size.y);
                    }

                    if (size != Vector2.zero)
                    {
                        // todo: 
                        var probeFormat = GraphicsFormat.D16_UNorm; //(GraphicsFormat)m_Asset.currentPlatformRenderPipelineSettings.lightLoopSettings.reflectionProbeFormat;
                        if (m_TemporaryTargetForCubemaps != null)
                        {
                            if (m_TemporaryTargetForCubemaps.width != size.x
                                || m_TemporaryTargetForCubemaps.height != size.y
                                || m_TemporaryTargetForCubemaps.graphicsFormat != probeFormat)
                            {
                                m_TemporaryTargetForCubemaps.Release();
                                m_TemporaryTargetForCubemaps = null;
                            }
                        }
                        if (m_TemporaryTargetForCubemaps == null)
                        {
                            m_TemporaryTargetForCubemaps = new RenderTexture(
                                size.x, size.y, 1, probeFormat
                            )
                            {
                                autoGenerateMips = false,
                                useMipMap = false,
                                name = "Temporary Target For Cubemap Face",
                                volumeDepth = 1,
                                useDynamicScale = false
                            };
                        }
                    }
                }
                

                #endregion


                #region Render the render requset

                /// Now is time to Do Rendering Work!!!
                using (ListPool<int>.Get(out List<int> renderRequestIndicesToRender))
                {
                    // Flatten the render requests graph in an array that guarantee dependency constraints
                    // TODO: maybe to remove cause we using nodes to drive render pass
                    {
                        using (GenericPool<Stack<int>>.Get(out Stack<int> stack))
                        {
                            stack.Clear();
                            for (int i = rootRenderRequestIndices.Count - 1; i >= 0; --i)
                            {
                                stack.Push(rootRenderRequestIndices[i]);
                                while (stack.Count > 0)
                                {
                                    var index = stack.Pop();
                                    if (!renderRequestIndicesToRender.Contains(index))
                                        renderRequestIndicesToRender.Add(index);

                                    var request = renderRequests[index];
                                    for (int j = 0; j < request.dependsOnRenderRequestIndices.Count; ++j)
                                        stack.Push(request.dependsOnRenderRequestIndices[j]);
                                }
                            }
                        }
                    }

                    /// render all the render request
                    using (new ProfilingScope(null, ProfilingSampler.Get(ProfileId.FlowPipelineRenderAllRenderRequest)))
                    {
                        // TODO: do some optimization ? 
                        // Warm up the RTHandle system so that it gets init to the maximum resolution available (avoiding to call multiple resizes
                        // that can lead to high memory spike as the memory release is delayed while the creation is immediate).
                        {
                            Vector2Int maxSize = new Vector2Int(1, 1);

                            for (int i = renderRequestIndicesToRender.Count - 1; i >= 0; --i)
                            {
                                var renderRequestIndex = renderRequestIndicesToRender[i];
                                var renderRequest = renderRequests[renderRequestIndex];
                                // TODO：
                                // var hdCamera = renderRequest.hdCamera;
                                //
                                // maxSize.x = Math.Max((int)hdCamera.finalViewport.size.x, maxSize.x);
                                // maxSize.y = Math.Max((int)hdCamera.finalViewport.size.y, maxSize.y);
                            }

                            // Here we use the non scaled resolution for the RTHandleSystem ref size because we assume that at some point we will need full resolution anyway.
                            // This is necessary because we assume that after post processes, we have the full size render target for debug rendering
                            // The only point of calling this here is to grow the render targets. The call in BeginRender will setup the current RTHandle viewport size.
                            // RTHandles.SetReferenceSize(maxSize.x, maxSize.y);
                        }
                        
                        // Execute render request graph, in reverse order
                        for (int i = renderRequestIndicesToRender.Count - 1; i >= 0; --i)
                        {
                            var renderRequestIndex = renderRequestIndicesToRender[i];
                            var renderRequest = renderRequests[renderRequestIndex];

                            var cmd = CommandBufferPool.Get("");
                            
                            // TODO: Avoid the intermediate target and render directly into final target
                            //  CommandBuffer.Blit does not work on Cubemap faces
                            //  So we use an intermediate RT to perform a CommandBuffer.CopyTexture in the target Cubemap face
                            if (renderRequest.target.face != CubemapFace.Unknown)
                            {
                                if (!m_TemporaryTargetForCubemaps.IsCreated())
                                    m_TemporaryTargetForCubemaps.Create();

                                var frpCamera = renderRequest.frpCamera;
                                ref var target = ref renderRequest.target;
                                target.id = m_TemporaryTargetForCubemaps;
                            }
                            
                            // TODO: handle dependent Probe data

                            #region Render AVOs

                            {
                                // TODO: render AOVs, for virtual multiple cameras support by composition system, no need for this?
                               
                            }
                            

                            #endregion


                            #region Render Requset
                            
                            using (new ProfilingScope(cmd, renderRequest.frpCamera.profilingSampler))
                            {
                                // TODO: camera settings
                                // cmd.SetInvertCulling(renderRequest.cameraSettings.invertFaceCulling);
                                ExecuteRenderRequest(renderRequest, renderContext, cmd/*, AOVRequestData.defaultAOVRequestDataNonAlloc*/);
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
            var target = renderRequest.target;
            
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
            out ScriptableCullingParameters cullingParams
        )
        {
            // First, get aggregate of frame settings base on global settings, camera frame settings and debug settings
            // Note: the SceneView camera will never have additionalCameraData
            additionalCameraData = FlowUtility.TryGetAdditionalCameraDataOrDefault(camera);
            cullingParams = default;

            if (!camera.TryGetCullingParameters(camera.stereoEnabled, out cullingParams))
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

        internal GraphicsFormat GetColorBufferFormat()
        {
            if (CoreUtils.IsSceneFilteringEnabled())
                return GraphicsFormat.R16G16B16A16_SFloat;

            return GraphicsFormat.R32G32B32A32_SFloat;
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