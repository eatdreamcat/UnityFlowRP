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

        public FlowNode CreateFlowNode(string nodeName, string guid, string dataID = "",
            FRPNodeType dataType = FRPNodeType.Unknow)
        {
            return new FlowNode()
            {
                name = name,
                guid = guid,
                dataType = dataType,
                dataID = dataID
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
        public class RenderRequestNode : BaseNode
        {
            public bool isEnabled;
            public string culling;
            public string state;
            public string material;
            public string camera;

            public List<string> inputList;
            public List<string> outputList;
        }
        
        public static RenderRequestNode CreateRenderRequestNode(string name, string guid)
        {
            return new RenderRequestNode()
            {
                name = name,
                guid = guid,
                type = FRPNodeType.FRPRenderRequestNode,
                isEnabled = true,
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
        public struct BlendStateData
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

        [Serializable]
        public class MaterialParameterNode : BaseNode
        {
            public RenderQueueRange renderQueueRange;
            public List<string> shaderTagList;
            public string shaderPath;
            public Material overrideMaterial;
        }

        public static MaterialParameterNode CreateMaterialParameterNode(string name, string guid)
        {
            return new MaterialParameterNode()
            {
                name = name,
                guid = guid,
                renderQueueRange = RenderQueueRange.all,
                shaderTagList = new List<string>()
                {
                    "SRPDefaultUnlit",
                    "FRPForward",
                    "FRPForwardOnly"
                },
                shaderPath = "",
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