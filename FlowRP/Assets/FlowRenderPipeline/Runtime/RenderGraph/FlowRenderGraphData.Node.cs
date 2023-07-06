using System;
using System.Collections.Generic;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.UIElements;

namespace UnityEngine.Rendering.FlowPipeline
{
    public partial class FlowRenderGraphData
    {
        [Serializable]
        public class BaseNode
        {
            /* using for tracking layout data*/
            public string guid;
            public string name;
            public NodeType type;
        }

        [Serializable]
        public class FlowNode : BaseNode
        {
            // for a flow node , it only contains flow information, data id is used to bind a extra data , like render request data
            public string dataID;
            public NodeType dataType;
            
            public List<string> flowIn;
            public List<string> flowOut;
        }

        public FlowNode CreateFlowNode(string nodeName, string dataID, NodeType dataType)
        {
            return new FlowNode()
            {
                name = nodeName,
                guid = dataID,
                dataType = dataType,
                dataID = dataID,
                type = NodeType.Flow,
                flowIn = new List<string>(),
                flowOut = new List<string>()
            };
        }

        [Serializable]
        public class BranchNode : FlowNode
        {
            
        }
        
        [Serializable]
        public class LoopNode : FlowNode
        {
            public int loopCount;
        }
        
        [Serializable]
        public class EntryNode : BaseNode
        {
            public string startPoint;
        }
        
        public static EntryNode CreateEntryNode(string name, string guid)
        {
            return new EntryNode()
            {
                name = name,
                guid = guid,
                type = NodeType.EntryNode,
                startPoint = ""
            };
        }
        
        
        [Serializable]
        public class DrawRendererNode : BaseNode
        {
            public string culling;
            public string state;
            public string material;
            public string camera;

            public List<string> inputList;
            public List<string> outputList;
        }
        
        public static DrawRendererNode CreateRenderRequestNode(string name, string guid)
        {
            return new DrawRendererNode()
            {
                name = name,
                guid = guid,
                type = NodeType.DrawRendererNode,

                inputList = new List<string>(),
                outputList = new List<string>()
            };
        }

        [Serializable]
        public class DrawFullScreenNode : BaseNode
        {
            public string state;
            public Shader shader;
            public List<string> shaderTagList;
            
            public List<string> inputList;
            public List<string> outputList;
        }

        public static DrawFullScreenNode CreateDrawFullScreenNode(string name, string guid)
        {
            return new DrawFullScreenNode()
            {
                name = name,
                guid = guid,
                type = NodeType.DrawFullScreenNode,
                
                inputList = new List<string>(),
                outputList = new List<string>()
            };
        }

        [Serializable]
        public class CullingParameterNode : BaseNode
        {
            public bool isAllowPassCulling;
            public bool isAllowRendererCulling;
            public LayerMask cullingMask;
            public PerObjectData perObjectData;
        }

        public static CullingParameterNode CreateCullingParameterNode(string name, string guid)
        {
            return new CullingParameterNode()
            {
                name = name,
                guid = guid,
                type = NodeType.CullingParameterNode,
                isAllowPassCulling = true,
                isAllowRendererCulling = true,
                cullingMask = -1,
                perObjectData = 0,
            };
        }
        
        [Serializable]
        public struct RasterStateData
        {
            public CullMode cullingMode;
            public int offsetUnits;
            public float offsetFactor;
            public bool depthClip;
        }
        
        [Serializable]
        public struct DepthStateData
        {
            public bool writeEnabled;
            public CompareFunction compareFunction;
        }
        
        [Serializable]
        public struct StencilStateData
        {
            public bool enabled;
            public byte readMask;
            public byte writeMask;
            
            public CompareFunction compareFunctionFront;
            public StencilOp passOperationFront;
            public StencilOp failOperationFront;
            public StencilOp zFailOperationFront;

            public CompareFunction compareFunctionBack;
            public StencilOp passOperationBack;
            public StencilOp failOperationBack;
            public StencilOp zFailOperationBack;
        }
        
