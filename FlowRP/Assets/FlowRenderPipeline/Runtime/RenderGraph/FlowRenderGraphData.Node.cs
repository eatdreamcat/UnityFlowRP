using System;
using System.Collections.Generic;

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
            public FRPNodeType type;
        }

        [Serializable]
        public class FlowNode : BaseNode
        {
            // for a flow node , it only contains flow information, data id is used to bind a extra data , like render request data
            public string dataID;
            public FRPNodeType dataType;
            
            public List<string> flowIn;
            public List<string> flowOut;
        }

        public FlowNode CreateFlowNode(string nodeName, string dataID, FRPNodeType dataType)
        {
            return new FlowNode()
            {
                name = nodeName,
                guid = dataID,
                dataType = dataType,
                dataID = dataID,
                type = FRPNodeType.Flow,
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
                type = FRPNodeType.Entry,
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
                type = FRPNodeType.FRPDrawRendererNode,

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
                type = FRPNodeType.FRPDrawFullScreenNode,
                
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
        }

        public static CullingParameterNode CreateCullingParameterNode(string name, string guid)
        {
            return new CullingParameterNode()
            {
                name = name,
                guid = guid,
                type = FRPNodeType.FRPCullingParameterNode,
                isAllowPassCulling = true,
                isAllowRendererCulling = true,
                cullingMask = -1
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
                type = FRPNodeType.FRPRenderStateNode,
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
                type = FRPNodeType.FRPRenderMaterialNode
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
                type = FRPNodeType.FRPCameraParameterNode
            };
        }
    }
}