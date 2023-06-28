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
        
        private FRPNodeBase DrawNode(FlowRenderGraphData.BaseNode baseNode)
        {
            var nodeViewData = m_GraphViewSavedData.TryGetNodeData(baseNode.guid);
            var graphNode = CreateNode(
                nodeViewData.position, baseNode, 
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

            return graphNode;
        }

        private void DrawRendererNodeList(List<FlowRenderGraphData.DrawRendererNode> nodeList)
        {
            for (int i = 0; i < nodeList.Count; ++i)
            {
                FRPDrawRendererNode renderRequestNode = (FRPDrawRendererNode)DrawNode(nodeList[i]);
                
                #region Draw Assignment

                var graphNodeData = m_CurrentRenderGraphData.TryGetRenderPassNode(nodeList[i].guid);
                
                // 1. culling assignment
                DrawAssignment(graphNodeData.culling, FRPDrawRendererNode.kCullingFoldoutName, renderRequestNode);
                
                // 2. state assignment
                DrawAssignment(graphNodeData.state, FRPDrawRendererNode.kStateFoldoutName, renderRequestNode);
                
                // 3. material assignment
                DrawAssignment(graphNodeData.material, FRPDrawRendererNode.kMaterialFoldoutName, renderRequestNode);
                
                // 4. camera assignment
                DrawAssignment(graphNodeData.camera, FRPDrawRendererNode.kCameraFoldoutName, renderRequestNode);

                #endregion
            }
        }

        private void DrawFullScreenNodeList(List<FlowRenderGraphData.DrawFullScreenNode> nodeList)
        {
            for (int i = 0; i < nodeList.Count; ++i)
            {
                DrawNode(nodeList[i]);
            }
        }

        void AddEdgeManually(Edge edge, bool isMainFlow = false)
        {
            edge.userData = true;
            AddElement(edge);
                
            if (isMainFlow)
            {
                edge.edgeControl.drawFromCap = true;
                edge.edgeControl.inputColor = new Color(255,0,0,255);
                edge.edgeControl.fromCapColor = new Color(255,0,0,255);
                edge.edgeControl.edgeWidth = 100;
                edge.edgeControl.MarkDirtyRepaint();
                Debug.Assert( edge.UpdateEdgeControl(), "edge.UpdateEdgeControl()");
            }
        }

        void DrawAssignment(string assignID, string kBlockName, FRPDrawRendererNode renderRequestNode)
        {
            if (!string.IsNullOrEmpty(assignID))
            {
                var parameterNode = m_FrpNodeMap[assignID];
                Debug.Assert(parameterNode != null, $"parameterNode {assignID} is null.");
                AddEdgeManually(parameterNode.FlowOut.ConnectTo(renderRequestNode.GetBlockPort(kBlockName)));
            }
        }
        
        private void DrawNodeConnections(string entryID)
        {
            var flowNode = m_CurrentRenderGraphData.TryFlowNode(entryID);
            Debug.Assert(flowNode.dataType == FlowRenderGraphData.FRPNodeType.Entry, "Entry Flow not binding to Entry Node.");
          
            // entry node flow...
            if (flowNode.flowOut.Count > 0)
            {
                // Caution: this is not a safe coding 
                bool canBreakLoop = false;
                int loopCount = 0;
                const int MaxLoopCount = 1000;
                while (flowNode != null && canBreakLoop == false && ++loopCount <= MaxLoopCount)
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
                            
                            case FlowRenderGraphData.FRPNodeType.FRPDrawRendererNode:
                            case FlowRenderGraphData.FRPNodeType.FRPDrawFullScreenNode:
                            {
                                // draw flow edge
                                if (flowNode.flowOut.Count > 0)
                                {
                                    var targetNodeID = flowNode.flowOut[0];
                                    var targetNode = m_FrpNodeMap[targetNodeID];
                                    Debug.Assert(targetNode != null, $"[GraphView.Draw] Node {flowNode.guid} flow out connection target {targetNodeID} is null ");
                                    // add edge
                                    AddEdgeManually(graphNode.FlowOut.ConnectTo(targetNode.FlowIn), true);
                                    
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
                
                Debug.Assert(loopCount <= MaxLoopCount, "A deadloop occured while draw connections, please check is there a new Node type not be managed.");
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
            var passNodeList = m_CurrentRenderGraphData.DrawRendererNodeList;
            var fullScreenNodeList = m_CurrentRenderGraphData.DrawFullScreenNodeList;
            var cullingNodeList = m_CurrentRenderGraphData.CullingNodeList;
            var renderStateNodeList = m_CurrentRenderGraphData.RenderStateNodeList;
            var materialNodeList = m_CurrentRenderGraphData.MaterialNodeList;
            var cameraNodeList = m_CurrentRenderGraphData.CameraNodeList;
            
            // draw parameter node first cause we will draw parameter assignment when drawing pass node
            DrawCullingNodeList(cullingNodeList);
            DrawRenderStateNodeList(renderStateNodeList);
            DrawMaterialNodeList(materialNodeList);
            DrawCameraNodeList(cameraNodeList);

            // draw pass node and it's assignments
            DrawRendererNodeList(passNodeList);
            DrawFullScreenNodeList(fullScreenNodeList);
            
            
            // 2. set flow connections
            DrawNodeConnections(m_CurrentRenderGraphData.EntryID);
           
            // 3. draw empty group
            DrawEmptyGroup();
           
        }
    }
}