        [Serializable]
        // chichi: we use class here to prevent the issue that when click ListView "+" button, it will raise a exception which I can't figure out yet. sad....
        public class BlendStateData
        {
            public ColorWriteMask writeMask;
            public BlendMode sourceColorBlendMode;
            public BlendMode destinationColorBlendMode;
            public BlendMode sourceAlphaBlendMode;
            public BlendMode destinationAlphaBlendMode;
            public BlendOp colorBlendOperation;
            public BlendOp alphaBlendOperation;
            
        }
        [Serializable]
        public class RenderStateNode : BaseNode
        {
            [SerializeField]
            public RasterStateData rasterState;
            [SerializeField]
            public DepthStateData depthState;
            [SerializeField]
            public StencilStateData stencilState;
            [SerializeField]
            // support for mrt
            public List<BlendStateData> blendStates;
        }

        public static RenderStateNode CreateRenderStateNode(string name, string guid)
        {
            return new RenderStateNode()
            {
                name = name,
                guid = guid,
                type = NodeType.RenderStateNode,
                rasterState = default,
                depthState = default,
                stencilState = default,
                blendStates = new List<BlendStateData>()
            };
        }

        public enum Queue {
            Start = 0,
            /// <summary>
            ///   <para>This render queue is rendered before any others.</para>
            /// </summary>
            Background = 1000, // 0x000003E8
            /// <summary>
            ///   <para>Opaque geometry uses this queue.</para>
            /// </summary>
            Geometry = 2000, // 0x000007D0
            /// <summary>
            ///   <para>Alpha tested geometry uses this queue.</para>
            /// </summary>
            AlphaTest = 2450, // 0x00000992
            /// <summary>
            ///   <para>Last render queue that is considered "opaque".</para>
            /// </summary>
            GeometryLast = 2500, // 0x000009C4
            /// <summary>
            ///   <para>This render queue is rendered after Geometry and AlphaTest, in back-to-front order.</para>
            /// </summary>
            Transparent = 3000, // 0x00000BB8
            /// <summary>
            ///   <para>This render queue is meant for overlay effects.</para>
            /// </summary>
            Overlay = 4000, // 0x00000FA0
            End = 5000
        }
        
        
        [Serializable]
        public struct QueueRange
        {
            public Queue start;
            public Queue end;
        }
        
        [Serializable]
        public class MaterialParameterNode : BaseNode
        {
            public QueueRange renderQueueRange;
            public List<string> shaderTagList;
            public Material overrideMaterial;
        }

        public static MaterialParameterNode CreateMaterialParameterNode(string name, string guid)
        {
            return new MaterialParameterNode()
            {
                name = name,
                guid = guid,
                renderQueueRange =
                {
                    start = Queue.Start,
                    end = Queue.End
                },
                shaderTagList = new List<string>()
                {
                    "SRPDefaultUnlit",
                    "FRPForward",
                    "FRPForwardOnly"
                },
                overrideMaterial = null,
                type = NodeType.RenderMaterialNode
            };
        }
        
        
        [Serializable]
        public class CameraParameterNode : BaseNode
        {
            public float fov;
            public Vector3 offset;
        }

        public static CameraParameterNode CreateCameraParameterNode(string name, string guid)
        {
            return new CameraParameterNode()
            {
                fov = 60,
                offset = Vector3.zero,
                name = name,
                guid = guid,
                type = NodeType.CameraParameterNode
            };
        }

        /// chichi: RenderBufferNode is also a special node like FlowNode, only keep the actual data 's reference key.
        /// because there may exist a shared buffer , which can shared among multiple graph within one frame, so we need
        /// store shared buffer to pipeline asset..

        public enum BufferType
        {
            TextureBuffer,
            ComputerBuffer
        }

        public enum BufferLifeTime
        {
            PerFrame,
            PerCamera,
            PerPass
        }
        
