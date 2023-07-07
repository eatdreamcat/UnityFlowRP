using UnityEngine;
using UnityEngine.Rendering.SuperAdvanced;


namespace UnityEditor.Rendering.SuperAdvanced
{
    public class SARenderMaterialNode : SANodeBase
    {
        public SARenderGraphData.MaterialParameterNode MaterialParameter => (SARenderGraphData.MaterialParameterNode)m_GraphNodeData;
        public override void Initialize(SAGraphView view, Vector2 position, SARenderGraphData.BaseNode nodeData)
        {
            base.Initialize(view, position, nodeData);

            mainContainer.Add(SAElementUtilities.CreateMaterialParameter(
                MaterialParameter, 
                evt =>
                {
                    MaterialParameter.renderQueueRange.start = (SARenderGraphData.Queue)evt.newValue;
                }, 
                evt =>
                {
                    MaterialParameter.renderQueueRange.end = (SARenderGraphData.Queue)evt.newValue;
                }
            ));
        }
    }
}