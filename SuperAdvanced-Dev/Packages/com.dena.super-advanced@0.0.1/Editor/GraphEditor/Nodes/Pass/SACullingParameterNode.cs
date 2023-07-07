using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.SuperAdvanced;

namespace UnityEditor.Rendering.SuperAdvanced
{
    public class SACullingParameterNode : SANodeBase
    {
        public SARenderGraphData.CullingParameterNode CullingParameter => (SARenderGraphData.CullingParameterNode)m_GraphNodeData;
        public override void Initialize(SAGraphView view, Vector2 position, SARenderGraphData.BaseNode nodeData)
        {
            base.Initialize(view, position, nodeData);
            
            mainContainer.Add(SAElementUtilities.CreateCullingParameter(
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