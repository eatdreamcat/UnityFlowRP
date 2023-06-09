using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering.FlowPipeline;

namespace UnityEditor.Rendering.FlowPipeline
{

    public class FRPGraphSearchWindow : ScriptableObject, ISearchWindowProvider
    {
       
        private FRPGraphView m_View;
        public void Initialize(FRPGraphView view)
        {
            m_View = view;
         
        }
        
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
          
            List<SearchTreeEntry> searchTreeEntries = new List<SearchTreeEntry>()
            {
                new SearchTreeGroupEntry(new GUIContent("Create Element")),
                
                /// level 1 menu -- node
                new SearchTreeGroupEntry(new GUIContent("Graph Node",(EditorGUIUtility.Load(FRPPathUtility.kIconPath + "Resource_Icon.png") as Texture2D)), 1),
                
                /// level 2
                new SearchTreeGroupEntry(new GUIContent("Resource Node", (EditorGUIUtility.Load(FRPPathUtility.kIconPath + "Resource_Icon.png") as Texture2D)), 2),
                new SearchTreeEntry(new GUIContent("Resource",  (EditorGUIUtility.Load(FRPPathUtility.kIconPath + "Resource_Icon.png") as Texture2D)))
                {
                    level = 3,
                    userData = FlowRenderGraphData.FRPNodeType.FRPResourceNode
                },
                
                new SearchTreeEntry(new GUIContent("RenderTarget",   (EditorGUIUtility.Load(FRPPathUtility.kIconPath + "Resource_Icon.png") as Texture2D)))
                {
                    level = 3,
                    userData = FlowRenderGraphData.FRPNodeType.FRPRenderTargetNode
                },
                
                new SearchTreeEntry(new GUIContent("RenderTexture",   (EditorGUIUtility.Load(FRPPathUtility.kIconPath + "Resource_Icon.png") as Texture2D)))
                {
                    level = 3,
                    userData = FlowRenderGraphData.FRPNodeType.FRPRenderTextureNode
                },
                
                /// level 2
                new SearchTreeGroupEntry(new GUIContent("Pass Node",(EditorGUIUtility.Load(FRPPathUtility.kIconPath + "Resource_Icon.png") as Texture2D)), 2),
                new SearchTreeEntry(new GUIContent("RenderPass",   (EditorGUIUtility.Load(FRPPathUtility.kIconPath + "Resource_Icon.png") as Texture2D)))
                {
                    level = 3,
                    userData = FlowRenderGraphData.FRPNodeType.FRPRenderRequestNode
                },
                
                /// level 2
                new SearchTreeGroupEntry(new GUIContent("Flow Control",(EditorGUIUtility.Load(FRPPathUtility.kIconPath + "Resource_Icon.png") as Texture2D)), 2),
                new SearchTreeEntry(new GUIContent("Branch",   (EditorGUIUtility.Load(FRPPathUtility.kIconPath + "Resource_Icon.png") as Texture2D)))
                {
                    level = 3,
                    userData = FlowRenderGraphData.FRPNodeType.FRPBranchNode
                },
                new SearchTreeEntry(new GUIContent("Loop",   (EditorGUIUtility.Load(FRPPathUtility.kIconPath + "Resource_Icon.png") as Texture2D)))
                {
                    level = 3,
                    userData = FlowRenderGraphData.FRPNodeType.FPRLoopNode
                },
                
                
                // end of node
                
                // level 1 menu -- group
                new SearchTreeGroupEntry(new GUIContent("Group",(EditorGUIUtility.Load(FRPPathUtility.kIconPath + "Resource_Icon.png") as Texture2D)), 1),
                new SearchTreeEntry(new GUIContent("Single Group",(EditorGUIUtility.Load(FRPPathUtility.kIconPath + "Resource_Icon.png") as Texture2D)))
                {
                    level = 2,
                    userData = new Group()
                }
                
                // end of group
            };
            return searchTreeEntries;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            Vector2 localMousePosition = m_View.GetLocalMousePosition(context.screenMousePosition, true);
            
            switch (SearchTreeEntry.userData)
            {
               // resources
               case FlowRenderGraphData.FRPNodeType.FRPResourceNode:
               case FlowRenderGraphData.FRPNodeType.FRPRenderTargetNode:
               case FlowRenderGraphData.FRPNodeType.FRPRenderTextureNode:
               // render pass

               case FlowRenderGraphData.FRPNodeType.FRPRenderRequestNode:
               // flow control

               case FlowRenderGraphData.FRPNodeType.FRPBranchNode:
               case FlowRenderGraphData.FRPNodeType.FPRLoopNode:
               {
                   m_View.AddElement(m_View.CreateNode((FlowRenderGraphData.FRPNodeType)SearchTreeEntry.userData, localMousePosition));
               }
                   return true;
               
               // group

               case Group _:
               {
                   m_View.AddElement(m_View.CreateGroup("New Group", localMousePosition));
               }
                   return true;
            }

            return false;
        }
    }
}