

using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.Rendering.SuperAdvanced
{
    public partial class SARenderGraphData : ScriptableObject, ISerializationCallbackReceiver
    {
        
        /// <summary>
        /// with "Node" suffix , will be render to GraphView and also store data
        /// others are only used for data.
        /// </summary>
        public enum NodeType
        {
            Unknow,
            Flow,
            
            EntryNode,
            
            // logic
            BranchNode,
            LoopNode,
            
            // parameter
            CameraParameterNode,
            CullingParameterNode,
            RenderMaterialNode,
            RenderStateNode,
            
            // pass
            DrawRendererNode,
            DrawFullScreenNode,
            ComputeNode,
            
            BufferNode,
            
            // buffer - only reference
            TextureBuffer,
            ComputerBuffer,
           

            // variables
            
            

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

        #region Dictionary Definition

        [Serializable]
        internal sealed class CullingNodeDictionary : SASerializedDictionary<string, CullingParameterNode> { }
        [Serializable]
        internal sealed class RenderStateNodeDictionary : SASerializedDictionary<string, RenderStateNode> { }
        [Serializable]
        internal sealed class MaterialNodeDictionary : SASerializedDictionary<string, MaterialParameterNode> { }
        [Serializable]
        internal sealed class CameraNodeDictionary : SASerializedDictionary<string, CameraParameterNode> { }
        [Serializable]
        internal sealed class DrawRendererNodeDictionary : SASerializedDictionary<string, DrawRendererNode> { }
        [Serializable]
        internal sealed class DrawFullScreenNodeDictionary : SASerializedDictionary<string, DrawFullScreenNode> { }
        [Serializable]
        internal sealed class FlowNodeDictionary : SASerializedDictionary<string, FlowNode> { }
        [Serializable]
        internal sealed class BufferNodeDictionay : SASerializedDictionary<string, BufferNode> {}
        [Serializable]
        internal sealed class TextureBufferNodeDictionary : SASerializedDictionary<string, TextureBufferNode> {}
        

        [SerializeField]
        private CullingNodeDictionary m_CullingNodesMap = new  CullingNodeDictionary();
        [SerializeField]
        private RenderStateNodeDictionary m_RenderStateNodesMap = new  RenderStateNodeDictionary();
        [SerializeField]
        private MaterialNodeDictionary m_MaterialNodesMap = new  MaterialNodeDictionary();
        [SerializeField]
        private CameraNodeDictionary m_CameraNodesMap = new  CameraNodeDictionary();
        [SerializeField]
        private DrawRendererNodeDictionary m_DrawRendererNodesMap = new  DrawRendererNodeDictionary();
        [SerializeField]
        private DrawFullScreenNodeDictionary m_DrawFullScreenNodesMap = new DrawFullScreenNodeDictionary();
        [SerializeField]
        private FlowNodeDictionary m_FlowNodesMap = new  FlowNodeDictionary();

        [SerializeField] private BufferNodeDictionay m_BufferNodeMap = new BufferNodeDictionay();
        [SerializeField] private TextureBufferNodeDictionary m_TextureBufferNodeMap = new TextureBufferNodeDictionary();
        
        
        #endregion
       
       
        #region NodeList Interface

        public List<CullingParameterNode> CullingNodeList => m_CullingNodesMap.Values.ToList();
        
        public CullingParameterNode TryGetCullingParameterNode(string nodeID)
        {
            Debug.Assert(m_CullingNodesMap.ContainsKey(nodeID), $"Culling Parameter Node {nodeID} not exist.");
            return m_CullingNodesMap[nodeID];
        }

        public List<RenderStateNode> RenderStateNodeList => m_RenderStateNodesMap.Values.ToList();
        
        public RenderStateNode TryGetRenderStateNode(string nodeID)
        {
            Debug.Assert(m_RenderStateNodesMap.ContainsKey(nodeID), $"Render State Node {nodeID} not exist.");
            return m_RenderStateNodesMap[nodeID];
        }

        public List<MaterialParameterNode> MaterialNodeList => m_MaterialNodesMap.Values.ToList();

        public MaterialParameterNode TryGetMaterialParameterNode(string nodeID)
        {
            Debug.Assert(m_MaterialNodesMap.ContainsKey(nodeID), $"Material Parameter Node {nodeID} not exist.");
            return m_MaterialNodesMap[nodeID];
        }

        public List<CameraParameterNode> CameraNodeList => m_CameraNodesMap.Values.ToList();

        public CameraParameterNode TryGetCameraParameterNode(string nodeID)
        {
            Debug.Assert(m_CameraNodesMap.ContainsKey(nodeID), $"Camera Parameter Node {nodeID} not exist.");
            return m_CameraNodesMap[nodeID];
        }

        public List<DrawRendererNode> DrawRendererNodeList => m_DrawRendererNodesMap.Values.ToList();
        public DrawRendererNode TryGetDrawRendererPassNode(string nodeID)
        {
            Debug.Assert(m_DrawRendererNodesMap.ContainsKey(nodeID), $"Draw Renderer Node {nodeID} not exist.");
            return m_DrawRendererNodesMap[nodeID];
        }

        public List<DrawFullScreenNode> DrawFullScreenNodeList => m_DrawFullScreenNodesMap.Values.ToList();
        
        public DrawFullScreenNode TryGetDrawFullScreenNode(string nodeID)
        {
            Debug.Assert(m_DrawFullScreenNodesMap.ContainsKey(nodeID), $"Draw Full Screen Node {nodeID} not exist.");
            return m_DrawFullScreenNodesMap[nodeID];
        }

        public List<BufferNode> BufferNodeList => m_BufferNodeMap.Values.ToList();

        public BufferNode TryGetBufferNode(string nodeID)
        {
            Debug.Assert(m_BufferNodeMap.ContainsKey(nodeID), $"Buffer Node {nodeID} not exist.");
            return m_BufferNodeMap[nodeID];
        }
        
        public TextureBufferNode TryGetTextureBufferNode(string nodeID)
        {
            Debug.Assert(m_TextureBufferNodeMap.ContainsKey(nodeID), $"Texture Buffer Node {nodeID} not exist.");
            return m_TextureBufferNodeMap[nodeID];
        }
        
        public FlowNode TryGetFlowNode(string nodeID)
        {
            Debug.Assert(m_FlowNodesMap.ContainsKey(nodeID), $"Flow Node {nodeID} not exist.");
            return m_FlowNodesMap[nodeID];
        }
        
        #endregion
        
        [SerializeField] private EntryNode m_EntryNode;
        [SerializeField] public string GraphGuid;

        public string EntryID
        {
            get
            {
                return m_EntryNode.guid;
            }
        }

        public EntryNode Entry
        {
            get
            {
                return m_EntryNode;
            }
        }
        
       
        public void OnBeforeSerialize()
        {
            //  throw new NotImplementedException();
        }
        
        public void OnAfterDeserialize()
        {
            
        }
        
    }
}
