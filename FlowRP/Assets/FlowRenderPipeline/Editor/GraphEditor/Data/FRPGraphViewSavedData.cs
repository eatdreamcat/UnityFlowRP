using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

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
        public sealed class NodeDataDictionary : SerializedDictionary<string, NodeData> { }
        [Serializable]
        public  sealed class GroupDataDictionary : SerializedDictionary<string, GroupData> { }

        [Serializable]
        public struct GraphViewData
        {
            public FRPGraphViewSavedData so;
            [SerializeField]
            public NodeDataDictionary nodeMaps;
            [SerializeField]
            public GroupDataDictionary groupMaps;

            public void AddDefaultEntry(string guid, string name)
            {
                if (nodeMaps.ContainsKey(guid))
                {
                    Debug.LogError($"Node {name} is already exist! ");
                    return;
                }
                
                nodeMaps.Add(guid, new NodeData()
                {
                    name = name,
                    position = new Vector2(100, 200),
                    groupGuid = ""
                });
            }

            public NodeData TryGetNodeData(string guid)
            {
                if (!nodeMaps.TryGetValue(guid, out var nodeData))
                {
                    Debug.LogError($"Node {guid} Data missing. ");
                }

                return nodeData;
            }

            public void UpdateNodePosition(string guid, Vector2 position)
            {
                if (nodeMaps.TryGetValue(guid, out var nodeData))
                {
                    nodeData.position = position;
                    nodeMaps[guid] = nodeData;
                    
                    return;
                }
                
                Debug.LogError($"Node {guid} Data missing. ");
            }
            
            public void UpdateNodeName(string guid, string newName)
            {
                if (nodeMaps.TryGetValue(guid, out var nodeData))
                {
                    nodeData.name = newName;
                    nodeMaps[guid] = nodeData;
                    
                    return;
                }
                
                Debug.LogError($"Node {guid} Data missing. ");
            }
            
            
            public void AddNewNode(string guid, NodeData nodeData)
            {
                if (nodeMaps.ContainsKey(guid))
                {
                    Debug.LogError($"Node {guid} already exisit.");
                    return;
                }
                
                nodeMaps.Add(guid, nodeData);
            }
        }

        [Serializable]
        internal sealed class GraphViewDataDictionary : SerializedDictionary<string, GraphViewData> { }

        //              graphView guid - GraphViewData
        [SerializeField]
        private GraphViewDataDictionary m_Datas = new GraphViewDataDictionary();

        private GraphViewData m_CurrentSelectedViewData;
        
        public void CreateViewDataIfNotExisit(string graphGUID)
        {
            if (!m_Datas.TryGetValue(graphGUID, out var viewData))
            {
                Debug.Log($"Create A new GraphViewData:{graphGUID}");
                viewData = new GraphViewData()
                {
                    so = this,
                    nodeMaps = new NodeDataDictionary(),
                    groupMaps = new GroupDataDictionary()
                };
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
    }
}