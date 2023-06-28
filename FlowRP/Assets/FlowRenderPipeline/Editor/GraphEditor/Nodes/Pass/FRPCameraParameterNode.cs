using UnityEngine;
using UnityEngine.Rendering.FlowPipeline;

namespace UnityEditor.Rendering.FlowPipeline
{
    
    public class FRPCameraParameterNode : FRPNodeBase
    {
        public FlowRenderGraphData.CameraParameterNode CameraParameter => (FlowRenderGraphData.CameraParameterNode)m_GraphNodeData;

        public override void Initialize(FRPGraphView view, Vector2 position, FlowRenderGraphData.BaseNode nodeData)
        {
            base.Initialize(view, position, nodeData);

            mainContainer.Add(FRPElementUtilities.CreateCameraParameter(
                CameraParameter.fov, evt =>
                {
                    CameraParameter.fov = evt.newValue;
                },
                CameraParameter.offset, evt =>
                {
                    CameraParameter.offset = evt.newValue;
                }));
        }
    }
}