using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering.FlowPipeline;

namespace UnityEditor.Rendering.FlowPipeline
{
    public partial class FRPGraphView 
    {
        Dictionary<string, FRPNodeBase> m_FrpNodeMap = new Dictionary<string, FRPNodeBase>();
        Dictionary<string, FRPGroup> m_FrpGroupMap = new Dictionary<string, FRPGroup>();

        private void DrawEmptyGroup()
        {
            var groupViewList = m_GraphViewSavedData.GroupViewList;
            foreach (var groupData in groupViewList)
            {
                if (!m_FrpGroupMap.TryGetValue(groupData.guid, out var groupElement))
                {
                    groupElement = CreateGroup(groupData.title, groupData.position, groupData.guid);
                    AddElement(groupElement);
                    m_FrpGroupMap.Add(groupElement.ID, groupElement);
                }
            }
        }
        
        private void DrawNode(FlowRenderGraphData.BaseNode baseNode)
        {
            var nodeViewData = m_GraphViewSavedData.TryGetNodeData(baseNode.guid);
            var graphNode = CreateNode(
                baseNode.type,
                nodeViewData.position,
                baseNode.guid, 
                nodeViewData.name, 
                false);
            
            //NOTE: we need to draw first , because in draw method, we will create actual element like ports which we need later when making connections.
            graphNode.Draw();
            
            AddElement(graphNode);
            m_FrpNodeMap.Add(graphNode.ID, graphNode);
            
            // add group 
            if (!string.IsNullOrEmpty(nodeViewData.groupGuid))
            {
                var groupViewData = m_GraphViewSavedData.TryGetGroupData(nodeViewData.groupGuid);
                
                if (m_FrpGroupMap.TryGetValue(nodeViewData.groupGuid, out var groupElement))
                {
                    // do nothing
                }
                else
                {
                    groupElement = CreateGroup(groupViewData.title, groupViewData.position, nodeViewData.groupGuid);
                    AddElement(groupElement);
                    m_FrpGroupMap.Add(groupElement.ID, groupElement);
                }
                
                groupElement.AddElement(graphNode);
                
            }
        }

        private void DrawPassNodeList(List<FlowRenderGraphData.RenderRequestNode> nodeList)
        {
            for (int i = 0; i < nodeList.Count; ++i)
            {
                DrawNode(nodeList[i]);
            }
        }

        private void DrawNodeConnections(string entryID)
        {
            void AddEdgeManually(Edge edge)
            {
                edge.userData = true;
                AddElement(edge);
            };

            void DrawAssignment(string assignID, string kBlockName, FRPRenderRequestNode renderRequestNode)
            {
                if (!string.IsNullOrEmpty(assignID))
                {
                    var parameterNode = m_FrpNodeMap[assignID];
                    Debug.Assert(parameterNode != null, $"parameterNode {assignID} is null.");
                    AddEdgeManually(parameterNode.FlowOut.ConnectTo(renderRequestNode.GetBlockPort(kBlockName)));
                }
            }
            
            for (int i = 0; i < nodeList.Count; ++i)
            {
                
                var graphNodeData = nodeList[i];
                if (m_FrpNodeMap.TryGetValue(graphNodeData.guid, out var graphNode))
                {
                    FRPRenderRequestNode renderRequestNode = (FRPRenderRequestNode)graphNode;
                    // 1. draw node flow connections
                    
                    // for branch node, may exist multiple flow-outs
                    for (int j = 0; j < graphNodeData.flowOut.Count; ++j)
                    {
                        var targetNodeID = graphNodeData.flowOut[j];
                        var targetNode = m_FrpNodeMap[targetNodeID];
                        if (targetNode == null)
                        {
                            Debug.LogError($"[GraphView.Draw] Node {graphNodeData.guid} flow out connection target {targetNodeID} is null ");
                            continue;
                        }
            
                        // add edge
                        AddEdgeManually(renderRequestNode.FlowOut.ConnectTo(targetNode.FlowIn));
                    }
                    
                    // 2. draw node culling assignment
                    DrawAssignment(graphNodeData.culling, FRPRenderRequestNode.kCullingFoldoutName, renderRequestNode);
                    
                    // 3. draw node state assignment
                    DrawAssignment(graphNodeData.state, FRPRenderRequestNode.kStateFoldoutName, renderRequestNode);
                    
                    // 4. draw node material assignment
                    DrawAssignment(graphNodeData.material, FRPRenderRequestNode.kMaterialFoldoutName, renderRequestNode);
                    
                    // 5. draw node camera assignment
                    DrawAssignment(graphNodeData.camera, FRPRenderRequestNode.kCameraFoldoutName, renderRequestNode);
                }
            }
        }
        
        private void DrawCullingNodeList(List<FlowRenderGraphData.CullingParameterNode> nodeList)
        {
            for (int i = 0; i < nodeList.Count; ++i)
            {
                DrawNode(nodeList[i]);
            }
        }
        
        private void DrawRenderStateNodeList(List<FlowRenderGraphData.RenderStateNode> nodeList)
        {
            for (int i = 0; i < nodeList.Count; ++i)
            {
                DrawNode(nodeList[i]);
            }
        }
        
        private void DrawMaterialNodeList(List<FlowRenderGraphData.MaterialParameterNode> nodeList)
        {
            for (int i = 0; i < nodeList.Count; ++i)
            {
                DrawNode(nodeList[i]);
            }
        }
        
        private void DrawCameraNodeList(List<FlowRenderGraphData.CameraParameterNode> nodeList)
        {
            for (int i = 0; i < nodeList.Count; ++i)
            {
                DrawNode(nodeList[i]);
            }
        }
        
        private void Draw()
        {
            
            RemoveAllElements();

            m_FrpNodeMap.Clear();
            m_FrpGroupMap.Clear();
            
            // 0. draw entry node
            DrawNode(m_CurrentRenderGraphData.Entry);
            
            // 1. draw nodes 
            var passNodeList = m_CurrentRenderGraphData.PassNodeList;
            var cullingNodeList = m_CurrentRenderGraphData.CullingNodeList;
            var renderStateNodeList = m_CurrentRenderGraphData.RenderStateNodeList;
            var materialNodeList = m_CurrentRenderGraphData.MaterialNodeList;
            var cameraNodeList = m_CurrentRenderGraphData.CameraNodeList;
           
            DrawPassNodeList(passNodeList);
            DrawCullingNodeList(cullingNodeList);
            DrawRenderStateNodeList(renderStateNodeList);
            DrawMaterialNodeList(materialNodeList);
            DrawCameraNodeList(cameraNodeList);
           
            // 2. set flow connections
            DrawNodeConnections(m_CurrentRenderGraphData.EntryID);
           
            // 3. draw empty group
            DrawEmptyGroup();
           
        }
    }
}