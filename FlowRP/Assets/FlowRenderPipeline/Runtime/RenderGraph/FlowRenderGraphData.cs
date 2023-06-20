

using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.Rendering.FlowPipeline
{
    public partial class FlowRenderGraphData : ScriptableObject, ISerializationCallbackReceiver
    {
        
        public enum FRPNodeType
        {
            Entry,
            
            // buffer
            BackBuffer,
            FrontBuffer,
            CameraTarget,
            RenderBuffer,
            
            // logic
            FRPBranchNode,
            FRPLoopNode,
            
            // pass
            FRPCameraParameterNode,
            FRPCullingParameterNode,
            FRPRenderMaterialNode,
            FRPRenderRequestNode,
            FRPRenderStateNode,
            
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
        internal sealed class CullingNodeDictionary : FRPSerializedDictionary<string, CullingParameterNode> { }
        [Serializable]
        internal sealed class RenderStateNodeDictionary : FRPSerializedDictionary<string, RenderStateNode> { }
        [Serializable]
        internal sealed class MaterialNodeDictionary : FRPSerializedDictionary<string, MaterialParameterNode> { }
        [Serializable]
        internal sealed class CameraNodeDictionary : FRPSerializedDictionary<string, CameraParameterNode> { }
        [Serializable]
        internal sealed class RenderRequestNodeDictionary : FRPSerializedDictionary<string, RenderRequestNode> { }
        
        [SerializeField]
        private CullingNodeDictionary m_CullingNodesMap = new  CullingNodeDictionary();
        [SerializeField]
        private RenderStateNodeDictionary m_RenderStateNodesMap = new  RenderStateNodeDictionary();
        [SerializeField]
        private MaterialNodeDictionary m_MaterialNodesMap = new  MaterialNodeDictionary();
        [SerializeField]
        private CameraNodeDictionary m_CameraNodesMap = new  CameraNodeDictionary();
        [SerializeField]
        private RenderRequestNodeDictionary m_RenderRequestNodesMap = new  RenderRequestNodeDictionary();
        
        
        #endregion
       
       
        #region NodeList Interface

        public List<CullingParameterNode> CullingNodeList
        {
            get
            {
                return m_CullingNodesMap.Values.ToList();
            }
        }
        
        public List<RenderStateNode> RenderStateNodeList
        {
            get
            {
                return m_RenderStateNodesMap.Values.ToList();
            }
        }
        
        public List<MaterialParameterNode> MaterialNodeList
        {
            get
            {
                return m_MaterialNodesMap.Values.ToList();
            }
        }
        
        
        public List<CameraParameterNode> CameraNodeList
        {
            get
            {
                return m_CameraNodesMap.Values.ToList();
            }
        }
        
        public List<RenderRequestNode> PassNodeList
        {
            get
            {
                return m_RenderRequestNodesMap.Values.ToList();
            }
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
          //  throw new NotImplementedException();
        }
        
        
    }
}
