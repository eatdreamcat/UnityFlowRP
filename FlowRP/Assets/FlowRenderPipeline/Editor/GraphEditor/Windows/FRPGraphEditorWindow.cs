

using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.FlowPipeline;
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

        private FlowRenderGraphData m_CurrentSelectedGraphData;

        private int m_CurrentIndex;

        private FlowRenderPipelineAsset m_PipelineAsset;

        private ToolbarMenu m_GraphDataListMenu;
        
        private Button m_SaveButton;
        
        #endregion
        

        #region Lifetime Method

        private void OnEnable()
        {
            if (!Initialize())
            {
                Debug.LogError("YOU ARE NOT USING MY POWERFUL PIPELINE ! DAMN!!!");
                return;
            }
            
            AddGraphView();
            AddToolBar();
            AddStyles();
          
            
            m_GraphView.Reload(m_CurrentSelectedGraphData);
        }

        private void OnDisable()
        {
            RemoveGraphView();
        }

        private void OnDestroy()
        {
            Debug.LogWarning("马老师顶住");
            RemoveGraphView();
        }

        #endregion


        #region Initialize
        
        private bool Initialize() {
            if (!GraphicsSettings.currentRenderPipeline || GraphicsSettings.currentRenderPipeline.GetType() != typeof(FlowRenderPipelineAsset))
                return false;
            m_PipelineAsset = (FlowRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;

            m_CurrentIndex = 0;
            m_CurrentSelectedGraphData = m_PipelineAsset.FlowRenderGraphDataList[0];
            
            return true;
        }
            
        private void AddToolBar()
        {
            
            Toolbar toolbar = new Toolbar();
            AddGraphDropdownList(toolbar);

            m_SaveButton = FRPElementUtilities.CreateButton("Save", () => Save());
            
            toolbar.Add(m_SaveButton);
            
            toolbar.AddStyleSheets("FRPToolbarStyles.uss");
            rootVisualElement.Add(toolbar);
        }

        private void Save()
        {
            m_GraphView.Save();
        }

        private void AddGraphDropdownList(Toolbar toolbar)
        {
            // Create a dropdown menu
            m_GraphDataListMenu = new ToolbarMenu();
            m_GraphDataListMenu.text = m_CurrentSelectedGraphData.name;
            toolbar.Add(m_GraphDataListMenu);

            for (int i = 0; i <  m_PipelineAsset.FlowRenderGraphDataList.Length; ++i)
            {
                var data = m_PipelineAsset.FlowRenderGraphDataList[i];
                m_GraphDataListMenu.menu.AppendAction(data.name, OnGraphDataSelected, DropdownMenuAction.AlwaysEnabled, i);
            }
        }
        
        private void AddGraphView()
        {
            
            RemoveGraphView();
            
            m_GraphView = new FRPGraphView(this);
            
            m_GraphView.StretchToParentSize();

            rootVisualElement.Add(m_GraphView);
        }

        private void RemoveGraphView()
        {
            if (m_GraphView != null)
            {
                rootVisualElement.Remove(m_GraphView);
                m_GraphView.Dispose();
                m_GraphView = null;
            }
        }
        
        private void AddStyles()
        {
            rootVisualElement.AddStyleSheets("FRPVariables.uss");
        }

        #endregion

        #region Hook

        private void OnGraphDataSelected(DropdownMenuAction action)
        {
            var newIndex = (int) action.userData;
            if (m_CurrentIndex == newIndex)
            {
                return;
            }
            
            m_CurrentIndex = newIndex;
            m_CurrentSelectedGraphData = m_PipelineAsset.FlowRenderGraphDataList[m_CurrentIndex];
            m_GraphView.Reload(m_CurrentSelectedGraphData);
            
            // update dropdown title
            m_GraphDataListMenu.text = m_CurrentSelectedGraphData.name;
        }

        #endregion
    }
}