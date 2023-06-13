
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

            m_CurrentRenderGraphData.AddNode(new FlowRenderGraphData.BaseNode()
            {
                name = node.Name,
                guid = node.ID,
                type = node.Type
            });
        }

        public void UpdateNodeTitle(string newTitle, FRPNodeBase node)
        {
            m_GraphViewSavedData.UpdateNodeName(node.ID, newTitle);
            m_CurrentRenderGraphData.UpdateNodeName(node.ID, newTitle);
            
        }
        
        private void Draw()
        {

            RemoveAllElements();
            
           // draw nodes 
           var nodeList = m_CurrentRenderGraphData.NodeList;
           for (int i = 0; i < nodeList.Count; ++i)
           {
               var node = nodeList[i];
               var nodeViewData = m_GraphViewSavedData.TryGetNodeData(node.guid);
               AddElement(CreateNode(node.type, nodeViewData.position, node.guid, node.name));
           }
        }
    }
}