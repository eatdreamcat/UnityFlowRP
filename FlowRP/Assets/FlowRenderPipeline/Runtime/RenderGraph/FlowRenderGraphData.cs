

using System;
using System.Collections.Generic;
using UnityEditor;

namespace UnityEngine.Rendering.FlowPipeline
{
    public class FlowRenderGraphData : ScriptableObject
    {
        
        public enum FRPNodeType
        {
            Entry,
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

        public struct NodeCreationResult
        {
            public string guid;
            public string name;
        }
        
        [Serializable]
        public class BaseNode
        {
            /* using for tracking layout data*/
            public string guid;
            public string name;
            public FRPNodeType type;
        }
        [Serializable]
        public class PassNode : BaseNode
        {
            
        }

        [Serializable]
        public class ResourceNode : BaseNode
        {
            
        }
        
        [Serializable]
        public class BranchNode : BaseNode
        {
            
        }

        [SerializeField]
        private List<BaseNode> m_NodeList = new List<BaseNode>();

        public List<BaseNode> NodeList
        {
            get
            {
                return m_NodeList;
            }
        }
        private string m_GUID;

        public string GUID
        {
           get
           {
               return m_GUID;
           }
           set
           {
               m_GUID = value;
           }
        }
        
        public bool IsEmpty()
        {
            return m_NodeList.Count <= 0;
        }
        
        public NodeCreationResult AddEntryNode()
        {
            var newNode = new BaseNode()
            {
                guid = Guid.NewGuid().ToString(),
                type = FRPNodeType.Entry,
                name = "Entry"
            };
            m_NodeList.Add(newNode);
            
            AssetDatabase.SaveAssets();
            
            return new NodeCreationResult()
            {
                name = "Entry",
                guid = newNode.guid
            };
        }
    }
}
