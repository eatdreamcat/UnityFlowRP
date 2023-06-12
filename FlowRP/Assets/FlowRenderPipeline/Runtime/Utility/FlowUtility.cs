using UnityEditor;

namespace UnityEngine.Rendering.FlowPipeline
{
    public class FlowUtility 
    {
        
        internal const PerObjectData k_RendererConfigurationBakedLighting = PerObjectData.LightProbe | PerObjectData.Lightmaps | PerObjectData.LightProbeProxyVolume;
        internal const PerObjectData k_RendererConfigurationBakedLightingWithShadowMask = k_RendererConfigurationBakedLighting | PerObjectData.OcclusionProbe | PerObjectData.OcclusionProbeProxyVolume | PerObjectData.ShadowMask;
        
        static internal FlowRPAdditionalCameraData s_DefaultFlowRPAdditionalCameraData { get { return ComponentSingleton<FlowRPAdditionalCameraData>.instance; } }
   
        // We need these at runtime for RenderPipelineResources upgrade
        internal static string GetFlowRenderPipelinePath()
            => "Packages/com.chichi.render-pipelines.flow/";
        
        
        
        // $"FlowRenderPipeline::Render {cameraName}"
        internal static unsafe string ComputeCameraName(string cameraName)
        {
            // Interpolate the camera name with as few allocation as possible
            const string pattern1 = "FlowRenderPipeline::Render ";
            const int maxCharCountPerName = 40;

            var cameraNameSize = Mathf.Min(cameraName.Length, maxCharCountPerName);
            int size = pattern1.Length + cameraNameSize;

            var buffer = stackalloc char[size];
            var p = buffer;
            int i, c, s = 0;
            for (i = 0; i < pattern1.Length; ++i, ++p)
                *p = pattern1[i];
            for (i = 0, c = cameraNameSize; i < c; ++i, ++p)
                *p = cameraName[i];
            s += c;

            s += pattern1.Length;
            return new string(buffer, 0, s);
        }
        
        internal static FlowRPAdditionalCameraData TryGetAdditionalCameraDataOrDefault(Camera camera)
        {
            if (camera == null || camera.Equals(null))
                return s_DefaultFlowRPAdditionalCameraData;

            if (camera.TryGetComponent<FlowRPAdditionalCameraData>(out var additionalCameraData))
                return additionalCameraData;

            return s_DefaultFlowRPAdditionalCameraData;
        }

        public static void SaveAsset(ScriptableObject so)
        {
            EditorUtility.SetDirty(so);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }


}