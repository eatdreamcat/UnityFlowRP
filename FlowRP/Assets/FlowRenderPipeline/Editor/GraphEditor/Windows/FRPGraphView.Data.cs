
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
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
            
            
            if (!m_CurrentRenderGraphData.HasEntry())
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

        public void UpdateGroupPositionData(FRPGroup group)
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

        public void AddNewGroupToData(FRPGroup group, Vector2 position)
        {
            m_GraphViewSavedData.AddNewGroup(group.ID, new FRPGraphViewSavedData.GroupData()
            {
               title = group.title,
               position = position,
               guid = group.ID
            });
        }

        public void AddNodesToGroup(FRPGroup frpGroup, List<GraphElement> nodeList)
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

            m_CurrentRenderGraphData.AddNode(node.Name, node.ID, node.Type);
        }

        public void UpdateNodeTitle(string newTitle, FRPNodeBase node)
        {
            m_GraphViewSavedData.UpdateNodeName(node.ID, newTitle);
            m_CurrentRenderGraphData.UpdateNodeName(node.ID, newTitle);
        }
        
        public void UpdateGroupTitle(string newTitle, FRPGroup group)
        {
            m_GraphViewSavedData.UpdateGroupTitle(group.ID, newTitle);
        }
        
        public void DeleteNode(FRPNodeBase nodeToDelete)
        {
            m_CurrentRenderGraphData.DeleteNode(nodeToDelete.ID, nodeToDelete.Type);
            m_GraphViewSavedData.DeleteNode(nodeToDelete);
        }

        public void AddEdge(Edge edge)
        {
            FRPNodeBase inNode = edge.input.node as FRPNodeBase;
            FRPNodeBase outNode = edge.output.node as FRPNodeBase;

            switch (outNode.Type)
            {
                case FlowRenderGraphData.FRPNodeType.FRPCullingParameterNode:
                {
                    m_CurrentRenderGraphData.AddCullingAssignment(outNode.ID, inNode.ID);
                }
                    break;

                case FlowRenderGraphData.FRPNodeType.FRPRenderStateNode:
                {
                    m_CurrentRenderGraphData.AddRenderStateAssignment(outNode.ID, inNode.ID);
                }
                    break;

                case FlowRenderGraphData.FRPNodeType.FRPRenderMaterialNode:
                {
                    m_CurrentRenderGraphData.AddMaterialAssignment(outNode.ID, inNode.ID);
                }
                    break;

                case FlowRenderGraphData.FRPNodeType.FRPCameraParameterNode:
                {
                    m_CurrentRenderGraphData.AddCameraAssignment(outNode.ID, inNode.ID);
                }
                    break;

                case FlowRenderGraphData.FRPNodeType.FRPRenderRequestNode:
                case FlowRenderGraphData.FRPNodeType.Entry:    
                {
                    m_CurrentRenderGraphData.AddFlowInOut(outNode.ID, inNode.ID);
                }
                    break;
            }
          
        }
        
        public void DeleteEdge(Edge edge)
        {
            FRPNodeBase inNode = edge.input.node as FRPNodeBase;
            FRPNodeBase outNode = edge.output.node as FRPNodeBase; 
            
            switch (outNode.Type)
            {
                case FlowRenderGraphData.FRPNodeType.FRPCullingParameterNode:
                {
                    m_CurrentRenderGraphData.DeleteCullingAssignment(outNode.ID, inNode.ID);
                }
                    break;

                case FlowRenderGraphData.FRPNodeType.FRPRenderStateNode:
                {
                    m_CurrentRenderGraphData.DeleteRenderStateAssignment(outNode.ID, inNode.ID);
                }
                    break;

                case FlowRenderGraphData.FRPNodeType.FRPRenderMaterialNode:
                {
                    m_CurrentRenderGraphData.DeleteMaterialAssignment(outNode.ID, inNode.ID);
                }
                    break;

                case FlowRenderGraphData.FRPNodeType.FRPCameraParameterNode:
                {
                    m_CurrentRenderGraphData.DeleteCameraAssignment(outNode.ID, inNode.ID);
                }
                    break;

                case FlowRenderGraphData.FRPNodeType.FRPRenderRequestNode:
                {
                    m_CurrentRenderGraphData.DeleteFlowInOut(outNode.ID, inNode.ID);
                }
                    break;
            }
        }

       
    }
}