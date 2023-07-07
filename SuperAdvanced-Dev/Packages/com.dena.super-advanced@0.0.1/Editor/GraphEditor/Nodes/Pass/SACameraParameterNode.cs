using UnityEngine;
using UnityEngine.Rendering.SuperAdvanced;

namespace UnityEditor.Rendering.SuperAdvanced
{
    
    public class SACameraParameterNode : SANodeBase
    {
        public SARenderGraphData.CameraParameterNode CameraParameter => (SARenderGraphData.CameraParameterNode)m_GraphNodeData;

        public override void Initialize(SAGraphView view, Vector2 position, SARenderGraphData.BaseNode nodeData)
        {
            base.Initialize(view, position, nodeData);

            mainContainer.Add(SAElementUtilities.CreateCameraParameter(
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