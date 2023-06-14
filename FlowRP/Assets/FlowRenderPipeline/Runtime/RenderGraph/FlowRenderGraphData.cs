

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Collections;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;

namespace UnityEngine.Rendering.FlowPipeline
{
    public class FlowRenderGraphData : ScriptableObject, ISerializationCallbackReceiver
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812")]
        internal class CreateRenderGraphDataAsset : EndNameEditAction
        {
            public override void Action(int instanceId, string path, string resourceFile)
            {
                //Create asset
                FlowRenderGraphData data = CreateInstance<FlowRenderGraphData>();
                data.InitGUID();
                AssetDatabase.CreateAsset(data, path);
                FlowUtility.SaveAsset(data);
                ResourceReloader.ReloadAllNullIn(data, FlowUtility.GetFlowRenderPipelinePath());
            }
        }
        
        
        [MenuItem("Assets/Create/Rendering/Render Graph Data Asset", priority = CoreUtils.Sections.section2 + CoreUtils.Priorities.assetsCreateRenderingMenuPriority + 1)]
        static void Create()
        {
            if (!Application.isPlaying)
            {
                // todo : reload all
            }
            
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, CreateInstance<CreateRenderGraphDataAsset>(),
                "New Render Graph Data Asset.asset",null, null);
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
            public List<string> inputList;
            public List<string> outputList;
           
            public List<string> flowIn;
            public List<string> flowOut;
        }

        [Serializable]
        internal sealed class NodeDictionary : FRPSerializedDictionary<string, BaseNode> { }
        

        [SerializeField]
        private NodeDictionary m_NodesMap = new  NodeDictionary();

        public List<BaseNode> NodeList
        {
            get
            {
                return m_NodesMap.Values.ToList();
            }
        }
        
        [SerializeField] public string GraphGuid;
        [SerializeField] public string EntryGuid;

        public void InitGUID()
        {
            GraphGuid = Guid.NewGuid().ToString();
        }
        
        public bool IsEmpty()
        {
            return NodeList.Count <= 0;
        }
        
        public NodeCreationResult AddEntryNode()
        {
            if (EntryGuid != null && EntryGuid != "")
            {
                Debug.LogError("[GraphData.AddEntryNode] Damn!!!!!!!!! This Graph already has a entry!!!");
            }

            var newNode = FlowUtility.CreateBaseNode("Entry", Guid.NewGuid().ToString(), FRPNodeType.Entry);
            m_NodesMap.Add(newNode.guid, newNode);
            EntryGuid = newNode.guid;
            FlowUtility.SaveAsset(this);
            
            return new NodeCreationResult()
            {
                name = "Entry",
                guid = newNode.guid
            };
        }

        // TODO 
        public void AddNode(BaseNode newNode)
        {
            // test code 
            m_NodesMap.Add(newNode.guid, newNode);
            
            switch (newNode.type)
            {
                // render request
                case FRPNodeType.FRPRenderRequestNode:
                {
                    
                }
                    break;
                
                // render resources
                case FRPNodeType.FRPResourceNode:
                {
                  
                   
            
                   
                }
                    break;
                
                
                /// flow control
                case FRPNodeType.FPRLoopNode:
                {
                    
                }
                    break;
                case FRPNodeType.FRPBranchNode:
                {
                    
                }
                    break;
            }
            
           FlowUtility.SaveAsset(this);
        }

        public void UpdateNodeName(string guid, string newName)
        {
            if (m_NodesMap.TryGetValue(guid, out var node))
            {
                node.name = newName;
                FlowUtility.SaveAsset(this);
            }
            else
            {
                Debug.LogError($"[GraphData.UpdateNodeName] Node {guid} not exist.");
            }
        }

        public void DeleteNode(string guid)
        {
            if (m_NodesMap.ContainsKey(guid))
            {
                m_NodesMap.Remove(guid);
                
                FlowUtility.SaveAsset(this);
                
                return;
            }
            
            Debug.LogError($"[GraphData.DeleteNode] Node {guid} not exist.");
        }

        // flowOut -> flowIn
        public void AddFlowInOut(string flowInID, string flowOutID)
        {
            if (m_NodesMap.TryGetValue(flowInID, out BaseNode inNode))
             {
                 inNode.flowIn.Add(flowOutID);
             }
             else
             {
                 Debug.LogError($"[GraphData.AddFlowInOut] Node(In) {flowInID} not exist.");
             }

             if (m_NodesMap.TryGetValue(flowOutID, out BaseNode outNode))
             {
                 outNode.flowOut.Add(flowInID);
             }
             else
             {
                 Debug.LogError($"[GraphData.AddFlowInOut] Node(Out) {flowOutID} not exist.");
             }
             
             FlowUtility.SaveAsset(this);
        }
        
        public void DeleteFlowInOut(string flowInID, string flowOutID)
        {
            if (m_NodesMap.TryGetValue(flowInID, out BaseNode inNode))
            {
                inNode.flowIn.Remove(flowOutID);
            }
            else
            {
                Debug.LogError($"[GraphData.DeleteFlowInOut] Node(In) {flowInID} not exist.");
            }

            if (m_NodesMap.TryGetValue(flowOutID, out BaseNode outNode))
            {
                outNode.flowOut.Remove(flowInID);
            }
            else
            {
                Debug.LogError($"[GraphData.DeleteFlowInOut] Node(Out) {flowOutID} not exist.");
            }
            
            FlowUtility.SaveAsset(this);
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
