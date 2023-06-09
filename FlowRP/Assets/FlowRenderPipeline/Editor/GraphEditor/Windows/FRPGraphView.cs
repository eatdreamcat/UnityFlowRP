
using System;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering.FlowPipeline;
using UnityEngine.UIElements;

namespace UnityEditor.Rendering.FlowPipeline
{
    public partial class FRPGraphView : GraphView, IDisposable
    {
        private FRPGraphEditorWindow m_EditorWindow;

        private FRPGraphSearchWindow m_SearchWindow;

        public FRPGraphView(FRPGraphEditorWindow editorWindow)
        {
            m_EditorWindow = editorWindow;
            
            
            
            AddManipulators();
            AddSearchWindow();
            AddGridBackground();
            AddStyles();

            AddHook();
        }
        
        private void AddStyles()
        {
            this.AddStyleSheets(
                "FRPGraphViewStyles.uss",
                "FRPNodeStyles.uss");
        }
        private void AddGridBackground()
        {
            GridBackground gridBackground = new GridBackground();

            gridBackground.StretchToParentSize();

            Insert(0, gridBackground);
        }

        private void AddSearchWindow()
        {
            if (m_SearchWindow == null)
            {
                m_SearchWindow = ScriptableObject.CreateInstance<FRPGraphSearchWindow>();
                m_SearchWindow.Initialize(this);
            }
            
            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), m_SearchWindow);
        }
        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            
            // this.AddManipulator(CreateNodeContextualMenu("Add Node", FRPNodeType.FRPNodeBase));
            
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            
            this.AddManipulator(CreateGroupContextualMenu());
        }
        
        private IManipulator CreateNodeContextualMenu(string actionTitle, FlowRenderGraphData.FRPNodeType nodeType)
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction(actionTitle, actionEvent => AddElement(CreateNode(nodeType, GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))))
            );

            return contextualMenuManipulator;
        }

        private IManipulator CreateGroupContextualMenu()
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction("Add Group", actionEvent => AddElement(CreateGroup("NewGroup", GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))))
            );

            return contextualMenuManipulator;
        }

        public Vector2 GetLocalMousePosition(Vector2 mousePosition, bool isSearchWindow = false)
        {
            Vector2 worldMousePosition = mousePosition;

            if (isSearchWindow)
            {
                worldMousePosition = m_EditorWindow.rootVisualElement.ChangeCoordinatesTo(m_EditorWindow.rootVisualElement.parent, mousePosition - m_EditorWindow.position.position);
            }

            Vector2 localMousePosition = contentViewContainer.WorldToLocal(worldMousePosition);

            return localMousePosition;
        }
        
        
        private void AddEntryPoint(Vector2 position)
        {
            AddElement(CreateNode(FlowRenderGraphData.FRPNodeType.FRPNodeBase, position, true, true));
        }
        
        public void Dispose()
        {
            RemoveHook();
        }
    }
}