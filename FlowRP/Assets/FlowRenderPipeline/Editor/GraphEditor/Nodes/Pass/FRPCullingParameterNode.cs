using UnityEngine;
using UnityEngine.Rendering.FlowPipeline;

namespace UnityEditor.Rendering.FlowPipeline
{
    public class FRPCullingParameterNode : FRPNodeBase
    {
        public FlowRenderGraphData.CullingParameterNode CullingParameter => (FlowRenderGraphData.CullingParameterNode)m_GraphNodeData;
        public override void Initialize(FRPGraphView view, Vector2 position, FlowRenderGraphData.BaseNode nodeData)
        {
            base.Initialize(view, position, nodeData);
            
            mainContainer.Add(FRPElementUtilities.CreateCullingParameter(
                CullingParameter.isAllowPassCulling, evt =>
                {
                    CullingParameter.isAllowPassCulling = evt.newValue;
                },
                CullingParameter.isAllowRendererCulling, evt =>
                {
                    CullingParameter.isAllowRendererCulling = evt.newValue;
                },
                CullingParameter.cullingMask, newLayer =>
                {
                    CullingParameter.cullingMask.value = newLayer;
                }
            ));
        }
    }

}