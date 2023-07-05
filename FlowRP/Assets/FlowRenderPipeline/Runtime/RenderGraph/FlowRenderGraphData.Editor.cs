using System;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;

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
        
        public FlowRenderGraphData()
        {
            InitGUID();
        }
        
        private void InitGUID()
        {
            GraphGuid = Guid.NewGuid().ToString();
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
            var entryFlow = CreateFlowNode("Entry", EntryID, NodeType.EntryNode);
            m_FlowNodesMap.Add(entryFlow.guid, entryFlow);
            
            FlowUtility.SaveAsset(this);

            return new NodeCreationResult()
            {
                name = "Entry",
                guid = m_EntryNode.guid
            };
        }

        public BaseNode AddNode(string nodeName, string guid, NodeType nodeType)
        {
            BaseNode result = default;
            switch (nodeType)
            {
                // render request
                case NodeType.DrawRendererNode:
                {
                    // check and set the first render request
                    if (string.IsNullOrEmpty(m_EntryNode.startPoint))
                    {
                        m_EntryNode.startPoint = guid;
                    }

                    var renderRequestNode = CreateRenderRequestNode(nodeName, guid);
                    m_DrawRendererNodesMap.Add(guid, renderRequestNode);
                    var flowNode = CreateFlowNode(renderRequestNode.name, renderRequestNode.guid,
                        NodeType.DrawRendererNode);
                    m_FlowNodesMap.Add(flowNode.guid, flowNode);

                    result =  renderRequestNode;
                    
                    break;
                }

                case NodeType.DrawFullScreenNode:
                {
                    // check and set the first render request
                    if (string.IsNullOrEmpty(m_EntryNode.startPoint))
                    {
                        m_EntryNode.startPoint = guid;
                    }

                    var drawFullScreenNode = CreateDrawFullScreenNode(nodeName, guid);
                    m_DrawFullScreenNodesMap.Add(guid, drawFullScreenNode);
                    var flowNode = CreateFlowNode(drawFullScreenNode.name, drawFullScreenNode.guid,
                        NodeType.DrawFullScreenNode);
                    m_FlowNodesMap.Add(flowNode.guid, flowNode);
                    
                    result = drawFullScreenNode;
                    
                    break;
                }

                case NodeType.CullingParameterNode:
                {
                    var cullingNode = CreateCullingParameterNode(nodeName, guid);
                    m_CullingNodesMap.Add(guid, cullingNode);
                    result = cullingNode;
                    
                    break;
                }
                    

                case NodeType.RenderStateNode:
                {
                    var renderState = CreateRenderStateNode(nodeName, guid);
                    m_RenderStateNodesMap.Add(guid, renderState);
                    result = renderState;
                    
                    break;
                }

                case NodeType.CameraParameterNode:
                {
                    var cameraParameter = CreateCameraParameterNode(nodeName, guid);
                    m_CameraNodesMap.Add(guid, cameraParameter);
                    result = cameraParameter;
                    
                    break;
                }
                  

                case NodeType.RenderMaterialNode:
                {
                    var materialNode = CreateMaterialParameterNode(nodeName, guid);
                    m_MaterialNodesMap.Add(guid, materialNode);
                    result =  materialNode;
                    
                    break;
                }
                
                // Buffer
                case NodeType.TextureBuffer:
                {
                    var textureBufferNode = CreateTextureBufferNode(nodeName, guid);
                    var bufferNode = CreateBufferNode(nodeName, guid, textureBufferNode.guid, BufferType.TextureBuffer);
                    m_BufferNodeMap.Add(bufferNode.guid, bufferNode);
                    m_TextureBufferNodeMap.Add(textureBufferNode.guid, textureBufferNode);

                    result = bufferNode;
                    
                    break;
                }
                
                case NodeType.ComputerBuffer:
                {
                   
                    
                    break;
                }
                
                // flow control
                case NodeType.LoopNode:
                {

                }
                    break;
                
                case NodeType.BranchNode:
                {

                }
                    break;
                
               
                
                
                // Variables
            }

            FlowUtility.SaveAsset(this);
            return result;
        }

        public void UpdateNodeName(string guid, string newName, NodeType nodeType)
        {
            switch (nodeType)
            {
                case NodeType.DrawRendererNode:
                {
                    if (m_DrawRendererNodesMap.TryGetValue(guid, out var node))
                    {
                        node.name = newName;
                        FlowUtility.SaveAsset(this);
                        return;
                    }
                }
                    break;
                case NodeType.DrawFullScreenNode:
                {
                    if (m_DrawFullScreenNodesMap.TryGetValue(guid, out var node))
                    {
                        node.name = newName;
                        FlowUtility.SaveAsset(this);
                        return;
                    }
                }
                    break;
                case NodeType.BufferNode:
                {
                    if (m_BufferNodeMap.TryGetValue(guid, out var node))
                    {
                        node.name = newName;

                        if (node.bufferType == BufferType.TextureBuffer)
                        {
                            m_TextureBufferNodeMap[guid].name = newName;
                        }
                        
                        FlowUtility.SaveAsset(this);
                        return;
                    }
                }
                    break;
            }
            
            Debug.LogError($"[GraphData.UpdateNodeName] Node {guid}, {nodeType} not exist.");
        }

        public void DeleteNode(string guid, NodeType nodeType)
        {
            switch (nodeType)
            {
                case NodeType.DrawRendererNode:
                {
                    if (m_DrawRendererNodesMap.ContainsKey(guid))
                    {
                        m_DrawRendererNodesMap.Remove(guid);

                        FlowUtility.SaveAsset(this);

                        return;
                    }
                }
                    break;
                
                case NodeType.DrawFullScreenNode:
                {
                    if (m_DrawFullScreenNodesMap.ContainsKey(guid))
                    {
                        m_DrawFullScreenNodesMap.Remove(guid);

                        FlowUtility.SaveAsset(this);

                        return;
                    }
                }
                    break;
                
                case NodeType.BufferNode:
                {
                    if (m_BufferNodeMap.TryGetValue(guid, out var node))
                    {
                       
                        if (node.bufferType == BufferType.TextureBuffer)
                        {
                            m_TextureBufferNodeMap.Remove(guid);
                        }

                        m_BufferNodeMap.Remove(guid);
                        
                        FlowUtility.SaveAsset(this);
                        return;
                    }
                }
                    break;

                case NodeType.CullingParameterNode:
                {
                    if (m_CullingNodesMap.ContainsKey(guid))
                    {
                        m_CullingNodesMap.Remove(guid);

                        FlowUtility.SaveAsset(this);

                        return;
                    }
                }
                    break;
                
                case NodeType.RenderStateNode:
                {
                    if (m_RenderStateNodesMap.ContainsKey(guid))
                    {
                        m_RenderStateNodesMap.Remove(guid);

                        FlowUtility.SaveAsset(this);

                        return;
                    }
                }
                    break;
                
                case NodeType.RenderMaterialNode:
                {
                    if (m_MaterialNodesMap.ContainsKey(guid))
                    {
                        m_MaterialNodesMap.Remove(guid);

                        FlowUtility.SaveAsset(this);

                        return;
                    }
                }
                    break;
                
                case NodeType.CameraParameterNode:
                {
                    if (m_CameraNodesMap.ContainsKey(guid))
                    {
                        m_CameraNodesMap.Remove(guid);

                        FlowUtility.SaveAsset(this);

                        return;
                    }
                }
                    break;
            }

            Debug.LogError($"[GraphData.DeleteNode] Node {guid}, {nodeType} not exist.");
        }



        #region Flow Management

        // flowOut -> flowIn
        public void AddFlowInOut(string flowOutID, string flowInID)
        {
            Debug.Assert(!string.IsNullOrEmpty(flowInID) && !string.IsNullOrEmpty(flowOutID), "Flow In or Out ID invalid.");
            
            if (m_FlowNodesMap.TryGetValue(flowInID, out var inNode))
            {
                if (inNode.flowIn.Count <= 0)
                {
                    inNode.flowIn.Add(flowOutID);
                }
                else
                {
                    inNode.flowIn[0] = flowOutID;
                }
            }
            else
            {
                Debug.LogError($"[GraphData.AddFlowInOut] Node(In) {flowInID} not exist.");
            }

            if (m_FlowNodesMap.TryGetValue(flowOutID, out var outNode))
            {
                if (outNode.dataType == NodeType.BranchNode)
                {
                    // branch node can have multiple flow outputs
                    outNode.flowOut.Add(flowInID);
                }
                else
                {
                    if (outNode.flowOut.Count <= 0)
                    {
                        outNode.flowOut.Add(flowInID);
                    }
                    else
                    {
                        outNode.flowOut[0] = flowInID;
                    }
                }
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
        
        public void AddTextureBufferInputAssignment(string assignIn, string targetNodeID)
        {
            if (m_DrawRendererNodesMap.TryGetValue(targetNodeID, out var inNode))
            {
                inNode.inputList.Add(assignIn);
            }
            else
            {
                Debug.LogError($"[GraphData.AddTextureBufferInputAssignment] Node(In) {targetNodeID} not exist.");
            }
        }
        
        public void DeleteTextureBufferInputAssignment(string assignIn, string targetNodeID)
        {
            if (m_DrawRendererNodesMap.TryGetValue(targetNodeID, out var inNode))
            {
                Debug.Assert(inNode.inputList.IndexOf(assignIn) >= 0, "Current assignment is not equal to the to-disconnected one .");
                inNode.inputList.Remove(assignIn);
            }
            else
            {
                Debug.LogError($"[GraphData.DeleteTextureBufferInputAssignment] Node(In) {targetNodeID} not exist.");
            }
        }
        
        public void AddTextureBufferOutputAssignment(string writeTarget, string operationNode)
        {
            if (m_DrawRendererNodesMap.TryGetValue(operationNode, out var inNode))
            {
                inNode.outputList.Add(writeTarget);
            }
            else
            {
                Debug.LogError($"[GraphData.AddTextureBufferOutputAssignment] Node(In) {operationNode} not exist.");
            }
        }
        
        public void DeleteTextureBufferOutputAssignment(string writeTarget, string operationNode)
        {
            if (m_DrawRendererNodesMap.TryGetValue(operationNode, out var inNode))
            {
                Debug.Assert(inNode.outputList.IndexOf(writeTarget) >= 0, "Current assignment is not equal to the to-disconnected one .");
                inNode.outputList.Remove(writeTarget);
            }
            else
            {
                Debug.LogError($"[GraphData.DeleteTextureBufferOutputAssignment] Node(In) {operationNode} not exist.");
            }
        }
        
        public void AddCullingAssignment(string assignIn, string targetNodeID)
        {
            if (m_DrawRendererNodesMap.TryGetValue(targetNodeID, out var inNode))
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
            if (m_DrawRendererNodesMap.TryGetValue(targetNodeID, out var inNode))
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
            if (m_DrawRendererNodesMap.TryGetValue(targetNodeID, out var inNode))
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
            if (m_DrawRendererNodesMap.TryGetValue(targetNodeID, out var inNode))
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
            if (m_DrawRendererNodesMap.TryGetValue(targetNodeID, out var inNode))
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
            if (m_DrawRendererNodesMap.TryGetValue(targetNodeID, out var inNode))
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
            if (m_DrawRendererNodesMap.TryGetValue(targetNodeID, out var inNode))
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
            if (m_DrawRendererNodesMap.TryGetValue(targetNodeID, out var inNode))
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