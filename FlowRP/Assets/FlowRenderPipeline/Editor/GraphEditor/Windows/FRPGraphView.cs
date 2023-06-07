using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace UnityEditor.Rendering.FlowPipeline
{
    public class FRPGraphView : GraphView
    {
        private FRPGraphEditorWindow m_EditorWindow;

        public FRPGraphView(FRPGraphEditorWindow editorWindow)
        {
            m_EditorWindow = editorWindow;
            
            AddGridBackground();
            AddStyles();
        }
        
        private void AddStyles()
        {
            this.AddStyleSheets("FRPVariableStyles.uss");
        }
        
        private void AddGridBackground()
        {
            GridBackground gridBackground = new GridBackground();

            gridBackground.StretchToParentSize();

            Insert(0, gridBackground);
        }
    }
}