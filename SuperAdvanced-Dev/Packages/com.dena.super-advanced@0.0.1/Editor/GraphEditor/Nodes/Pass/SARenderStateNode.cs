using UnityEngine;
using UnityEngine.Rendering.SuperAdvanced;

namespace UnityEditor.Rendering.SuperAdvanced
{
    public class SARenderStateNode : SANodeBase
    {
        public SARenderGraphData.RenderStateNode RenderStateParameter => (SARenderGraphData.RenderStateNode)m_GraphNodeData;

        public override void Initialize(SAGraphView view, Vector2 position, SARenderGraphData.BaseNode nodeData)
        {
            base.Initialize(view, position, nodeData);
            
            mainContainer.Add(SAElementUtilities.CreateStateParameter(RenderStateParameter));
        }
    }
}