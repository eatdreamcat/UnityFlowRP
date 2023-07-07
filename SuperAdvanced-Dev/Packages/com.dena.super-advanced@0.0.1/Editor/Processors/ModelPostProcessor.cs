
using UnityEngine;
using UnityEngine.Rendering.SuperAdvanced;

namespace UnityEditor.Rendering.SuperAdvanced
{
    public class ModelPostProcessor : AssetPostprocessor
    {
        void OnPostprocessModel(GameObject go)
        {
           
            CoreEditorUtils.AddAdditionalData<Camera, SAAdditionalCameraData>(go, SAAdditionalCameraData.InitDefaultFlowRPAdditionalCameraData);
            // CoreEditorUtils.AddAdditionalData<Light, HDAdditionalLightData>(go, HDAdditionalLightData.InitDefaultHDAdditionalLightData);
            // CoreEditorUtils.AddAdditionalData<ReflectionProbe, HDAdditionalReflectionData>(go);
        }
    }
}
