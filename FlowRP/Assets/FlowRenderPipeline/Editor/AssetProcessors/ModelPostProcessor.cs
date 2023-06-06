
using UnityEngine;
using UnityEngine.Rendering.FlowPipeline;

namespace UnityEditor.Rendering.FlowPipeline
{
    public class ModelPostProcessor : AssetPostprocessor
    {
        void OnPostprocessModel(GameObject go)
        {
            Debug.LogError("OnPostprocessModel:" + go.name);
            CoreEditorUtils.AddAdditionalData<Camera, FlowRPAdditionalCameraData>(go, FlowRPAdditionalCameraData.InitDefaultFlowRPAdditionalCameraData);
            // CoreEditorUtils.AddAdditionalData<Light, HDAdditionalLightData>(go, HDAdditionalLightData.InitDefaultHDAdditionalLightData);
            // CoreEditorUtils.AddAdditionalData<ReflectionProbe, HDAdditionalReflectionData>(go);
        }
    }
}
