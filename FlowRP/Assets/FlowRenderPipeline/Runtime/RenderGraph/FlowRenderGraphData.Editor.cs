using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEditor.VersionControl;

namespace UnityEngine.Rendering.FlowPipeline
{
#if UNITY_EDITOR
    public partial class FlowRenderGraphData
    {

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

        
        public void InitGUID()
        {
            GraphGuid = Guid.NewGuid().ToString();
        }

        public bool IsEmpty()
        {
            return m_RenderRequestNodesMap.Count <= 0;
        }

        public bool HasEntry()
        {
            return m_EntryNode != null && !string.IsNullOrEmpty(m_EntryNode.guid);
        }

        public NodeCreationResult AddEntryNode()
        {
            if (m_EntryNode != null && m_EntryNode.guid != "")
            {
                Debug.LogError("[GraphData.AddEntryNode] Damn!!!!!!!!! This Graph already has a entry!!!");
            }

            m_EntryNode = CreateEntryNode("Entry", Guid.NewGuid().ToString());
            m_FlowNodesMap.Add(EntryID, CreateFlowNode("Entry", EntryID, EntryID, FRPNodeType.Entry));
            
            FlowUtility.SaveAsset(this);

            return new NodeCreationResult()
            {
                name = "Entry",
                guid = m_EntryNode.guid
            };
        }

        public void AddNode(string nodeName, string guid, FRPNodeType nodeType)
        {
            switch (nodeType)
            {
                // render request
                case FRPNodeType.FRPRenderRequestNode:
                {
                    // check and set the first render request
                    if (string.IsNullOrEmpty(m_EntryNode.startPoint))
                    {
                        m_EntryNode.startPoint = guid;
                    }

                    var renderRequestNode = CreateRenderRequestNode(nodeName, guid);
                    m_RenderRequestNodesMap.Add(guid, renderRequestNode);
                    m_FlowNodesMap.Add(guid, CreateFlowNode(renderRequestNode.name, renderRequestNode.guid, renderRequestNode.guid, FRPNodeType.FRPRenderRequestNode));
                }
                    break;

                case FRPNodeType.FRPCullingParameterNode:
                {
                    m_CullingNodesMap.Add(guid, CreateCullingParameterNode(nodeName, guid));
                }
                    break;

                case FRPNodeType.FRPRenderStateNode:
                {
                    m_RenderStateNodesMap.Add(guid, CreateRenderStateNode(nodeName, guid));
                }
                    break;

                case FRPNodeType.FRPCameraParameterNode:
                {
                    m_CameraNodesMap.Add(guid, CreateCameraParameterNode(nodeName, guid));
                }
                    break;

                case FRPNodeType.FRPRenderMaterialNode:
                {
                    m_MaterialNodesMap.Add(guid, CreateMaterialParameterNode(nodeName, guid));
                }
                    break;
                
                
                // flow control
                case FRPNodeType.FRPLoopNode:
                {

                }
                    break;
                case FRPNodeType.FRPBranchNode:
                {

                }
                    break;
                
                // Buffer
                
                
                // Variables
            }

            FlowUtility.SaveAsset(this);
        }

        public void UpdateNodeName(string guid, string newName)
        {
            if (m_RenderRequestNodesMap.TryGetValue(guid, out var node))
            {
                node.name = newName;
                FlowUtility.SaveAsset(this);
            }
            else
            {
                Debug.LogError($"[GraphData.UpdateNodeName] Node {guid} not exist.");
            }
        }

        public void DeleteNode(string guid, FRPNodeType nodeType)
        {
            if (m_RenderRequestNodesMap.ContainsKey(guid))
            {
                m_RenderRequestNodesMap.Remove(guid);

                FlowUtility.SaveAsset(this);

                return;
            }

            Debug.LogError($"[GraphData.DeleteNode] Node {guid} not exist.");
        }



        #region Flow Management

        // flowOut -> flowIn
        public void AddFlowInOut(string flowOutID, string flowInID)
        {
            Debug.Assert(!string.IsNullOrEmpty(flowInID) && !string.IsNullOrEmpty(flowOutID), "Flow In or Out ID invalid.");
            
            if (m_FlowNodesMap.TryGetValue(flowInID, out var inNode))
            {
                inNode.flowIn.Add(flowOutID);
            }
            else
            {
                Debug.LogError($"[GraphData.AddFlowInOut] Node(In) {flowInID} not exist.");
            }

            if (m_FlowNodesMap.TryGetValue(flowOutID, out var outNode))
            {
                outNode.flowOut.Add(flowInID);
            }
            else
            {
                Debug.LogError($"[GraphData.AddFlowInOut] Node(Out) {flowOutID} not exist.");
            }

            FlowUtility.SaveAsset(this);
        }

