using UnityEngine;
using UnityEngine.Rendering;
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
                CullingParameter, evt =>
                {
                    CullingParameter.isAllowPassCulling = evt.newValue;
                }, 
                evt =>
                {
                    CullingParameter.isAllowRendererCulling = evt.newValue;
                },
              newLayer =>
                {
                    CullingParameter.cullingMask.value = newLayer;
                },
                evt =>
                {
                    CullingParameter.perObjectData = (PerObjectData) evt.newValue;
                }
            ));
        }
    }

}