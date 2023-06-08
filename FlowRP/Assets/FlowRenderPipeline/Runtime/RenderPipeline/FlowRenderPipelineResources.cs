

namespace UnityEngine.Rendering.FlowPipeline
{

    public enum ResourceLifeTime
    {
        
        Persistent,
        PerFrame,
        // per-camera
        PerGraph,
        // know as temporary, logic pass
        PerPass
    }
    
    public class FlowRenderPipelineResources : RenderPipelineResources
    {
        protected override string packagePath => FlowUtility.GetFlowRenderPipelinePath();
        
    }
}