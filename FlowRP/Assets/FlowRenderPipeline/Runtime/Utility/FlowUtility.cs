using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace UnityEngine.Rendering.FlowPipeline
{
    public class FlowUtility 
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

        internal static readonly FlowRenderGraphData.CullingParameterNode k_DefaultCullingNode =
            new FlowRenderGraphData.CullingParameterNode()
            {
                isAllowPassCulling = true,
                isAllowRendererCulling = true,
                cullingMask = k_DefaultLayerMask,
                perObjectData = k_DefaultPerObjectData
            };

        #endregion


        #region State

        internal static readonly FlowRenderGraphData.RenderStateNode k_DefaultRenderStateNode =
            new FlowRenderGraphData.RenderStateNode()
            {

            };

        #endregion

        #region Material

        internal static readonly FlowRenderGraphData.MaterialParameterNode k_DefaultMaterialNode =
            new FlowRenderGraphData.MaterialParameterNode()
            {
                renderQueueRange = new FlowRenderGraphData.QueueRange()
                {
                    start = FlowRenderGraphData.Queue.Start,
                    end = FlowRenderGraphData.Queue.End
                },
                shaderTagList = new List<string>()
                {
                    "SRPUnlitDefault"
                },
                overrideMaterial = null
            };

        #endregion

        #region Camera

        internal static readonly FlowRenderGraphData.CameraParameterNode k_DefaultCameraOverrideNode = 
            new FlowRenderGraphData.CameraParameterNode() 
            {
                fov = 0, 
                offset = Vector3.zero
                
            };
        
        #endregion
        
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