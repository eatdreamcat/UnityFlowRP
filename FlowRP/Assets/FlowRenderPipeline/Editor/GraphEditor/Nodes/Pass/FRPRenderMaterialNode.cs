using UnityEngine;
using UnityEngine.Rendering.FlowPipeline;

namespace UnityEditor.Rendering.FlowPipeline
{
    public class FRPRenderMaterialNode : FRPNodeBase
    {
        public FlowRenderGraphData.MaterialParameterNode MaterialParameter => (FlowRenderGraphData.MaterialParameterNode)m_GraphNodeData;
        public override void Initialize(FRPGraphView view, Vector2 position, FlowRenderGraphData.BaseNode nodeData)
        {
            base.Initialize(view, position, nodeData);

            mainContainer.Add(FRPElementUtilities.CreateMaterialParameter(
                MaterialParameter.renderQueueRange.start, evt =>
                {
                    MaterialParameter.renderQueueRange.start = (FlowRenderGraphData.Queue)evt.newValue;
                },
                MaterialParameter.renderQueueRange.end, evt =>
                {
                    MaterialParameter.renderQueueRange.end = (FlowRenderGraphData.Queue)evt.newValue;
                },
                MaterialParameter.shaderTagList
                ));
        }
    }
}