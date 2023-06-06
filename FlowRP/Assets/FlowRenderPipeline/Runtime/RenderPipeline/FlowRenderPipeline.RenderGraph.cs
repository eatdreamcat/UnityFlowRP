using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RendererUtils;


// Resove the ambiguity in the RendererList name (pick the in-engine version)
using RendererList = UnityEngine.Rendering.RendererUtils.RendererList;
using RendererListDesc = UnityEngine.Rendering.RendererUtils.RendererListDesc;


namespace UnityEngine.Rendering.FlowPipeline
{
    public partial class FlowRenderPipeline
    {

        #region Data

        class ForwardPassData
        {
            public RendererListHandle rendererList;
            public ComputeBufferHandle lightListTile;
            public ComputeBufferHandle lightListCluster;

            public ComputeBufferHandle perVoxelOffset;
            public ComputeBufferHandle perTileLogBaseTweak;
            // public FrameSettings frameSettings;
        }
        
        class ForwardOpaquePassData : ForwardPassData
        {
            // public DBufferOutput dbuffer;
            // public LightingBuffers lightingBuffers;
            public bool enableDecals;
        }
        
        
        #endregion

        void PrepareCommonForwardPassData(
            RenderGraph renderGraph,
            RenderGraphBuilder builder,
            ForwardPassData data,
            bool opaque,
           /* FrameSettings frameSettings,*/
            RendererListDesc rendererListDesc /*,
           in BuildGPULightListOutput lightLists,
            ShadowResult shadowResult*/)
        {
        
            data.rendererList =  builder.UseRendererList(renderGraph.CreateRendererList(rendererListDesc));
        }

        RendererListDesc PrepareForwardOpaqueRendererList(CullingResults cullResults, FRPCamera frpCamera)
        {
            // var passNames = frpCamera.frameSettings.litShaderMode == LitShaderMode.Forward
            //     ? m_ForwardAndForwardOnlyPassNames
            //     : m_ForwardOnlyPassNames;
            var passNames = m_ForwardAndForwardOnlyPassNames;
            return CreateOpaqueRendererListDesc(cullResults, frpCamera.camera, passNames, m_CurrentRendererConfigurationBakedLighting);
        }
        
        
        /// <summary>
        /// Execute render graph
        /// </summary>
        /// <param name="renderRequest"></param>
        /// <param name="renderContext"></param>
        /// <param name="commandBuffer"></param>
        void ExecuteWithRenderGraph(RenderRequest renderRequest,
            ScriptableRenderContext renderContext,
            CommandBuffer commandBuffer)
        {
            using (m_RenderGraph.RecordAndExecute(new RenderGraphParameters
                   {
                       executionName = renderRequest.frpCamera.name,
                       currentFrameIndex = 0,
                       rendererListCulling = false,
                       scriptableRenderContext = renderContext,
                       commandBuffer = commandBuffer
                   }))
            {
                RecordRenderGraph(renderRequest, renderContext, commandBuffer);
            }
        }

