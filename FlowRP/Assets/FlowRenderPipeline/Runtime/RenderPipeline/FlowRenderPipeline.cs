using System.Collections.Generic;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.FlowRP
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
         * 
         */
        struct RenderRequest
        {
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
            /// Doing some preparation and validation work. cooooooooooooool.

#if UNITY_2021_1_OR_NEWER
            if (cameras.Count == 0)
#else
            if (cameras.Length == 0)
#endif
                return;


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

            // This syntax is awful and hostile to debugging, please don't use it... !!! So why you using it !!!??????????
            using (ListPool<RenderRequest>.Get(out List<RenderRequest> renderRequests))
            using (ListPool<int>.Get(out List<int> rootRenderRequestIndices))
            {
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
                    // TODO： deal with cullingParams
                    ScriptableCullingParameters cullingParams = default;
                    skipRequest = !TryCull(camera, renderContext, cullingParams, m_Asset, ref cullingResults);


                    /// Skip !!! Off duty !!!!!!!!!!!!!!!!!!
                    if (skipRequest)
                    {
                        // Submit render context and free pooled resources for this request
                        renderContext.Submit();
                        UnsafeGenericPool<FlowRPCullingResults>.Release(cullingResults);
                        UnityEngine.Rendering.RenderPipeline.EndCameraRendering(renderContext, camera);
                        continue;
                    }


                    /// oops!!!! we need to work more
                    ///
                    
                    
                    
                } // end of culling loop
                
                
                
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
                }
            }


            // TODO: Now that all cameras have been rendered, let's make sure to keep track of update the screen space shadow data

            m_RenderGraph.EndFrame();
            m_CameraSystem.ReleaseFrame();

#if UNITY_2021_1_OR_NEWER
            EndContextRendering(renderContext, cameras);
#else
            EndFrameRendering(renderContext, cameras);
#endif
        }


        /////////////// Dispose !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        protected override void Dispose(bool disposing)
        {
            m_CameraSystem.Cleanup();
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

        #endregion
    }
}