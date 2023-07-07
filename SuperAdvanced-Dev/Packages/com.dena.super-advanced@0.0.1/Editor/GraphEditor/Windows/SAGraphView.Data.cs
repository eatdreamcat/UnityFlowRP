
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering.SuperAdvanced;
using Edge = UnityEditor.Experimental.GraphView.Edge;

namespace UnityEditor.Rendering.SuperAdvanced
{
    
    public partial class SAGraphView
    {
        private SAGraphViewSavedData m_GraphViewSavedData;
        private SARenderGraphData m_CurrentRenderGraphData;

        public SARenderGraphData GraphData => m_CurrentRenderGraphData; 
        private void RemoveData()
        {
            m_GraphViewSavedData = null;
            m_CurrentRenderGraphData = null;
        }
        public void Save()
        {
            
        }

        public void Reload(SARenderGraphData currentRenderGraphData)
        {

            if (m_GraphViewSavedData == null)
            {
                m_GraphViewSavedData =
                    AssetDatabase.LoadAssetAtPath<SAGraphViewSavedData>(SAPathUtility
                        .kGraphViewDataSavedFullPath);
                
                /// if not exist ViewData, then create one
                if (m_GraphViewSavedData == null)
                {
                    Debug.Log($"Create A new GraphViewSavedData.");
                    m_GraphViewSavedData = ScriptableObject.CreateInstance<SAGraphViewSavedData>();
                    AssetDatabase.CreateAsset(m_GraphViewSavedData, SAPathUtility.kGraphViewDataSavedFullPath);
                    AssetDatabase.Refresh();
                }
            }
            
            
            // TODO: store the preview ?
            
            m_CurrentRenderGraphData = currentRenderGraphData;
            
            // Debug.Log($" Current Selected Graph Data:{m_CurrentRenderGraphData.name}");
            
            m_GraphViewSavedData.CreateViewDataIfNotExisit(m_CurrentRenderGraphData.GraphGuid,  new SAGraphViewSavedData.ViewTransformData()
            {
                position = contentContainer.transform.position,
                scale = contentContainer.transform.scale,
            });
            
            
            if (!m_CurrentRenderGraphData.HasEntry())
            {
                // create a entry point
                SARenderGraphData.NodeCreationResult creationResult = m_CurrentRenderGraphData.AddEntryNode();
                m_GraphViewSavedData.AddDefaultEntry(creationResult.guid, creationResult.name);
            }
            
            // set view transform
            var currentTransform = m_GraphViewSavedData.TryGetViewTransformData(m_CurrentRenderGraphData.GraphGuid);

            viewTransform.position = currentTransform.position;
            viewTransform.scale = currentTransform.scale;
            
            Draw();
        }

        public void UpdateNodePositionData(SANodeBase node)
        {
            m_GraphViewSavedData.UpdateNodePosition(node.ID, node.GetPosition().position);
        }

        public void UpdateGroupPositionData(SAGroup group)
        {
            foreach (var element in group.containedElements)
            {
                if (element is SANodeBase)
                {
                    UpdateNodePositionData(element as SANodeBase);
                }
            }
            
            m_GraphViewSavedData.UpdateGroupPosition(group.ID, group.GetPosition().position);
        }

        public void AddNewGroupToData(SAGroup group, Vector2 position)
        {
            m_GraphViewSavedData.AddNewGroup(group.ID, new SAGraphViewSavedData.GroupData()
            {
               title = group.title,
               position = position,
               guid = group.ID
            });
        }

        public void AddNodesToGroup(SAGroup saGroup, List<GraphElement> nodeList)
        {
            foreach (var node in nodeList)
            {
                m_GraphViewSavedData.BindGroup((node as SANodeBase).ID, saGroup.ID);
            }
        }
        
        public SARenderGraphData.BaseNode AddNewNodeToData(string ID, string name, SARenderGraphData.NodeType type, Vector2 position)
        {
            
            m_GraphViewSavedData.AddNewNode(ID, new SAGraphViewSavedData.NodeData()
            {
                name = name,
                groupGuid = "",
                position = position
            });

            return m_CurrentRenderGraphData.AddNode(name, ID, type);
        }

        public void UpdateNodeTitle(string newTitle, SANodeBase node)
        {
            m_GraphViewSavedData.UpdateNodeName(node.ID, newTitle);
            m_CurrentRenderGraphData.UpdateNodeName(node.ID, newTitle, node.Type);
        }
        
        public void UpdateGroupTitle(string newTitle, SAGroup group)
        {
            m_GraphViewSavedData.UpdateGroupTitle(group.ID, newTitle);
        }
        
        public void DeleteNode(SANodeBase nodeToDelete)
        {
            m_CurrentRenderGraphData.DeleteNode(nodeToDelete.ID, nodeToDelete.Type);
            m_GraphViewSavedData.DeleteNode(nodeToDelete);
        }

