
using System;
using System.IO;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;

namespace UnityEngine.Rendering.FlowPipeline
{
    [ExcludeFromPreset]
    public partial class FlowRenderPipelineAsset : RenderPipelineAsset, ISerializationCallbackReceiver
    {
        #region Static Properties

        public static readonly string kIconPath = "Assets/FlowRenderPipeline/Editor/Resources/Icon/";

        #endregion


        #region RendererSettings

        [SerializeField] internal FlowRenderGraphData[] m_RenderGraphDataList = new FlowRenderGraphData[1];
        [SerializeField] internal int m_DefaultRendererIndex = 0;
        
        // Default values set when a new UniversalRenderPipeline asset is created
       int k_AssetVersion = 9;
       int k_AssetPreviousVersion = 9;

        #endregion



        #region RenderPileine Asset and RendererData Create 

                
        
        public static FlowRenderPipelineAsset Create(FlowRenderGraphData rendererData = null)
        {
            // Create Universal RP Asset
            var instance = CreateInstance<FlowRenderPipelineAsset>();
            if (rendererData != null)
                instance.m_RenderGraphDataList[0] = rendererData;
            else
                instance.m_RenderGraphDataList[0] = CreateInstance<FlowRenderGraphData>();
            
            return instance;
        }
        
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812")]
        internal class CreateFlowRenderPipelineAsset : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                //Create asset
                AssetDatabase.CreateAsset(Create(CreateRendererAsset(pathName)), pathName);
            }
        }
        
        [MenuItem("Assets/Create/Rendering/Flow Render Pipeline Asset", priority = CoreUtils.Sections.section2 + CoreUtils.Priorities.assetsCreateRenderingMenuPriority + 1)]
        static void CreateFlowRenderPipeline()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, CreateInstance<CreateFlowRenderPipelineAsset>(),
                "New Flow Render Pipeline Asset.asset",null, null);
        }

        internal static FlowRenderGraphData CreateRendererAsset(string path,
            bool relativePath = true, string suffix = "GraphData")
        {
            FlowRenderGraphData data = CreateRendererData();
            string dataPath;
            if (relativePath)
                dataPath =
                    $"{Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path))}_{suffix}{Path.GetExtension(path)}";
            else
                dataPath = path;
            
            AssetDatabase.CreateAsset(data, dataPath);
            FlowUtility.SaveAsset(data);
            ResourceReloader.ReloadAllNullIn(data, FlowUtility.GetFlowRenderPipelinePath());
            return data;
        }
        
        
        static FlowRenderGraphData CreateRendererData()
        {
            var rendererData = CreateInstance<FlowRenderGraphData>();
            return rendererData;
        }
        
        protected override RenderPipeline CreatePipeline()
        {
            if (m_RenderGraphDataList == null)
                m_RenderGraphDataList = new FlowRenderGraphData[1];

            // If no default data we can't create pipeline instance
            if (m_RenderGraphDataList[m_DefaultRendererIndex] == null)
            {
                // If previous version and current version are miss-matched then we are waiting for the upgrader to kick in
                if (k_AssetPreviousVersion != k_AssetVersion)
                    return null;

                Debug.LogError(
                    $"Default Renderer is missing, make sure there is a Renderer assigned as the default on the current Flow RP asset:{FlowRenderPipeline.asset.name}",
                    this);
                return null;
            }
            
            return new FlowRenderPipeline(this);
        }
        

        #endregion
        
        /**
         * TODO: .//// 
         */
        public void OnBeforeSerialize()
        {
           // throw new NotImplementedException();
        }

        public void OnAfterDeserialize()
        {
          //  throw new NotImplementedException();
        }
        
        
        internal bool ValidateRendererData(int index)
        {
            // Check to see if you are asking for the default renderer
            if (index == -1) index = m_DefaultRendererIndex;
            return index < m_RenderGraphDataList.Length ? m_RenderGraphDataList[index] != null : false;
        }
        
        internal FlowRenderGraphData FlowRenderGraphData
        {
            get
            {
                if (m_RenderGraphDataList[m_DefaultRendererIndex] == null)
                    CreatePipeline();

                return m_RenderGraphDataList[m_DefaultRendererIndex];
            }
        }

        public FlowRenderGraphData[] FlowRenderGraphDataList
        {
            get
            {
                return m_RenderGraphDataList;
            }
        }
        
    }
}
