using System;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.SuperAdvanced
{
    public partial class SARenderPipeline
    {
        TextureHandle CreateSharedTextureBuffer(RenderGraph renderGraph, SACamera saCamera, TextureCreationData textureBufferData)
        {
            var textureBufferNode = textureBufferData.textureBuffer;
            switch (textureBufferData.lifeTime)
            {
                case SARenderGraphData.BufferLifeTime.PerPass:
                    throw new Exception("Transient Texture should not create here.");
                case SARenderGraphData.BufferLifeTime.PerCamera:
                    return renderGraph.CreateSharedTexture(BuildTextureDesc(textureBufferNode, saCamera));
                case SARenderGraphData.BufferLifeTime.PerFrame:
                    return renderGraph.CreateSharedTexture(BuildTextureDesc(textureBufferNode, saCamera));
                default:
                    return renderGraph.CreateSharedTexture(BuildTextureDesc(textureBufferNode, saCamera));
            }
        }
        
        TextureHandle CreateTransientTextureBuffer(RenderGraphBuilder builder, SACamera saCamera, TextureCreationData textureBufferData)
        {
            var textureBufferNode = textureBufferData.textureBuffer;
            switch (textureBufferData.lifeTime)
            {
                case SARenderGraphData.BufferLifeTime.PerPass:
                    return builder.CreateTransientTexture(BuildTextureDesc(textureBufferNode, saCamera));
                default:
                    throw new Exception("Shared Texture should not create here.");
            }
        }

        TextureDesc BuildTextureDesc(SARenderGraphData.TextureBufferNode textureBufferNode, SACamera saCamera)
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
                clearBuffer = textureBufferNode.clearBuffer || NeedClearColorBuffer(saCamera),
                clearColor = textureBufferNode.clearBuffer
                    ? textureBufferNode.clearColor
                    : GetColorBufferClearColor(saCamera),
                
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