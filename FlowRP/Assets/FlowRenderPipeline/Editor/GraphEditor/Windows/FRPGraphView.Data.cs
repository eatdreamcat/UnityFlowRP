
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering.FlowPipeline;

namespace UnityEditor.Rendering.FlowPipeline
{
    
    public partial class FRPGraphView
    {
        private FRPGraphViewSavedData m_GraphViewSavedData;
        private FlowRenderGraphData m_CurrentRenderGraphData;

        private void RemoveData()
        {
            m_GraphViewSavedData = null;
            m_CurrentRenderGraphData = null;
        }
        public void Save()
        {
            
        }

        public void Reload(FlowRenderGraphData currentRenderGraphData)
        {

            if (m_GraphViewSavedData == null)
            {
                m_GraphViewSavedData =
                    AssetDatabase.LoadAssetAtPath<FRPGraphViewSavedData>(FRPPathUtility
                        .kGraphViewDataSavedFullPath);
                
                /// if not exist ViewData, then create one
                if (m_GraphViewSavedData == null)
                {
                    Debug.Log($"Create A new GraphViewSavedData.");
                    m_GraphViewSavedData = ScriptableObject.CreateInstance<FRPGraphViewSavedData>();
                    AssetDatabase.CreateAsset(m_GraphViewSavedData, FRPPathUtility.kGraphViewDataSavedFullPath);
                    AssetDatabase.Refresh();
                }
            }
            
            
            // TODO: store the preview ?
            
            m_CurrentRenderGraphData = currentRenderGraphData;
            
            // Debug.Log($" Current Selected Graph Data:{m_CurrentRenderGraphData.name}");
            
            m_GraphViewSavedData.CreateViewDataIfNotExisit(m_CurrentRenderGraphData.GraphGuid);
            
            
            if (m_CurrentRenderGraphData.IsEmpty())
            {
                // create a entry point
                FlowRenderGraphData.NodeCreationResult creationResult = m_CurrentRenderGraphData.AddEntryNode();
                m_GraphViewSavedData.AddDefaultEntry(creationResult.guid, creationResult.name);
            }
            
            
            Draw();
        }

        public void UpdateNodePositionData(FRPNodeBase node)
        {
            m_GraphViewSavedData.UpdateNodePosition(node.ID, node.GetPosition().position);
        }

        public void AddNewNodeToData(FRPNodeBase node, Vector2 position)
        {
            
            m_GraphViewSavedData.AddNewNode(node.ID, new FRPGraphViewSavedData.NodeData()
            {
                name = node.Name,
                groupGuid = "",
                position = position
            });

            m_CurrentRenderGraphData.AddNode(FlowUtility.CreateBaseNode(node.Name, node.ID, node.Type));
        }

        public void UpdateNodeTitle(string newTitle, FRPNodeBase node)
        {
            m_GraphViewSavedData.UpdateNodeName(node.ID, newTitle);
            m_CurrentRenderGraphData.UpdateNodeName(node.ID, newTitle);
            
        }
        
        public void DeleteNode(FRPNodeBase nodeToDelete)
        {
            m_CurrentRenderGraphData.DeleteNode(nodeToDelete.ID);
            m_GraphViewSavedData.DeleteNode(nodeToDelete);
        }

        public void AddEdge(Edge edge)
        {
            FRPNodeBase inNode = edge.input.node as FRPNodeBase;
            FRPNodeBase outNode = edge.output.node as FRPNodeBase;
            
            m_CurrentRenderGraphData.AddFlowInOut(inNode.ID, outNode.ID);
        }
        
        public void DeleteEdge(Edge edge)
        {
            FRPNodeBase inNode = edge.input.node as FRPNodeBase;
            FRPNodeBase outNode = edge.output.node as FRPNodeBase;
            m_CurrentRenderGraphData.DeleteFlowInOut(inNode.ID, outNode.ID);
        }

        private void Draw()
        {

            RemoveAllElements();
            
           // 1. draw nodes 
           var nodeList = m_CurrentRenderGraphData.NodeList;

           Dictionary<string, FRPNodeBase> frpNodeMap = new Dictionary<string, FRPNodeBase>();
           
           // add nodes
           for (int i = 0; i < nodeList.Count; ++i)
           {
               var graphNodeData = nodeList[i];
               var nodeViewData = m_GraphViewSavedData.TryGetNodeData(graphNodeData.guid);
              
               var graphNode = CreateNode(graphNodeData.type, nodeViewData.position, graphNodeData.guid, graphNodeData.name, false);
               
               // todo: initialize inout data ( port name ...)
               
               //NOTE: we need to draw first , because in draw method, we will create actual element like ports which we need later when making connections.
               graphNode.Draw();
               AddElement(graphNode);
               frpNodeMap.Add(graphNode.ID, graphNode);
           }
           
           // 2. set flow connections
           for (int i = 0; i < nodeList.Count; ++i)
           {
               var graphNodeData = nodeList[i];
               if (frpNodeMap.TryGetValue(graphNodeData.guid, out var graphNode))
               {
                   // for branch node, may exist multiple flow-outs
                   for (int j = 0; j < graphNodeData.flowOut.Count; ++j)
                   {
                       var targetNodeID = graphNodeData.flowOut[j];
                       var targetNode = frpNodeMap[targetNodeID];
                       if (targetNode == null)
                       {
                           Debug.LogError($"[GraphView.Draw] Node {graphNodeData.guid} flow out connection target {targetNodeID} is null ");
                           continue;
                       }

                       // add edge
                       var edge = graphNode.FlowOut.ConnectTo(targetNode.FlowIn);
                       edge.userData = true;
                       AddElement(edge);
                   }
               }
           }
        }
    }
}