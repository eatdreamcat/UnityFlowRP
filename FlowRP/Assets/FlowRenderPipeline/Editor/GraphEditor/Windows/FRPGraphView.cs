
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.Rendering.FlowPipeline
{
    public partial class FRPGraphView : GraphView
    {
        private FRPGraphEditorWindow m_EditorWindow;

        public FRPGraphView(FRPGraphEditorWindow editorWindow)
        {
            m_EditorWindow = editorWindow;
            
            
            
            AddManipulators();
           
            AddGridBackground();
            AddEntryPoint();
            AddStyles();
        }
        
        
        public void OnGraphDataUpdate() {
            
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

        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            
            this.AddManipulator(CreateNodeContextualMenu("Add Node", FRPNodeType.FRPNodeBase));
            
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            
            this.AddManipulator(CreateGroupContextualMenu());
        }
        
        private IManipulator CreateNodeContextualMenu(string actionTitle, FRPNodeType nodeType)
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction(actionTitle, actionEvent => AddElement(CreateNode("NodeName", nodeType, GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))))
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
        
        
        private void AddEntryPoint()
        {
            AddElement(CreateNode("Entry", FRPNodeType.FRPNodeBase, new Vector2(100, 200), true, true));
        }
    }
}