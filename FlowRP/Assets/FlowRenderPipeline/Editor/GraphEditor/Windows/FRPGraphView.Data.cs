
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.Rendering.FlowPipeline;
using Edge = UnityEditor.Experimental.GraphView.Edge;

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
            
            m_GraphViewSavedData.CreateViewDataIfNotExisit(m_CurrentRenderGraphData.GraphGuid,  new FRPGraphViewSavedData.ViewTransformData()
            {
                position = contentContainer.transform.position,
                scale = contentContainer.transform.scale,
            });
            
            
            if (m_CurrentRenderGraphData.IsEmpty())
            {
                // create a entry point
                FlowRenderGraphData.NodeCreationResult creationResult = m_CurrentRenderGraphData.AddEntryNode();
                m_GraphViewSavedData.AddDefaultEntry(creationResult.guid, creationResult.name);
            }
            
            // set view transform
            var currentTransform = m_GraphViewSavedData.TryGetViewTransformData(m_CurrentRenderGraphData.GraphGuid);

            viewTransform.position = currentTransform.position;
            viewTransform.scale = currentTransform.scale;
            
            Draw();
        }

        public void UpdateNodePositionData(FRPNodeBase node)
        {
            m_GraphViewSavedData.UpdateNodePosition(node.ID, node.GetPosition().position);
        }

        public void UpdateGroupPositionData(FRPNodeGroup group)
        {
            foreach (var element in group.containedElements)
            {
                if (element is FRPNodeBase)
                {
                    UpdateNodePositionData(element as FRPNodeBase);
                }
            }
            
            m_GraphViewSavedData.UpdateGroupPosition(group.ID, group.GetPosition().position);
        }

        public void AddNewGroupToData(FRPNodeGroup group, Vector2 position)
        {
            m_GraphViewSavedData.AddNewGroup(group.ID, new FRPGraphViewSavedData.GroupData()
            {
               title = group.title,
               position = position,
               guid = group.ID
            });
        }

        public void AddNodesToGroup(FRPNodeGroup frpGroup, List<GraphElement> nodeList)
        {
            foreach (var node in nodeList)
            {
                m_GraphViewSavedData.BindGroup((node as FRPNodeBase).ID, frpGroup.ID);
            }
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
        
        public void UpdateGroupTitle(string newTitle, FRPNodeGroup group)
        {
            m_GraphViewSavedData.UpdateGroupTitle(group.ID, newTitle);
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
           Dictionary<string, FRPNodeGroup> frpGroupMap = new Dictionary<string, FRPNodeGroup>();
           
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
               
               // add group 
               if (!string.IsNullOrEmpty(nodeViewData.groupGuid))
               {
                   var groupViewData = m_GraphViewSavedData.TryGetGroupData(nodeViewData.groupGuid);

                   if (frpGroupMap.TryGetValue(nodeViewData.groupGuid, out var groupElement))
                   {
                       // do nothing
                   }
                   else
                   {
                       groupElement = CreateGroup(groupViewData.title, groupViewData.position, nodeViewData.groupGuid);
                       AddElement(groupElement);
                       frpGroupMap.Add(groupElement.ID, groupElement);
                   }
                   
                   
                   groupElement.AddElement(graphNode);
                   
               }
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
           
           // 3. draw empty group
           var groupViewList = m_GraphViewSavedData.GroupViewList;
           foreach (var groupData in groupViewList)
           {
               if (!frpGroupMap.TryGetValue(groupData.guid, out var groupElement))
               {
                   groupElement = CreateGroup(groupData.title, groupData.position, groupData.guid);
                   AddElement(groupElement);
                   frpGroupMap.Add(groupElement.ID, groupElement);
               }
           }
        }
    }
}