using System;
using System.Collections.Generic;
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


        class DrawRendererPassData
        {
            public RendererListHandle rendererList;
        }
        
        public struct TextureCreationData
        {
            public FlowRenderGraphData.TextureBufferNode textureBuffer;
            public FlowRenderGraphData.BufferLifeTime lifeTime;
        }

        private int m_FrameIndex = 0;
        
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
         
            return CreateOpaqueRendererListDesc(cullResults, frpCamera.camera, new ShaderTagId[] {}, PerObjectData.Lightmaps);
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
            switch (renderRequest.nodeType)
            {
                case FlowRenderGraphData.NodeType.DrawRendererNode:
                    ExecuteDrawRendererPass(renderRequest, renderContext, commandBuffer);
                    break;
                default:
                    ExecuteEmptyPass(renderRequest, renderContext, commandBuffer);
                    break;
            }
        }

        void ExecuteEmptyPass(RenderRequest renderRequest,
            ScriptableRenderContext renderContext,
            CommandBuffer commandBuffer)
        {
            
        }
        
        void ExecuteDrawRendererPass(RenderRequest renderRequest,
            ScriptableRenderContext renderContext,
            CommandBuffer commandBuffer)
        {
            var graphData = renderRequest.currentGraphData;
            FlowRenderGraphData.DrawRendererNode drawRendererPass = graphData.TryGetDrawRendererPassNode(renderRequest.nodeID);

            FlowRenderGraphData.CullingParameterNode cullingParameterNode =
                string.IsNullOrEmpty(drawRendererPass.culling)
                    ? FlowUtility.k_DefaultCullingNode
                    : graphData.TryGetCullingParameterNode(drawRendererPass.culling);
            
            
            using (m_RenderGraph.RecordAndExecute(new RenderGraphParameters
                   {
                       executionName = renderRequest.frpCamera.name,
                       currentFrameIndex = m_FrameIndex++,
                       rendererListCulling = cullingParameterNode.isAllowRendererCulling,
                       scriptableRenderContext = renderContext,
                       commandBuffer = commandBuffer
                   }))
            {
                RecordDrawRenderer(renderRequest, renderContext, commandBuffer, drawRendererPass, cullingParameterNode);
            }
        }

        void RecordDrawRenderer( 
            RenderRequest renderRequest,
            ScriptableRenderContext renderContext,
            CommandBuffer commandBuffer,
            FlowRenderGraphData.DrawRendererNode drawRendererPass,
            FlowRenderGraphData.CullingParameterNode cullingParameterNode
        )
        {
            var graphData = renderRequest.currentGraphData;
            
            FlowRenderGraphData.RenderStateNode renderStateNode = string.IsNullOrEmpty(drawRendererPass.state)
                ? FlowUtility.k_DefaultRenderStateNode
                : graphData.TryGetRenderStateNode(drawRendererPass.state);

            FlowRenderGraphData.MaterialParameterNode materialParameterNode =
                string.IsNullOrEmpty(drawRendererPass.material)
                    ? FlowUtility.k_DefaultMaterialNode
                    : graphData.TryGetMaterialParameterNode(drawRendererPass.material);

            FlowRenderGraphData.CameraParameterNode cameraParameterNode = string.IsNullOrEmpty(drawRendererPass.camera)
                ? FlowUtility.k_DefaultCameraOverrideNode
                : graphData.TryGetCameraParameterNode(drawRendererPass.camera);

            using (new ProfilingScope(commandBuffer, ProfilingSampler.Get(ProfileId.RecordRenderGraph)))
            {
                var frpCamera = renderRequest.frpCamera;
                
                // DrawRenderer(m_RenderGraph, frpCamera, 
                //     drawRendererPass, 
                //     cullingParameterNode, 
                //     renderStateNode);
                
                RenderGizmos(m_RenderGraph, frpCamera, GizmoSubset.PostImageEffects);
            }
        }

        void DrawRenderer(
            RenderGraph renderGraph, 
            FRPCamera frpCamera, 
            FlowRenderGraphData.DrawRendererNode drawRendererNode, 
            FlowRenderGraphData.CullingParameterNode cullingParameterNode,
            FlowRenderGraphData.RenderStateNode renderStateNode,
            FlowRenderGraphData.MaterialParameterNode materialParameterNode,
            FlowRenderGraphData.CameraParameterNode cameraParameterNode,
            CullingResults cullingResults
            )
        {
            var debugDisplay = false;
            // using (var builder = renderGraph.AddRenderPass<DrawRendererPassData>(
            //            debugDisplay ? renderRequest.name : renderRequest.name + " Debug",
            //            out var passData))
            // {
            //     
            // }
        }
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
                           ? ProfilingSampler.Get(ProfileId.DrawRendererDebug)
                           : ProfilingSampler.Get(ProfileId.DrawRenderer)))
            {
                
                builder.AllowPassCulling(false);
                builder.AllowRendererListCulling(false);
                
                PrepareCommonForwardPassData(renderGraph, builder, passData, true, PrepareForwardOpaqueRendererList(cullResults, frpCamera));

                builder.UseColorBuffer(colorBuffer, 0);
                
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


        TextureHandle CreateSharedColorBuffer(RenderGraph renderGraph, FRPCamera frpCamera, TextureCreationData textureBufferData)
        {
            var textureBufferNode = textureBufferData.textureBuffer;
            switch (textureBufferData.lifeTime)
            {
                case FlowRenderGraphData.BufferLifeTime.PerPass:
                    throw new Exception("Transient Texture should not create here.");
                case FlowRenderGraphData.BufferLifeTime.PerCamera:
                    return renderGraph.CreateSharedTexture(BuildTextureDesc(textureBufferNode, frpCamera));
                case FlowRenderGraphData.BufferLifeTime.PerFrame:
                    return renderGraph.CreateSharedTexture(BuildTextureDesc(textureBufferNode, frpCamera));
                default:
                    return renderGraph.CreateSharedTexture(BuildTextureDesc(textureBufferNode, frpCamera));
            }
        }
        
        TextureHandle CreateTransientColorBuffer(RenderGraphBuilder builder, FRPCamera frpCamera, TextureCreationData textureBufferData)
        {
            var textureBufferNode = textureBufferData.textureBuffer;
            switch (textureBufferData.lifeTime)
            {
                case FlowRenderGraphData.BufferLifeTime.PerPass:
                   return builder.CreateTransientTexture(BuildTextureDesc(textureBufferNode, frpCamera));
                default:
                    throw new Exception("Shared Texture should not create here.");
            }
        }

        TextureDesc BuildTextureDesc(FlowRenderGraphData.TextureBufferNode textureBufferNode, FRPCamera frpCamera)
        {
#if UNITY_2020_2_OR_NEWER
            FastMemoryDesc colorFastMemDesc;
            colorFastMemDesc.flags = textureBufferNode.fastMemoryDesc.flags;
            colorFastMemDesc.inFastMemory = textureBufferNode.fastMemoryDesc.inFastMemory;
            colorFastMemDesc.residencyFraction = textureBufferNode.fastMemoryDesc.residencyFraction;
#endif
            
            return new TextureDesc()
            {
                // header
                name = textureBufferNode.name,
                isShadowMap = textureBufferNode.isShadowMap,
                fallBackToBlackTexture = textureBufferNode.fallBackToBlackTexture,
                
                // size 
                sizeMode = textureBufferNode.sizeMode,
                width = textureBufferNode.width,
                height = textureBufferNode.height,
                scale = textureBufferNode.scale,
                useDynamicScale = textureBufferNode.useDynamicScale,
                
                // init state
                clearBuffer = textureBufferNode.clearBuffer || NeedClearColorBuffer(frpCamera),
                clearColor = textureBufferNode.clearBuffer
                    ? textureBufferNode.clearColor
                    : GetColorBufferClearColor(frpCamera),
                
                // format
                colorFormat = GetColorBufferFormat(textureBufferNode),
                depthBufferBits = textureBufferNode.depthBits,
                dimension = textureBufferNode.dimension,

                // filter & addressing
                filterMode = textureBufferNode.filterMode,
                wrapMode = textureBufferNode.wrapMode,
                anisoLevel = textureBufferNode.anisoLevel,
                
                // mip
                useMipMap = textureBufferNode.useMipMap,
                autoGenerateMips = textureBufferNode.autoGenerateMips,
                mipMapBias = textureBufferNode.mipMapBias,
                
                // memory
                enableRandomWrite = textureBufferNode.enableRandomWrite,
                memoryless = textureBufferNode.memoryless,
#if UNITY_2020_2_OR_NEWER
                fastMemoryDesc = colorFastMemDesc,
#endif
                // msaa
                bindTextureMS = textureBufferNode.bindTextureMS,
                msaaSamples = textureBufferNode.msaaSamples,
                
            };
        }
    }
}