        public void DeleteFlowInOut(string flowOutID, string flowInID)
        {
            if (m_FlowNodesMap.TryGetValue(flowInID, out var inNode))
            {
                inNode.flowIn.Remove(flowOutID);
            }
            else
            {
                Debug.LogError($"[GraphData.DeleteFlowInOut] Node(In) {flowInID} not exist.");
            }

            if (m_FlowNodesMap.TryGetValue(flowOutID, out var outNode))
            {
                outNode.flowOut.Remove(flowInID);
            }
            else
            {
                Debug.LogError($"[GraphData.DeleteFlowInOut] Node(Out) {flowOutID} not exist.");
            }

            FlowUtility.SaveAsset(this);
        }

        #endregion

        
        #region RenderRequestNode
        
        public void AddCullingAssignment(string assignIn, string targetNodeID)
        {
            if (m_RenderRequestNodesMap.TryGetValue(targetNodeID, out var inNode))
            {
                inNode.culling = assignIn;
            }
            else
            {
                Debug.LogError($"[GraphData.AddCullingAssignment] Node(In) {targetNodeID} not exist.");
            }
        }

        public void DeleteCullingAssignment(string assignIn, string targetNodeID)
        {
            if (m_RenderRequestNodesMap.TryGetValue(targetNodeID, out var inNode))
            {
                Debug.Assert(inNode.culling == assignIn, "Current assignment is not equal to the to-disconnected one .");
                inNode.culling = "";
            }
            else
            {
                Debug.LogError($"[GraphData.DeleteCullingAssignment] Node(In) {targetNodeID} not exist.");
            }
        }

        public void AddRenderStateAssignment(string assignIn, string targetNodeID)
        {
            if (m_RenderRequestNodesMap.TryGetValue(targetNodeID, out var inNode))
            {
                inNode.state = assignIn;
            }
            else
            {
                Debug.LogError($"[GraphData.AddRenderStateAssignment] Node(In) {targetNodeID} not exist.");
            }
        }

        public void DeleteRenderStateAssignment(string assignIn, string targetNodeID)
        {
            if (m_RenderRequestNodesMap.TryGetValue(targetNodeID, out var inNode))
            {
                Debug.Assert(inNode.state == assignIn, "Current assignment is not equal to the to-disconnected one .");
                inNode.state = "";
            }
            else
            {
                Debug.LogError($"[GraphData.DeleteRenderStateAssignment] Node(In) {targetNodeID} not exist.");
            }
        }

        public void AddMaterialAssignment(string assignIn, string targetNodeID)
        {
            if (m_RenderRequestNodesMap.TryGetValue(targetNodeID, out var inNode))
            {
                inNode.material = assignIn;
            }
            else
            {
                Debug.LogError($"[GraphData.AddMaterialAssignment] Node(In) {targetNodeID} not exist.");
            }
        }

        public void DeleteMaterialAssignment(string assignIn, string targetNodeID)
        {
            if (m_RenderRequestNodesMap.TryGetValue(targetNodeID, out var inNode))
            {
                Debug.Assert(inNode.material == assignIn, "Current assignment is not equal to the to-disconnected one .");
                inNode.material = "";
            }
            else
            {
                Debug.LogError($"[GraphData.DeleteMaterialAssignment] Node(In) {targetNodeID} not exist.");
            }
        }

        public void AddCameraAssignment(string assignIn, string targetNodeID)
        {
            if (m_RenderRequestNodesMap.TryGetValue(targetNodeID, out var inNode))
            {
                inNode.camera = assignIn;
            }
            else
            {
                Debug.LogError($"[GraphData.AddCameraAssignment] Node(In) {targetNodeID} not exist.");
            }
        }

        public void DeleteCameraAssignment(string assignIn, string targetNodeID)
        {
            if (m_RenderRequestNodesMap.TryGetValue(targetNodeID, out var inNode))
            {
                Debug.Assert(inNode.camera == assignIn, "Current assignment is not equal to the to-disconnected one .");
                inNode.camera = "";
            }
            else
            {
                Debug.LogError($"[GraphData.DeleteCameraAssignment] Node(In) {targetNodeID} not exist.");
            }
        }
        
        #endregion
    }
    
#endif
}