        [Serializable]
        public class BufferNode : BaseNode
        {
            public string bufferID;
            public BufferType bufferType;
            public BufferLifeTime lifeTime;
        }
        

        public static BufferNode CreateBufferNode(string nodeName, string guid, string bufferID, BufferType bufferType, BufferLifeTime lifeTime = BufferLifeTime.PerPass)
        {
            return new BufferNode()
            {
                name = nodeName,
                guid = guid,
                type = NodeType.BufferNode,
                bufferType = bufferType,
                bufferID = bufferID,
                lifeTime = lifeTime
            };
        }

        [Serializable]
        public struct FastMemory
        {
            ///<summary>Whether the texture will be in fast memory.</summary>
            public bool inFastMemory;
            ///<summary>Flag to determine what parts of the render target is spilled if not fully resident in fast memory.</summary>
            public FastMemoryFlags flags;
            ///<summary>How much of the render target is to be switched into fast memory (between 0 and 1).</summary>
            public float residencyFraction;
        }

        [Serializable]
        public class TextureBufferNode : BaseNode
        {
            #region Header Info
            
            ///<summary>Texture is a shadow map.</summary>
            public bool isShadowMap;
            ///<summary>Determines whether the texture will fallback to a black texture if it is read without ever writing to it.</summary>
            public bool fallBackToBlackTexture;
            
            #endregion

            #region Size Info

            public TextureSizeMode sizeMode;
            public int width;
            public int height;
            public Vector2 scale;
            ///<summary>Texture uses dynamic scaling.</summary>
            public bool useDynamicScale;
            ///<summary>used for xr.</summary>
            public int slices;
            #endregion

            #region Init State

            // Initial state. Those should not be used in the hash
            ///<summary>Texture needs to be cleared on first use.</summary>
            public bool clearBuffer;
            ///<summary>Clear color.</summary>
            public Color clearColor;

            #endregion

            #region Format Info

            public GraphicsFormat colorFormat;
            public DepthBits depthBits;
            ///<summary>Texture dimension.</summary>
            public TextureDimension dimension;

            #endregion

            #region Filtering and Addressing

            ///<summary>Filtering mode.</summary>
            public FilterMode filterMode;
            ///<summary>Addressing mode.</summary>
            public TextureWrapMode wrapMode;
            ///<summary>Anisotropic filtering level.</summary>
            public int anisoLevel;
            
            #endregion

            #region Mipmap Info

            ///<summary>Texture needs mip maps.</summary>
            public bool useMipMap;
            ///<summary>Automatically generate mip maps.</summary>
            public bool autoGenerateMips;
            ///<summary>Mip map bias.</summary>
            public float mipMapBias;

            #endregion

            #region Memory Info

            ///<summary>Enable random UAV read/write on the texture.</summary>
            public bool enableRandomWrite;
            ///<summary>Memory less flag.</summary>
            public RenderTextureMemoryless memoryless;
            // fast memory desc
            public FastMemory fastMemoryDesc;

            public DepthAccess depthAccess;
            
            #endregion

            #region MSAA

            ///<summary>Number of MSAA samples.</summary>
            public MSAASamples msaaSamples;
            ///<summary>Bind texture multi sampled.</summary>
            public bool bindTextureMS;

            #endregion
        }

        public static TextureBufferNode CreateTextureBufferNode(string name, string guid)
        {
            return new TextureBufferNode()
            {
                name = name,
                type = NodeType.TextureBuffer,
                guid = guid,
                
                // Size related init
                sizeMode = TextureSizeMode.Explicit,
                width = 1,
                height = 1,
                scale = Vector2.one,
                
                // Important default values not handled by zero construction in this()
                msaaSamples = MSAASamples.None,
                useDynamicScale = false,
                slices = 1,
                dimension = TextureDimension.Tex2D,
                
                fastMemoryDesc = new FastMemory()
            };
        }
        
        [Serializable]
        public class ComputerBufferNode : BaseNode
        {
            
        }
    }
}