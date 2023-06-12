using System.Linq;
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
                    m_GraphViewSavedData = ScriptableObject.CreateInstance<FRPGraphViewSavedData>();
                    AssetDatabase.CreateAsset(m_GraphViewSavedData, FRPPathUtility.kGraphViewDataSavedFullPath);
                    AssetDatabase.Refresh();
                }
            }
            
            
            // TODO: store the preview ?
            
            m_CurrentRenderGraphData = currentRenderGraphData;
            
            m_GraphViewSavedData.CreateViewDataIfNotExisit(m_CurrentRenderGraphData.GUID);
            
            
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

        private void Draw()
        {

            RemoveAllElements();
            
           // draw nodes 
           var nodeList = m_CurrentRenderGraphData.NodeList;
           for (int i = 0; i < nodeList.Count; ++i)
           {
               var node = nodeList[i];
               var nodeViewData = m_GraphViewSavedData.TryGetNodeData(node.guid);
               AddElement(CreateNode(node.type, nodeViewData.position, node.guid));
           }
        }
    }
}