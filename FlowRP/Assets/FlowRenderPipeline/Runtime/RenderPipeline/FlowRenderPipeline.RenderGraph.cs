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

        
        public class DrawRendererPassData: IDisposable
        {
            public struct TextureHandleData
            {
                public TextureHandle handle;
                public string name;
            }
            public RendererListHandle rendererListHandle;
            public List<TextureHandleData> readTextures;
            public List<TextureHandleData> writeTextures;

            public DrawRendererPassData()
            {
                readTextures = new List<TextureHandleData>();
                writeTextures = new List<TextureHandleData>();
            }

            public void Dispose()
            {
                readTextures.Clear();
                writeTextures.Clear();
            }
        }
        
        public struct TextureCreationData
        {
            public FlowRenderGraphData.TextureBufferNode textureBuffer;
            public FlowRenderGraphData.BufferLifeTime lifeTime;
        }
        
        public struct DrawRendererData
        {
            public FlowRenderGraphData.DrawRendererNode drawRendererNode;
            public FlowRenderGraphData.CullingParameterNode cullingParameterNode;
            public FlowRenderGraphData.RenderStateNode renderStateNode;
            public FlowRenderGraphData.MaterialParameterNode materialParameterNode;
            public FlowRenderGraphData.CameraParameterNode cameraParameterNode;
            public CullingResults cullingResults;
            public FlowRenderGraphData currentGraphData;
            public FRPCamera frpCamera;
        }

        private int m_FrameIndex = 0;
        
        #endregion

        DrawRendererPassData.TextureHandleData ReadTexture(RenderGraph renderGraph, RenderGraphBuilder builder, FlowRenderGraphData graphData, FlowRenderGraphData.BufferNode bufferNode, FRPCamera frpCamera)
        {
            var textureBuffer = graphData.TryGetTextureBufferNode(bufferNode.bufferID);
            if (bufferNode.lifeTime == FlowRenderGraphData.BufferLifeTime.PerPass)
            {
                return new DrawRendererPassData.TextureHandleData()
                {
                    handle =  builder.ReadTexture(CreateTransientTextureBuffer(builder, frpCamera, new TextureCreationData()
                    {
                        lifeTime = bufferNode.lifeTime,
                        textureBuffer = textureBuffer
                    })),
                    name = textureBuffer.name
                };
            }
            else
            {
               return new DrawRendererPassData.TextureHandleData()
               {
                   handle = builder.ReadTexture(CreateSharedTextureBuffer(renderGraph, frpCamera, new TextureCreationData()
                   {
                       lifeTime = bufferNode.lifeTime,
                       textureBuffer = textureBuffer
                   })),
                   name = textureBuffer.name
               };
            }
        }

        // TODO: need to split into computer buffer (using "builder.write" interface ?) and texture buffer (using builder.use interface ?) type, 
        DrawRendererPassData.TextureHandleData WriteTexture(RenderGraph renderGraph, RenderGraphBuilder builder, FlowRenderGraphData graphData, FlowRenderGraphData.BufferNode bufferNode, FRPCamera frpCamera, int index = 0)
        {
            var textureBuffer = graphData.TryGetTextureBufferNode(bufferNode.bufferID);
            if (bufferNode.lifeTime == FlowRenderGraphData.BufferLifeTime.PerPass)
            {
                return new DrawRendererPassData.TextureHandleData()
                {
                    handle =  builder.UseColorBuffer(CreateTransientTextureBuffer(builder, frpCamera, new TextureCreationData()
                    {
                        lifeTime = bufferNode.lifeTime,
                        textureBuffer = textureBuffer
                    }), index),
                    name = textureBuffer.name
                };
            }
            else
            {
                return new DrawRendererPassData.TextureHandleData()
                {
                    handle = builder.UseColorBuffer(CreateSharedTextureBuffer(renderGraph, frpCamera, new TextureCreationData()
                    {
                        lifeTime = bufferNode.lifeTime,
                        textureBuffer = textureBuffer
                    }), index),
                    name = textureBuffer.name
                };
            }
        }
            
        void PrepareDrawRendererPassData(
            RenderGraph renderGraph,
            RenderGraphBuilder builder,
            DrawRendererPassData data,
            RendererListDesc rendererListDesc,
            DrawRendererData drawRendererData)
        {

            var graphData = drawRendererData.currentGraphData;
            // set renderer List
            {
                data.rendererListHandle =  builder.UseRendererList(renderGraph.CreateRendererList(rendererListDesc));
            }

            // set inputBuffer List
            {
                var drawRendererPassData = drawRendererData.drawRendererNode;
                for (int i = 0; i < drawRendererPassData.inputList.Count; ++i)
                {
                    var bufferID = drawRendererPassData.inputList[i];
                    if (string.IsNullOrEmpty(bufferID))
                    {
                        continue;
                    }

                    var bufferNode = graphData.TryGetBufferNode(bufferID);
                    if (bufferNode.bufferType == FlowRenderGraphData.BufferType.TextureBuffer)
                    {
                        data.readTextures.Add(ReadTexture(renderGraph, builder, graphData, bufferNode, drawRendererData.frpCamera));
                        
                    } else if (bufferNode.bufferType == FlowRenderGraphData.BufferType.ComputerBuffer)
                    {
                        // not support yet
                    }
                }
            }
            
            // set outputBuffer List
            {
                var drawRendererPassData = drawRendererData.drawRendererNode;
                for (int i = 0; i < drawRendererPassData.outputList.Count; ++i)
                {
                    var bufferID = drawRendererPassData.outputList[i];
                    if (string.IsNullOrEmpty(bufferID))
                    {
                        continue;
                    }

                    var bufferNode = graphData.TryGetBufferNode(bufferID);
                    if (bufferNode.bufferType == FlowRenderGraphData.BufferType.TextureBuffer)
                    {
                        data.writeTextures.Add(WriteTexture(renderGraph, builder, graphData, bufferNode, drawRendererData.frpCamera, i));
                        
                    } else if (bufferNode.bufferType == FlowRenderGraphData.BufferType.ComputerBuffer)
                    {
                        // not support yet
                    }
                }
            }
            
            // parameter and keywords

        }

        RenderTargetBlendState CreateRenderTargetBlendState(FlowRenderGraphData.BlendStateData blendStateData)
        {
            return new RenderTargetBlendState()
            {
                writeMask = blendStateData.writeMask,
                sourceColorBlendMode = blendStateData.sourceColorBlendMode,
                destinationColorBlendMode = blendStateData.destinationColorBlendMode,
                sourceAlphaBlendMode = blendStateData.sourceAlphaBlendMode,
                destinationAlphaBlendMode = blendStateData.destinationAlphaBlendMode,
                colorBlendOperation = blendStateData.colorBlendOperation,
                alphaBlendOperation = blendStateData.alphaBlendOperation
            };
        }
        void SetBlendState(ref BlendState blendState, List<FlowRenderGraphData.BlendStateData> blendStateDatas)
        {
            switch (blendStateDatas.Count)
            {
                case 0:
                    blendState.blendState0 = FlowUtility.k_DefaultRenderTargetBlendState;
                    break;
                case 1:
                    blendState.blendState0 = CreateRenderTargetBlendState(blendStateDatas[0]);
                    break;
                case 2:
                    blendState.blendState0 = CreateRenderTargetBlendState(blendStateDatas[0]);
                    blendState.blendState1 = CreateRenderTargetBlendState(blendStateDatas[1]);
                    break;
                case 3:
                    blendState.blendState0 = CreateRenderTargetBlendState(blendStateDatas[0]);
                    blendState.blendState1 = CreateRenderTargetBlendState(blendStateDatas[1]);
                    blendState.blendState2 = CreateRenderTargetBlendState(blendStateDatas[2]);
                    break;
                case 4:
                    blendState.blendState0 = CreateRenderTargetBlendState(blendStateDatas[0]);
                    blendState.blendState1 = CreateRenderTargetBlendState(blendStateDatas[1]);
                    blendState.blendState2 = CreateRenderTargetBlendState(blendStateDatas[2]);
                    blendState.blendState3 = CreateRenderTargetBlendState(blendStateDatas[3]);
                    break;
                case 5:
                    blendState.blendState0 = CreateRenderTargetBlendState(blendStateDatas[0]);
                    blendState.blendState1 = CreateRenderTargetBlendState(blendStateDatas[1]);
                    blendState.blendState2 = CreateRenderTargetBlendState(blendStateDatas[2]);
                    blendState.blendState3 = CreateRenderTargetBlendState(blendStateDatas[3]);
                    
                    blendState.blendState4 = CreateRenderTargetBlendState(blendStateDatas[4]);
                    break;
                case 6:
                    blendState.blendState0 = CreateRenderTargetBlendState(blendStateDatas[0]);
                    blendState.blendState1 = CreateRenderTargetBlendState(blendStateDatas[1]);
                    blendState.blendState2 = CreateRenderTargetBlendState(blendStateDatas[2]);
                    blendState.blendState3 = CreateRenderTargetBlendState(blendStateDatas[3]);
                    
                    blendState.blendState4 = CreateRenderTargetBlendState(blendStateDatas[4]);
                    blendState.blendState5 = CreateRenderTargetBlendState(blendStateDatas[5]);
                    break;
                case 7:
                    blendState.blendState0 = CreateRenderTargetBlendState(blendStateDatas[0]);
                    blendState.blendState1 = CreateRenderTargetBlendState(blendStateDatas[1]);
                    blendState.blendState2 = CreateRenderTargetBlendState(blendStateDatas[2]);
                    blendState.blendState3 = CreateRenderTargetBlendState(blendStateDatas[3]);
                    
                    blendState.blendState4 = CreateRenderTargetBlendState(blendStateDatas[4]);
                    blendState.blendState5 = CreateRenderTargetBlendState(blendStateDatas[5]);
                    blendState.blendState6 = CreateRenderTargetBlendState(blendStateDatas[6]);
                    break;
                // 8 targets or more ... 8 is the max limit
                default:
                    blendState.blendState0 = CreateRenderTargetBlendState(blendStateDatas[0]);
                    blendState.blendState1 = CreateRenderTargetBlendState(blendStateDatas[1]);
                    blendState.blendState2 = CreateRenderTargetBlendState(blendStateDatas[2]);
                    blendState.blendState3 = CreateRenderTargetBlendState(blendStateDatas[3]);
                    
                    blendState.blendState4 = CreateRenderTargetBlendState(blendStateDatas[4]);
                    blendState.blendState5 = CreateRenderTargetBlendState(blendStateDatas[5]);
                    blendState.blendState6 = CreateRenderTargetBlendState(blendStateDatas[6]);
                    blendState.blendState7 = CreateRenderTargetBlendState(blendStateDatas[7]);
                    break;
            }
        }
        
        RendererListDesc PrepareDrawRendererRendererList(FRPCamera frpCamera, DrawRendererData drawRendererData)
        {
            // tags
            var shaderTagIds = new List<ShaderTagId>();
            foreach (var tagName in drawRendererData.materialParameterNode.shaderTagList)
            {
                shaderTagIds.Add(new ShaderTagId(tagName));
            }
            
            // state block
            var renderStateBlock = new RenderStateBlock();
            renderStateBlock.rasterState = new RasterState()
            {
                cullingMode = drawRendererData.renderStateNode.rasterState.cullingMode,
                depthClip = drawRendererData.renderStateNode.rasterState.depthClip,
                offsetFactor = drawRendererData.renderStateNode.rasterState.offsetFactor,
                offsetUnits = drawRendererData.renderStateNode.rasterState.offsetUnits
            };

            renderStateBlock.depthState = new DepthState()
            {
                writeEnabled = drawRendererData.renderStateNode.depthState.writeEnabled,
                compareFunction = drawRendererData.renderStateNode.depthState.compareFunction
            };

            renderStateBlock.stencilState = new StencilState()
            {
                enabled = drawRendererData.renderStateNode.stencilState.enabled,
                readMask = drawRendererData.renderStateNode.stencilState.readMask,
                writeMask = drawRendererData.renderStateNode.stencilState.writeMask,
                compareFunctionFront = drawRendererData.renderStateNode.stencilState.compareFunctionFront,
                passOperationFront = drawRendererData.renderStateNode.stencilState.passOperationFront,
                failOperationFront = drawRendererData.renderStateNode.stencilState.failOperationFront,
                zFailOperationFront = drawRendererData.renderStateNode.stencilState.zFailOperationFront,
                compareFunctionBack = drawRendererData.renderStateNode.stencilState.compareFunctionBack,
                passOperationBack = drawRendererData.renderStateNode.stencilState.passOperationBack,
                failOperationBack = drawRendererData.renderStateNode.stencilState.failOperationBack,
                zFailOperationBack = drawRendererData.renderStateNode.stencilState.zFailOperationBack
            };

            var blendState = new BlendState()
            {
                separateMRTBlendStates = drawRendererData.renderStateNode.blendStateSettings.blendStates.Count > 1,
                alphaToMask = drawRendererData.renderStateNode.blendStateSettings.alphaToMask
            };

            SetBlendState(ref blendState, drawRendererData.renderStateNode.blendStateSettings.blendStates);
            
            return CreateDrawRendererListDesc(
                drawRendererData.cullingResults,
                frpCamera.camera, 
                shaderTagIds.ToArray(),
                drawRendererData.cullingParameterNode.perObjectData,
                new RenderQueueRange((int)drawRendererData.materialParameterNode.renderQueueRange.start, (int)drawRendererData.materialParameterNode.renderQueueRange.end),
                renderStateBlock,
               drawRendererData.materialParameterNode.overrideMaterial);
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
                
                DrawRenderer(m_RenderGraph, frpCamera, new DrawRendererData()
                {
                    drawRendererNode = drawRendererPass, 
                    cullingParameterNode = cullingParameterNode, 
                    renderStateNode = renderStateNode,
                    materialParameterNode = materialParameterNode,
                    cameraParameterNode = cameraParameterNode,
                    cullingResults = renderRequest.cullingResults.cullingResults,
                    currentGraphData = renderRequest.currentGraphData,
                    frpCamera = renderRequest.frpCamera
                });
                
                RenderGizmos(m_RenderGraph, frpCamera, GizmoSubset.PostImageEffects);
            }
        }

        void DrawRenderer(
            RenderGraph renderGraph, 
            FRPCamera frpCamera,
            DrawRendererData drawRendererData
            )
        {
            var debugDisplay = false;

            var drawRendererNode = drawRendererData.drawRendererNode;
            var cullingParameterNode = drawRendererData.cullingParameterNode;
            
            using (var builder = renderGraph.AddRenderPass<DrawRendererPassData>(
                       debugDisplay ? drawRendererNode.name : drawRendererNode.name + " Debug",
                       out var passData,
                       debugDisplay
                           ? ProfilingSampler.Get(ProfileId.DrawRendererDebug)
                           : ProfilingSampler.Get(ProfileId.DrawRenderer)))
            {
                builder.AllowPassCulling(cullingParameterNode.isAllowPassCulling);
                builder.AllowRendererListCulling(cullingParameterNode.isAllowRendererCulling);

                PrepareDrawRendererPassData(renderGraph, builder, passData, PrepareDrawRendererRendererList(frpCamera, drawRendererData), drawRendererData);

                builder.SetRenderFunc((DrawRendererPassData data, RenderGraphContext context) =>
                {
                    var cmd = context.cmd;
                    // set keywords
                    // set parameters
                    // set textures
                    
                    CoreUtils.DrawRendererList(context.renderContext, cmd, data.rendererListHandle);
                });
            }
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
//             var debugDisplay = false;
//             using (var builder = renderGraph.AddRenderPass<ForwardOpaquePassData>(
//                        debugDisplay ? "Forward (+ Emissive) Opaque  Debug" : "Forward (+ Emissive) Opaque",
//                        out var passData,
//                        debugDisplay
//                            ? ProfilingSampler.Get(ProfileId.DrawRendererDebug)
//                            : ProfilingSampler.Get(ProfileId.DrawRenderer)))
//             {
//                 
//                 builder.AllowPassCulling(false);
//                 builder.AllowRendererListCulling(false);
//                 
//                
//
//                 builder.UseColorBuffer(colorBuffer, 0);
//                 
// #if ENABLE_VIRTUALTEXTURES
//                 builder.UseColorBuffer(vtFeedbackBuffer, index++);
// #endif
//                 builder.SetRenderFunc((ForwardOpaquePassData data, RenderGraphContext context) =>
//                 {
//                     RenderForwardRendererList(/*data.frameSettings, */data.rendererList, true, context.renderContext, context.cmd);
//                 });
            // }
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
               // DrawOpaqueRendererList(renderContext, cmd/*, frameSettings*/, rendererList);
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
        
    }
}
