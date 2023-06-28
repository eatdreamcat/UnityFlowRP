using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.FlowPipeline;
using UnityEngine.UIElements;

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
            public string title;
            public string guid;
        }
        
        [Serializable]
        public struct ViewTransformData
        {
            public Vector3 position;
            public Vector3 scale;
        }

        [Serializable]
        public sealed class NodeDataDictionary : FRPSerializedDictionary<string, NodeData> { }
        [Serializable]
        public  sealed class GroupDataDictionary : FRPSerializedDictionary<string, GroupData> { }

        [Serializable]
        /**
         * Store nodes view data in graphview
         */
        public class GraphViewData
        {
            [HideInInspector]
            private FRPGraphViewSavedData m_So;
            [SerializeField]
            private NodeDataDictionary m_NodeMap;
            [SerializeField]
            private GroupDataDictionary m_GroupMap;

            public List<GroupData> GroupList
            {
                get
                {
                    return m_GroupMap.Values.ToList();
                } 
            }

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
                    // throw new Exception("Node Data missing");
                    Debug.LogError($"[ViewSavedData.TryGetNodeData] Node {guid} Data missing. ");
                }

                return nodeData;
            }
            
            public GroupData TryGetGroupData(string guid)
            {
                if (!m_GroupMap.TryGetValue(guid, out var groupData))
                {
                    Debug.LogError($"[ViewSavedData.TryGetGroupData] Group {guid} Data missing. ");
                }

                return groupData;
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

            /**
             * we still store group position cause there may exist some empty groups...
             */
            public void UpdateGroupPosition(string guid, Vector2 position)
            {
                if (m_GroupMap.TryGetValue(guid, out var groupData))
                {
                    groupData.position = position;
                    m_GroupMap[guid] = groupData;
                    
                    return;
                }
                
                Debug.LogError($"[ViewSavedData.UpdateGroupPosition] Group {guid} Data missing. ");
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
            
            public void UpdateGroupTitle(string guid, string title)
            {
                if (m_GroupMap.TryGetValue(guid, out var groupData))
                {
                    groupData.title = title;
                    m_GroupMap[guid] = groupData;
                    
                    return;
                }
                
                Debug.LogError($"[ViewSavedData.UpdateGroupTitle] Group {guid} Data missing. ");
            }

            public void BindGroup(string nodeID, string groupID)
            {
                if (m_NodeMap.TryGetValue(nodeID, out var nodeData))
                {
                    nodeData.groupGuid = groupID;
                    m_NodeMap[nodeID] = nodeData;
                    
                    return;
                }
                
                Debug.LogError($"[ViewSavedData.BindGroup] Node {nodeID} Data missing. ");
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
            
            public void AddNewGroup(string guid, GroupData groupData)
            {
                if (m_GroupMap.ContainsKey(guid))
                {
                    Debug.LogError($"[ViewSavedData.AddNewGroup] Group {guid} already exisit.");
                    return;
                }
                
                m_GroupMap.Add(guid, groupData);
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
        
        [Serializable]
        internal sealed class GraphViewTransformDictionary : FRPSerializedDictionary<string, ViewTransformData> { }


        [SerializeField] private GraphViewTransformDictionary m_ViewTransformDatas = new GraphViewTransformDictionary();
        
        [SerializeField] private GraphViewDataDictionary m_Datas = new GraphViewDataDictionary();
        
        private GraphViewData m_CurrentSelectedViewData;

        public List<GroupData> GroupViewList
        {
            get
            {
                return m_CurrentSelectedViewData.GroupList;
            }
        }

        public void UpdateViewTransform(string guid, ViewTransformData transform)
        {
            
            if (m_ViewTransformDatas.TryGetValue(guid, out var _))
            {
                m_ViewTransformDatas[guid] = transform;
                
                return;
            }
            
            Debug.LogError($"View {guid} doesn't exist.");
        }
        
        public void CreateViewDataIfNotExisit(string graphGUID, ViewTransformData viewTransformData)
        {
            var dirty = false;
            if (!m_Datas.TryGetValue(graphGUID, out var graphViewData))
            {
                // Debug.Log($"Create A new GraphViewData:{graphGUID}");
                graphViewData = new GraphViewData(this, new NodeDataDictionary(), new GroupDataDictionary());
                m_Datas.Add(graphGUID, graphViewData);
                dirty = true;
            }
           

            if (!m_ViewTransformDatas.TryGetValue(graphGUID, out var transformData))
            {
                // Debug.Log($"Create A new GraphViewTransformData:{graphGUID}");
                m_ViewTransformDatas.Add(graphGUID, viewTransformData);
                dirty = true;
            }
           
            m_CurrentSelectedViewData = graphViewData;

            if (dirty)
            {
                FRPAssetsUtility.SaveAsset(this);
            }
            
        }

        public ViewTransformData TryGetViewTransformData(string viewID)
        {
            if (m_ViewTransformDatas.TryGetValue(viewID, out var transformData))
            {
            }
            else
            {
                Debug.LogError($"View {viewID} transform data not exist.");
            }

            return transformData;
        }

        public void AddDefaultEntry(string guid, string name)
        {
            m_CurrentSelectedViewData.AddDefaultEntry(guid, name);
            FRPAssetsUtility.SaveAsset(this);
        }
        
        public NodeData TryGetNodeData(string guid)
        {
            Debug.Assert(!string.IsNullOrEmpty(guid), "guid is null.");
            
            return m_CurrentSelectedViewData.TryGetNodeData(guid);
        }

        public GroupData TryGetGroupData(string guid)
        {
            Debug.Assert(!string.IsNullOrEmpty(guid), "guid is null.");
            
            return m_CurrentSelectedViewData.TryGetGroupData(guid);
        }

        public void UpdateNodePosition(string guid, Vector2 position)
        {
            Debug.Assert(!string.IsNullOrEmpty(guid), "guid is null.");
            
            m_CurrentSelectedViewData.UpdateNodePosition(guid, position);
            FRPAssetsUtility.SaveAsset(this);
        }

        public void UpdateGroupPosition(string guid, Vector2 position)
        {
            Debug.Assert(!string.IsNullOrEmpty(guid), "guid is null.");
            
            m_CurrentSelectedViewData.UpdateGroupPosition(guid, position);
            FRPAssetsUtility.SaveAsset(this);
        }
        
        public void UpdateNodeName(string guid, string name)
        {
            Debug.Assert(!string.IsNullOrEmpty(guid), "guid is null.");
            
            m_CurrentSelectedViewData.UpdateNodeName(guid, name);
            FRPAssetsUtility.SaveAsset(this);
        }

        public void UpdateGroupTitle(string guid, string title)
        {
            Debug.Assert(!string.IsNullOrEmpty(guid), "guid is null.");
            
            m_CurrentSelectedViewData.UpdateGroupTitle(guid, title);
            FRPAssetsUtility.SaveAsset(this);
        }

        public void AddNewNode(string guid, NodeData nodeData)
        {
            Debug.Assert(!string.IsNullOrEmpty(guid), "guid is null.");
            
            m_CurrentSelectedViewData.AddNewNode(guid, nodeData);
            FRPAssetsUtility.SaveAsset(this);
        }

        public void AddNewGroup(string guid, GroupData groupData)
        {
            Debug.Assert(!string.IsNullOrEmpty(guid), "guid is null.");
            
            m_CurrentSelectedViewData.AddNewGroup(guid, groupData);
            FRPAssetsUtility.SaveAsset(this);
        }

        public void BindGroup(string nodeID, string groupID)
        {
            Debug.Assert(!string.IsNullOrEmpty(nodeID) && !string.IsNullOrEmpty(groupID), "nodeID or groupID is null.");
            
            m_CurrentSelectedViewData.BindGroup(nodeID, groupID);
            FRPAssetsUtility.SaveAsset(this);
        }

        public void DeleteNode(FRPNodeBase nodeToDelete)
        {
            m_CurrentSelectedViewData.DeleteNode(nodeToDelete.ID);
            FRPAssetsUtility.SaveAsset(this);
        }
    }
}