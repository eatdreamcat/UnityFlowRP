using UnityEngine;
using UnityEngine.Rendering.FlowPipeline;

namespace UnityEditor.Rendering.FlowPipeline
{
  
    [CustomEditorForRenderPipeline(typeof(Camera), typeof(FlowRenderPipelineAsset))]
    [CanEditMultipleObjects]
    class FlowRenderPipelineCameraEditor : CameraEditor
    {
        
        FlowRenderPipelineSerializedCamera m_SerializedCamera;
        
        public new void OnEnable()
        {
            base.OnEnable();
            m_SerializedCamera = new FlowRenderPipelineSerializedCamera(serializedObject, settings);
            
        }

        void UpdateCameras()
        {
            m_SerializedCamera.Refresh();
        }
        
        public new void OnDisable()
        {
            base.OnDisable();
        }

        public override void OnInspectorGUI()
        {
            var rpAsset = FlowRenderPipeline.asset;
            if (rpAsset == null)
            {
                base.OnInspectorGUI();
                return;
            }

            m_SerializedCamera.Update();
        }
    }

}