using System;
using System.Collections.Generic;
using UnityEngine;

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
        struct GraphViewData
        {
            public FRPGraphViewSavedData so;
            [SerializeField]
            public FRPSerializedDictionary<string, NodeData> nodeMaps;
            [SerializeField]
            public FRPSerializedDictionary<string, GroupData> groupMaps;

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
        
        //              graphView guid - GraphViewData
        [SerializeField]
        private FRPSerializedDictionary<string, GraphViewData> m_Datas = new FRPSerializedDictionary<string, GraphViewData>();

        private GraphViewData m_CurrentSelectedViewData;
        
        public void CreateViewDataIfNotExisit(string graphGUID)
        {
            if (!m_Datas.TryGetValue(graphGUID, out var viewData))
            {
                Debug.Log($"Create A new GraphViewData:{graphGUID}");
                viewData = new GraphViewData()
                {
                    so = this,
                    nodeMaps = new FRPSerializedDictionary<string, NodeData>(),
                    groupMaps = new FRPSerializedDictionary<string, GroupData>()
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

        public void AddNewNode(string guid, NodeData nodeData)
        {
            m_CurrentSelectedViewData.AddNewNode(guid, nodeData);
            FRPAssetsUtility.SaveAsset(this);
        }
    }
}