        /// <summary>
        /// Setup render request list
        /// </summary>
        /// <param name="renderRequest"></param>
        /// <param name="aovRequest"></param>
        /// <param name="aovBuffers"></param>
        /// <param name="aovCustomPassBuffers"></param>
        /// <param name="renderContext"></param>
        /// <param name="commandBuffer"></param>
        void RecordRenderGraph(RenderRequest renderRequest,
            ScriptableRenderContext renderContext,
            CommandBuffer commandBuffer)
        {
            using (new ProfilingScope(commandBuffer, ProfilingSampler.Get(ProfileId.RecordRenderGraph)))
            {
                var frpCamera = renderRequest.frpCamera;
                
                TextureHandle colorBuffer = CreateColorBuffer(m_RenderGraph, false, frpCamera , true);

                RenderForwardOpaque(m_RenderGraph, frpCamera, colorBuffer, renderRequest.cullingResults.cullingResults);
                
                RenderGizmos(m_RenderGraph, frpCamera, GizmoSubset.PostImageEffects);
            }
        }

        
        // Guidelines: In deferred by default there is no opaque in forward. However it is possible to force an opaque material to render in forward
        // by using the pass "ForwardOnly". In this case the .shader should not have "Forward" but only a "ForwardOnly" pass.
        // It must also have a "DepthForwardOnly" and no "DepthOnly" pass as forward material (either deferred or forward only rendering) have always a depth pass.
        // The RenderForward pass will render the appropriate pass depends on the engine settings. In case of forward only rendering, both "Forward" pass and "ForwardOnly" pass
        // material will be render for both transparent and opaque. In case of deferred, both path are used for transparent but only "ForwardOnly" is use for opaque.
        // (Thus why "Forward" and "ForwardOnly" are exclusive, else they will render two times"
        void RenderForwardOpaque(RenderGraph renderGraph,
            FRPCamera frpCamera,
            TextureHandle colorBuffer, 
            /*in LightingBuffers lightingBuffers,
            in BuildGPULightListOutput lightLists,
            in PrepassOutput prepassOutput,
            TextureHandle vtFeedbackBuffer,
            ShadowResult shadowResult,*/
            CullingResults cullResults)
        {
            var debugDisplay = false;
            using (var builder = renderGraph.AddRenderPass<ForwardOpaquePassData>(
                       debugDisplay ? "Forward (+ Emissive) Opaque  Debug" : "Forward (+ Emissive) Opaque",
                       out var passData,
                       debugDisplay
                           ? ProfilingSampler.Get(ProfileId.ForwardOpaqueDebug)
                           : ProfilingSampler.Get(ProfileId.ForwardOpaque)))
            {
                
                PrepareCommonForwardPassData(renderGraph, builder, passData, true, PrepareForwardOpaqueRendererList(cullResults, frpCamera));
                
                
                int index = 0;
                builder.UseColorBuffer(colorBuffer, index++);
                builder.UseDepthBuffer(colorBuffer, DepthAccess.Read);
#if ENABLE_VIRTUALTEXTURES
                builder.UseColorBuffer(vtFeedbackBuffer, index++);
#endif
                builder.SetRenderFunc((ForwardOpaquePassData data, RenderGraphContext context) =>
                {
                    RenderForwardRendererList(/*data.frameSettings, */data.rendererList, true, context.renderContext, context.cmd);
                });
            }
        }

        static void RenderForwardRendererList(
            /*FrameSettings frameSettings,*/
            RendererList rendererList,
            bool opaque,
            ScriptableRenderContext renderContext,
            CommandBuffer cmd)
        {
            if (opaque)
            {
                DrawOpaqueRendererList(renderContext, cmd/*, frameSettings*/, rendererList);
            }
            else
            {
                //  DrawTransparentRendererList(renderContext, cmd/*, frameSettings, rendererList*/);
            }
        }

        void RenderGizmos(RenderGraph renderGraph, FRPCamera frpCamera, GizmoSubset gizmoSubset)
        {
#if UNITY_EDITOR
            
#endif
        }


        TextureHandle CreateColorBuffer(RenderGraph renderGraph, bool msaa, FRPCamera frpCamera,
            bool fallbackToBlack = false)
        {
#if UNITY_2020_2_OR_NEWER
            FastMemoryDesc colorFastMemDesc;
            colorFastMemDesc.inFastMemory = true;
            colorFastMemDesc.residencyFraction = 1.0f;
            colorFastMemDesc.flags = FastMemoryFlags.SpillTop;
#endif
            return renderGraph.CreateTexture(
                new TextureDesc(Vector2.one, false, false)
                {
                    colorFormat = GetColorBufferFormat(),
                    depthBufferBits = 0,
                    enableRandomWrite = !msaa,
                    bindTextureMS = msaa,
                    msaaSamples = MSAASamples.None,
                    clearBuffer = NeedClearColorBuffer(frpCamera),
                    clearColor = GetColorBufferClearColor(frpCamera),
                    name = msaa ? "CameraColorMSAA" : "CameraColor",
                    fallBackToBlackTexture = fallbackToBlack
#if UNITY_2020_2_OR_NEWER
                    , fastMemoryDesc = colorFastMemDesc
#endif
                });
        }
    }
}
