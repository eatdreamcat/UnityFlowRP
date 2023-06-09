

using System;
using UnityEngine.UIElements;

namespace UnityEngine.Rendering.FlowPipeline
{
    public class FlowRenderGraphData : ScriptableObject
    {
        
        public enum FRPNodeType
        {
            FRPNodeBase,
            // flow control
            FRPBranchNode,
            FPRLoopNode,
            
            // render pass
            FRPRenderRequestNode,
            
            // render target
            FRPRenderTargetNode,
            
            // render resources： TextureBuffer, DepthStencilBuffer, ComputeBuffer
            FRPResourceNode,
            FRPRenderTextureNode,
        }

        
        
        protected FlowRenderGraphData Create()
        {
            if (!Application.isPlaying)
            {
                // todo : reload all
            }
            return new FlowRenderGraphData();
        }
        
        
        ///
        ///
        ///    GraphData
        ///
        ///    NodeList
        ///      Node - use guid to map FRPGraphViewSavedData
        ///
        /// 
        ///    ConnectionsMap
        ///      node-connecttions
        ///      guid - guid list
        ///    NodeInputMap
        ///      node-(input list)
        ///      guid - guid list
        ///    NodeOutputMap
        ///      node-(output list)
        ///      guid - guid list
        ///
        ///

        [Serializable]
        class BaseNode
        {
            /* using for tracking layout data*/
            public string guid;
        }
        [Serializable]
        class PassNode : BaseNode
        {
            
        }

        [Serializable]
        class ResourceNode : BaseNode
        {
            
        }
        
        [Serializable]
        class BranchNode : BaseNode
        {
            
        }
    }
}
