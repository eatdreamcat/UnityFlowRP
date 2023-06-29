using UnityEngine;
using UnityEngine.Rendering.FlowPipeline;

namespace UnityEditor.Rendering.FlowPipeline
{
    public class FRPRenderStateNode : FRPNodeBase
    {
        public FlowRenderGraphData.RenderStateNode RenderStateParameter => (FlowRenderGraphData.RenderStateNode)m_GraphNodeData;

        public override void Initialize(FRPGraphView view, Vector2 position, FlowRenderGraphData.BaseNode nodeData)
        {
            base.Initialize(view, position, nodeData);
            
            mainContainer.Add(FRPElementUtilities.CreateStateParameter(RenderStateParameter));
        }
    }
}