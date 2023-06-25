using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering.FlowPipeline;
using UnityEngine.UIElements;

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
            void AddEdgeManually(Edge edge, bool isMainFlow = false)
            {
                edge.userData = true;
                AddElement(edge);
                
                if (isMainFlow)
                {
                    edge.edgeControl.inputColor = new Color(199,299,333,255);
                    edge.edgeControl.edgeWidth = 100;
                    edge.MarkDirtyRepaint();
                }
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

            var flowNode = m_CurrentRenderGraphData.TryFlowNode(entryID);
            Debug.Assert(flowNode.dataType == FlowRenderGraphData.FRPNodeType.Entry, "Entry Flow not binding to Entry Node.");
          
            if (flowNode.flowOut.Count > 0)
            {
                // Caution: this is not a safe coding 
                bool canBreakLoop = false;
                while (flowNode != null && canBreakLoop == false)
                {
                    if (!string.IsNullOrEmpty(flowNode.dataID) && m_FrpNodeMap.TryGetValue(flowNode.dataID, out var graphNode))
                    {
                        switch (flowNode.dataType)
                        {
                            case FlowRenderGraphData.FRPNodeType.Entry:
                            {
                                var targetNodeID = flowNode.flowOut[0];
                                var targetNode = m_FrpNodeMap[targetNodeID];
                                Debug.Assert(targetNode != null, $"[GraphView.Draw] Node {flowNode.guid} flow out connection target {targetNodeID} is null ");
                                // add edge
                                AddEdgeManually(graphNode.FlowOut.ConnectTo(targetNode.FlowIn), true);
                                
                                if (flowNode.flowOut.Count > 0)
                                {
                                    // pass node only has one flow output.
                                    flowNode = m_CurrentRenderGraphData.TryFlowNode(flowNode.flowOut[0]);
                                }
                                else
                                {
                                    canBreakLoop = true;
                                }
                            }
                                break;
                            case FlowRenderGraphData.FRPNodeType.FRPRenderRequestNode:
                            {
                                var graphNodeData = m_CurrentRenderGraphData.TryGetRenderPassNode(flowNode.dataID);
                            
                                FRPRenderRequestNode renderRequestNode = (FRPRenderRequestNode)graphNode;

                                #region Draw Assignment

                                // 1. culling assignment
                                DrawAssignment(graphNodeData.culling, FRPRenderRequestNode.kCullingFoldoutName, renderRequestNode);
                
                                // 2. state assignment
                                DrawAssignment(graphNodeData.state, FRPRenderRequestNode.kStateFoldoutName, renderRequestNode);
                
                                // 3. material assignment
                                DrawAssignment(graphNodeData.material, FRPRenderRequestNode.kMaterialFoldoutName, renderRequestNode);
                
                                // 4. camera assignment
                                DrawAssignment(graphNodeData.camera, FRPRenderRequestNode.kCameraFoldoutName, renderRequestNode);

                                #endregion
                                
                                // draw flow edge
                                if (flowNode.flowOut.Count > 0)
                                {
                                    var targetNodeID = flowNode.flowOut[0];
                                    var targetNode = m_FrpNodeMap[targetNodeID];
                                    Debug.Assert(targetNode != null, $"[GraphView.Draw] Node {flowNode.guid} flow out connection target {targetNodeID} is null ");
                                    // add edge
                                    AddEdgeManually(renderRequestNode.FlowOut.ConnectTo(targetNode.FlowIn), true);
                                    
                                    // pass node only has one flow output.
                                    flowNode = m_CurrentRenderGraphData.TryFlowNode(flowNode.flowOut[0]);
                                }
                                else
                                {
                                    canBreakLoop = true;
                                }
                            }
                                break;
                            case FlowRenderGraphData.FRPNodeType.FRPBranchNode:
                            {
                                // TODO:
                                canBreakLoop = true;
                            }
                                break;
                            case FlowRenderGraphData.FRPNodeType.FRPLoopNode:
                            {
                                // TODO: 
                                canBreakLoop = true;
                            }
                                break;
                        }
                    }
                    else
                    {
                        Debug.LogError($"Flow node dataID is null or Node bound by dataID not exist : {flowNode.dataID}");
                        break;
                    }

                   
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