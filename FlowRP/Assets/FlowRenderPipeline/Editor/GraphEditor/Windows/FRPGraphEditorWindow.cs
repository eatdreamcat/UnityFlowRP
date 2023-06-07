

using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.Rendering.FlowPipeline
{
    public class FRPGraphEditorWindow : EditorWindow
    {
        [MenuItem("FRP/ Render Graph")]
        public static void Open()
        {
            GetWindow<FRPGraphEditorWindow>("Render Graph");
        }

        #region Properties

        private FRPGraphView m_GraphView;
        

        #endregion
        

        #region Lifetime Method

        private void OnEnable()
        {
            AddGraphView();
          
        }

        private void OnDisable()
        {
            Debug.LogWarning("马老师顶住");
        }

        private void OnDestroy()
        {
            Debug.LogWarning("马老师顶住");
        }

        #endregion


        #region Initialize

        private void AddGraphView()
        {
            m_GraphView = new FRPGraphView(this);
            
            m_GraphView.StretchToParentSize();

            rootVisualElement.Add(m_GraphView);
        }
        
     

        #endregion
    }
}