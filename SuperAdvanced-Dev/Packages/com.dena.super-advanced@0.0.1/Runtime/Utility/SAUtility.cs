using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace UnityEngine.Rendering.SuperAdvanced
{
    public class SAUtility 
    {


        #region Culling

        internal static  readonly PerObjectData k_DefaultPerObjectData = 
            PerObjectData.LightData | 
            PerObjectData.Lightmaps | 
            PerObjectData.LightIndices | 
            PerObjectData.LightProbe | 
            PerObjectData.MotionVectors | 
            PerObjectData.OcclusionProbe | 
            PerObjectData.ReflectionProbes |
            PerObjectData.ShadowMask |
            PerObjectData.ReflectionProbeData | 
            PerObjectData.LightProbeProxyVolume |
            PerObjectData.OcclusionProbeProxyVolume;

        internal static readonly LayerMask k_DefaultLayerMask = -1;

        internal static readonly SARenderGraphData.CullingParameterNode k_DefaultCullingNode =
            new SARenderGraphData.CullingParameterNode()
            {
                isAllowPassCulling = true,
                isAllowRendererCulling = true,
                cullingMask = k_DefaultLayerMask,
                perObjectData = k_DefaultPerObjectData
            };

        #endregion


        #region State

        internal static readonly SARenderGraphData.RenderStateNode k_DefaultRenderStateNode =
            new SARenderGraphData.RenderStateNode()
            {
                blendStateSettings = new SARenderGraphData.BlendStateSettings()
                {
                    blendStates = new List<SARenderGraphData.BlendStateData>()
                }
            };
        
        internal static readonly RenderTargetBlendState k_DefaultRenderTargetBlendState = RenderTargetBlendState.defaultValue;

        #endregion

        #region Material

        internal static readonly SARenderGraphData.MaterialParameterNode k_DefaultMaterialNode =
            new SARenderGraphData.MaterialParameterNode()
            {
                renderQueueRange = new SARenderGraphData.QueueRange()
                {
                    start = SARenderGraphData.Queue.Start,
                    end = SARenderGraphData.Queue.End
                },
                shaderTagList = new List<string>()
                {
                    "SRPUnlitDefault"
                },
                overrideMaterial = null
            };

        #endregion

        #region Camera

        internal static readonly SARenderGraphData.CameraParameterNode k_DefaultCameraOverrideNode = 
            new SARenderGraphData.CameraParameterNode() 
            {
                fov = 0, 
                offset = Vector3.zero
                
            };
        
        #endregion
        
        static internal SAAdditionalCameraData SDefaultSaAdditionalCameraData { get { return ComponentSingleton<SAAdditionalCameraData>.instance; } }
   
        // We need these at runtime for RenderPipelineResources upgrade
        internal static string GetSARenderPipelinePath()
            => "Packages/com.dena.render-pipelines.super-advanced/";
        
        
        
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
        
        internal static SAAdditionalCameraData TryGetAdditionalCameraDataOrDefault(Camera camera)
        {
            if (camera == null || camera.Equals(null))
                return SDefaultSaAdditionalCameraData;

            if (camera.TryGetComponent<SAAdditionalCameraData>(out var additionalCameraData))
                return additionalCameraData;

            return SDefaultSaAdditionalCameraData;
        }

        public static void SaveAsset(ScriptableObject so)
        {
            EditorUtility.SetDirty(so);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }


}