        public void AddEdge(Edge edge)
        {
            SANodeBase inNode = edge.input.node as SANodeBase;
            SANodeBase outNode = edge.output.node as SANodeBase;

            switch (outNode.Type)
            {
                case SARenderGraphData.NodeType.CullingParameterNode:
                {
                    m_CurrentRenderGraphData.AddCullingAssignment(outNode.ID, inNode.ID);
                }
                    break;

                case SARenderGraphData.NodeType.RenderStateNode:
                {
                    m_CurrentRenderGraphData.AddRenderStateAssignment(outNode.ID, inNode.ID);
                }
                    break;

                case SARenderGraphData.NodeType.RenderMaterialNode:
                {
                    m_CurrentRenderGraphData.AddMaterialAssignment(outNode.ID, inNode.ID);
                }
                    break;

                case SARenderGraphData.NodeType.CameraParameterNode:
                {
                    m_CurrentRenderGraphData.AddCameraAssignment(outNode.ID, inNode.ID);
                }
                    break;
                case SARenderGraphData.NodeType.BufferNode:
                {
                    SABufferNode bufferNode = outNode as SABufferNode;
                    
                    if (bufferNode.BufferNode.bufferType == SARenderGraphData.BufferType.TextureBuffer)
                    {
                        m_CurrentRenderGraphData.AddTextureBufferInputAssignment(outNode.ID, inNode.ID);
                        
                    } else if (bufferNode.BufferNode.bufferType == SARenderGraphData.BufferType.ComputerBuffer)
                    {
                        
                    }
                }
                    break;

                case SARenderGraphData.NodeType.DrawRendererNode:
                case SARenderGraphData.NodeType.DrawFullScreenNode:
                case SARenderGraphData.NodeType.EntryNode:
                {
                    if (inNode.Type == SARenderGraphData.NodeType.BufferNode)
                    {
                        // assign render target
                        SABufferNode bufferNode = inNode as SABufferNode;
                        
                        if (bufferNode.BufferNode.bufferType == SARenderGraphData.BufferType.TextureBuffer)
                        {
                            m_CurrentRenderGraphData.AddTextureBufferOutputAssignment(inNode.ID, outNode.ID);
                        
                        } else if (bufferNode.BufferNode.bufferType == SARenderGraphData.BufferType.ComputerBuffer)
                        {
                        
                        }
                    }
                    else
                    {
                        edge.style.color = Color.cyan;
                        m_CurrentRenderGraphData.AddFlowInOut(outNode.ID, inNode.ID);
                    }
                }
                    break;
            }
          
        }
        
        public void DeleteEdge(Edge edge)
        {
            SANodeBase inNode = edge.input.node as SANodeBase;
            SANodeBase outNode = edge.output.node as SANodeBase; 
            
            switch (outNode.Type)
            {
                case SARenderGraphData.NodeType.CullingParameterNode:
                {
                    m_CurrentRenderGraphData.DeleteCullingAssignment(outNode.ID, inNode.ID);
                }
                    break;

                case SARenderGraphData.NodeType.RenderStateNode:
                {
                    m_CurrentRenderGraphData.DeleteRenderStateAssignment(outNode.ID, inNode.ID);
                }
                    break;

                case SARenderGraphData.NodeType.RenderMaterialNode:
                {
                    m_CurrentRenderGraphData.DeleteMaterialAssignment(outNode.ID, inNode.ID);
                }
                    break;

                case SARenderGraphData.NodeType.CameraParameterNode:
                {
                    m_CurrentRenderGraphData.DeleteCameraAssignment(outNode.ID, inNode.ID);
                }
                    break;
                case SARenderGraphData.NodeType.BufferNode:
                {
                    SABufferNode bufferNode = outNode as SABufferNode;
                    
                    if (bufferNode.BufferNode.bufferType == SARenderGraphData.BufferType.TextureBuffer)
                    {
                        m_CurrentRenderGraphData.DeleteTextureBufferInputAssignment(outNode.ID, inNode.ID);
                        
                    } else if (bufferNode.BufferNode.bufferType == SARenderGraphData.BufferType.ComputerBuffer)
                    {
                        
                    }
                }
                    break;

                case SARenderGraphData.NodeType.DrawRendererNode:
                case SARenderGraphData.NodeType.EntryNode:
                {
                    if (inNode.Type == SARenderGraphData.NodeType.BufferNode)
                    {
                        // assign render target
                        SABufferNode bufferNode = inNode as SABufferNode;
                        
                        if (bufferNode.BufferNode.bufferType == SARenderGraphData.BufferType.TextureBuffer)
                        {
                            m_CurrentRenderGraphData.DeleteTextureBufferOutputAssignment(inNode.ID, outNode.ID);
                        
                        } else if (bufferNode.BufferNode.bufferType == SARenderGraphData.BufferType.ComputerBuffer)
                        {
                        
                        }
                    }
                    else
                    {
                        m_CurrentRenderGraphData.DeleteFlowInOut(outNode.ID, inNode.ID);
                    }
                    
                }
                    break;
            }
        }

       
    }
}