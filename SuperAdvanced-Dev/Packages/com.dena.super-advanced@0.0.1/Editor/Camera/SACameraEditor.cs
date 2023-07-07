using UnityEngine;
using UnityEngine.Rendering.SuperAdvanced;

namespace UnityEditor.Rendering.SuperAdvanced
{
  
    [CustomEditorForRenderPipeline(typeof(Camera), typeof(SARenderPipelineAsset))]
    [CanEditMultipleObjects]
    class SACameraEditor : CameraEditor
    {
        
        SASerializedCamera m_SerializedCamera;
        
        public new void OnEnable()
        {
            base.OnEnable();
            m_SerializedCamera = new SASerializedCamera(serializedObject, settings);
            
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
            base.OnInspectorGUI();
            
            var rpAsset = SARenderPipeline.asset;
            if (rpAsset == null)
            {
               
                return;
            }

            m_SerializedCamera.Update();
        }
    }

}