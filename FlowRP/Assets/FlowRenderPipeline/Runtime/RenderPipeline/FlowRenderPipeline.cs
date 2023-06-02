
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.FlowRP
{
    public partial class FlowRenderPipeline : RenderPipeline
    {
        
        // RenderPipeline Config Asset
        public static FlowRenderPipelineAsset asset
        {
            get => GraphicsSettings.currentRenderPipeline as FlowRenderPipelineAsset;
        }


        private RenderGraph m_RenderGraph = new RenderGraph("FlowRenderGraph");


        public FlowRenderPipeline(FlowRenderPipelineAsset asset)
        {

        }

        
        
        
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            
        }
    }
}
