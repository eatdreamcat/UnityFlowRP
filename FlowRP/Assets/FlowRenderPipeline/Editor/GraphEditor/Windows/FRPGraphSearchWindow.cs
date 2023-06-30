using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering.FlowPipeline;

namespace UnityEditor.Rendering.FlowPipeline
{

    public class FRPGraphSearchWindow : ScriptableObject, ISearchWindowProvider
    {
       
        private FRPGraphView m_View;
        private Texture2D m_EmptyTexture;
        public void Initialize(FRPGraphView view)
        {
            m_View = view;
            m_EmptyTexture = new Texture2D(1, 1);
            m_EmptyTexture.SetPixel(0,0, Color.clear);
            m_EmptyTexture.Apply();
        }
        
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
          
            List<SearchTreeEntry> searchTreeEntries = new List<SearchTreeEntry>()
            {
                new SearchTreeGroupEntry(new GUIContent("Create Element")),
                
                /// level 1 menu -- node
                new SearchTreeGroupEntry(new GUIContent("Node"), 1),
                
                /// level 2
                new SearchTreeGroupEntry(new GUIContent("Buffer Node"), 2),

                /// level 2
                new SearchTreeGroupEntry(new GUIContent("Pass Node"), 2),
                new SearchTreeEntry(new GUIContent("DrawRenderer", m_EmptyTexture))
                {
                    level = 3,
                    userData = FlowRenderGraphData.NodeType.DrawRendererNode
                },
                new SearchTreeEntry(new GUIContent("DrawFullScreen", m_EmptyTexture))
                {
                    level = 3,
                    userData = FlowRenderGraphData.NodeType.DrawFullScreenNode
                },
                new SearchTreeEntry(new GUIContent("Culling Options", m_EmptyTexture))
                {
                    level = 3,
                    userData = FlowRenderGraphData.NodeType.CullingParameterNode
                },
                
                new SearchTreeEntry(new GUIContent("Material", m_EmptyTexture))
                {
                    level = 3,
                    userData = FlowRenderGraphData.NodeType.RenderMaterialNode
                },
                
                new SearchTreeEntry(new GUIContent("Camera Options", m_EmptyTexture))
                {
                    level = 3,
                    userData = FlowRenderGraphData.NodeType.CameraParameterNode
                },
                
                new SearchTreeEntry(new GUIContent("Render State", m_EmptyTexture))
                {
                    level = 3,
                    userData = FlowRenderGraphData.NodeType.RenderStateNode
                },
                
                /// level 2
                new SearchTreeGroupEntry(new GUIContent("Logic Flow Control"), 2),
                new SearchTreeEntry(new GUIContent("Branch", m_EmptyTexture))
                {
                    level = 3,
                    userData = FlowRenderGraphData.NodeType.BranchNode
                },
                new SearchTreeEntry(new GUIContent("Loop", m_EmptyTexture))
                {
                    level = 3,
                    userData = FlowRenderGraphData.NodeType.LoopNode
                },
                
                
                /// level 2
                new SearchTreeGroupEntry(new GUIContent("Variables"), 2),
                
                
                // end of node
                
                // level 1 menu -- group
                new SearchTreeGroupEntry(new GUIContent("Group"), 1),
                new SearchTreeEntry(new GUIContent("Single Group", m_EmptyTexture))
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
               // buffer
             
               
               // render pass
               case FlowRenderGraphData.NodeType.DrawFullScreenNode:
               case FlowRenderGraphData.NodeType.DrawRendererNode:
               case FlowRenderGraphData.NodeType.CameraParameterNode:
               case FlowRenderGraphData.NodeType.CullingParameterNode:
               case FlowRenderGraphData.NodeType.RenderMaterialNode:
               case FlowRenderGraphData.NodeType.RenderStateNode:

               // logic flow control
               case FlowRenderGraphData.NodeType.BranchNode:
               case FlowRenderGraphData.NodeType.LoopNode:
               {
                   m_View.AddElement(m_View.CreateNode(
                           localMousePosition, 
                           m_View.AddNewNodeToData(
                               Guid.NewGuid().ToString(), 
                               SearchTreeEntry.userData.ToString(), 
                               (FlowRenderGraphData.NodeType)SearchTreeEntry.userData, 
                               localMousePosition
                               ))
                       );
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