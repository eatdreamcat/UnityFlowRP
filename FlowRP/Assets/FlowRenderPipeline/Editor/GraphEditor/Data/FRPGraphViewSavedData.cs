using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.FlowPipeline;

namespace UnityEditor.Rendering.FlowPipeline
{
    /**
     *
     *   note: for GraphViewSavedData, we only focus on the view side informations.
     *         Relation informations between nodes and Node Data (like properties, type , connections ... ) we will save to FlowRenderGraphData as these data are necessary on runtime.
     * 
     *   FRPGraphViewSavedData
     *      RendererData - GraphView
     *        NodesMap - GUID   
     *          Position
     *          Group - GUID
     *          Size ?
     *        GroupsMap
     *          Position
     *          Name
     *          Size ? 
     *
     *   
     */
    public class FRPGraphViewSavedData : ScriptableObject
    {
        [Serializable]
        public struct NodeData
        {
            public string name;
            public Vector2 position;
            public string groupGuid;
        }
        
        [Serializable]
        public struct GroupData
        {
            public Vector2 position;
            public string name;
        }

        [Serializable]
        public sealed class NodeDataDictionary : FRPSerializedDictionary<string, NodeData> { }
        [Serializable]
        public  sealed class GroupDataDictionary : FRPSerializedDictionary<string, GroupData> { }

        [Serializable]
        public class GraphViewData
        {
            [HideInInspector]
            private FRPGraphViewSavedData m_So;
            [SerializeField]
            private NodeDataDictionary m_NodeMap;
            [SerializeField]
            private GroupDataDictionary m_GroupMap;

            public GraphViewData(FRPGraphViewSavedData so, NodeDataDictionary nodeMap, GroupDataDictionary groupMap)
            {
                m_So = so;
                m_NodeMap = nodeMap;
                m_GroupMap = groupMap;
            }
            
            public void AddDefaultEntry(string guid, string name)
            {
                if (m_NodeMap.ContainsKey(guid))
                {
                    Debug.LogError($"[ViewSavedData.AddDefaultEntry] Node {name} is already exist! ");
                    return;
                }
                
                m_NodeMap.Add(guid, new NodeData()
                {
                    name = name,
                    position = new Vector2(100, 200),
                    groupGuid = ""
                });
            }

            public NodeData TryGetNodeData(string guid)
            {
                if (!m_NodeMap.TryGetValue(guid, out var nodeData))
                {
                    Debug.LogError($"[ViewSavedData.TryGetNodeData] Node {guid} Data missing. ");
                }

                return nodeData;
            }

            public void UpdateNodePosition(string guid, Vector2 position)
            {
                if (m_NodeMap.TryGetValue(guid, out var nodeData))
                {
                    nodeData.position = position;
                    m_NodeMap[guid] = nodeData;
                    
                    return;
                }
                
                Debug.LogError($"[ViewSavedData.UpdateNodePosition] Node {guid} Data missing. ");
            }
            
            public void UpdateNodeName(string guid, string newName)
            {
                if (m_NodeMap.TryGetValue(guid, out var nodeData))
                {
                    nodeData.name = newName;
                    m_NodeMap[guid] = nodeData;
                    
                    return;
                }
                
                Debug.LogError($"[ViewSavedData.UpdateNodeName] Node {guid} Data missing. ");
            }
            
            
            public void AddNewNode(string guid, NodeData nodeData)
            {
                if (m_NodeMap.ContainsKey(guid))
                {
                    Debug.LogError($"[ViewSavedData.AddNewNode] Node {guid} already exisit.");
                    return;
                }
                
                m_NodeMap.Add(guid, nodeData);
            }

            public void DeleteNode(string guid)
            {
                if (m_NodeMap.ContainsKey(guid))
                {
                    m_NodeMap.Remove(guid);
                    
                    return;
                }
                
                Debug.LogError($"[ViewSavedData.DeleteNode] Node {guid} already exisit.");
            }
            
            
        }

        [Serializable]//                                            graphView guid - GraphViewData
        internal sealed class GraphViewDataDictionary : FRPSerializedDictionary<string, GraphViewData> { }

        
        [SerializeField]
        private GraphViewDataDictionary m_Datas = new GraphViewDataDictionary();

        private GraphViewData m_CurrentSelectedViewData;
        
        public void CreateViewDataIfNotExisit(string graphGUID)
        {
            if (!m_Datas.TryGetValue(graphGUID, out var viewData))
            {
                Debug.Log($"Create A new GraphViewData:{graphGUID}");
                viewData = new GraphViewData(this, new NodeDataDictionary(), new GroupDataDictionary());
               
                m_Datas.Add(graphGUID, viewData);
                FRPAssetsUtility.SaveAsset(this);
            }

            m_CurrentSelectedViewData = viewData;
        }

        public void AddDefaultEntry(string guid, string name)
        {
            m_CurrentSelectedViewData.AddDefaultEntry(guid, name);
            FRPAssetsUtility.SaveAsset(this);
        }
        
        public NodeData TryGetNodeData(string guid)
        {
            return m_CurrentSelectedViewData.TryGetNodeData(guid);
            FRPAssetsUtility.SaveAsset(this);
        }

        public void UpdateNodePosition(string guid, Vector2 position)
        {
            m_CurrentSelectedViewData.UpdateNodePosition(guid, position);
            FRPAssetsUtility.SaveAsset(this);
        }
        
        public void UpdateNodeName(string guid, string name)
        {
            m_CurrentSelectedViewData.UpdateNodeName(guid, name);
            FRPAssetsUtility.SaveAsset(this);
        }

        public void AddNewNode(string guid, NodeData nodeData)
        {
            m_CurrentSelectedViewData.AddNewNode(guid, nodeData);
            FRPAssetsUtility.SaveAsset(this);
        }

        public void DeleteNode(FRPNodeBase nodeToDelete)
        {
            m_CurrentSelectedViewData.DeleteNode(nodeToDelete.ID);
            FRPAssetsUtility.SaveAsset(this);
        }
    }
}