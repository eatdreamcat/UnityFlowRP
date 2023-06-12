

using System;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;

namespace UnityEngine.Rendering.FlowPipeline
{
    public class FlowRenderGraphData : ScriptableObject, ISerializationCallbackReceiver
    {
        
        public enum FRPNodeType
        {
            Entry,
            // flow control
            FRPBranchNode,
            FPRLoopNode,
            
            // render pass
            FRPRenderRequestNode,
            
            // render target
            FRPRenderTargetNode,
            
            // render resources： TextureBuffer, DepthStencilBuffer, ComputeBuffer
            FRPResourceNode,
            FRPRenderTextureNode,
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812")]
        internal class CreateRenderGraphDataAsset : EndNameEditAction
        {
            public override void Action(int instanceId, string path, string resourceFile)
            {
                //Create asset
                FlowRenderGraphData data = CreateInstance<FlowRenderGraphData>();
                data.InitGUID();
                AssetDatabase.CreateAsset(data, path);
                FlowUtility.SaveAsset(data);
                ResourceReloader.ReloadAllNullIn(data, FlowUtility.GetFlowRenderPipelinePath());
            }
        }
        
        
        [MenuItem("Assets/Create/Rendering/Render Graph Data Asset", priority = CoreUtils.Sections.section2 + CoreUtils.Priorities.assetsCreateRenderingMenuPriority + 1)]
        static void Create()
        {
            if (!Application.isPlaying)
            {
                // todo : reload all
            }
            
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, CreateInstance<CreateRenderGraphDataAsset>(),
                "New Render Graph Data Asset.asset",null, null);
        }
        
        
        ///
        ///
        ///    GraphData
        ///
        ///    NodeList
        ///      Node - use guid to map FRPGraphViewSavedData
        ///
        /// 
        ///    ConnectionsMap
        ///      node-connecttions
        ///      guid - guid list
        ///    NodeInputMap
        ///      node-(input list)
        ///      guid - guid list
        ///    NodeOutputMap
        ///      node-(output list)
        ///      guid - guid list
        ///
        ///

        public struct NodeCreationResult
        {
            public string guid;
            public string name;
        }
        
        [Serializable]
        public class BaseNode
        {
            /* using for tracking layout data*/
            public string guid;
            public string name;
            public FRPNodeType type;
        }
        

        [SerializeField]
        private List<BaseNode> m_NodeList = new List<BaseNode>();

        public List<BaseNode> NodeList
        {
            get
            {
                return m_NodeList;
            }
        }

        [SerializeField] public string GUID;

        public void InitGUID()
        {
            GUID = Guid.NewGuid().ToString();
        }
        
        public bool IsEmpty()
        {
            return m_NodeList.Count <= 0;
        }
        
        public NodeCreationResult AddEntryNode()
        {
            var newNode = new BaseNode()
            {
                guid = Guid.NewGuid().ToString(),
                type = FRPNodeType.Entry,
                name = "Entry"
            };
            m_NodeList.Add(newNode);
            
            FlowUtility.SaveAsset(this);
            
            return new NodeCreationResult()
            {
                name = "Entry",
                guid = newNode.guid
            };
        }

        // TODO 
        public void AddNode(BaseNode newNode)
        {
            // test code 
            m_NodeList.Add(newNode);
            
            
            switch (newNode.type)
            {
                // render request
                case FRPNodeType.FRPRenderRequestNode:
                {
                    
                }
                    break;
                
                // render resources
                case FRPNodeType.FRPResourceNode:
                {
                  
                   
            
                   
                }
                    break;
                
                
                /// flow control
                case FRPNodeType.FPRLoopNode:
                {
                    
                }
                    break;
                case FRPNodeType.FRPBranchNode:
                {
                    
                }
                    break;
            }
            
           FlowUtility.SaveAsset(this);
        }

        public void OnBeforeSerialize()
        {
          //  throw new NotImplementedException();
        }

        public void OnAfterDeserialize()
        {
          //  throw new NotImplementedException();
        }
        
        
    }
}
