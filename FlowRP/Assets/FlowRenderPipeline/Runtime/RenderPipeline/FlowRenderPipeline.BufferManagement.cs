using System;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.FlowPipeline
{
    public partial class FlowRenderPipeline
    {
        TextureHandle CreateSharedTextureBuffer(RenderGraph renderGraph, FRPCamera frpCamera, TextureCreationData textureBufferData)
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
        
        TextureHandle CreateTransientTextureBuffer(RenderGraphBuilder builder, FRPCamera frpCamera, TextureCreationData textureBufferData)
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