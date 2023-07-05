using UnityEngine;
using UnityEngine.Rendering.FlowPipeline;

namespace UnityEditor.Rendering.FlowPipeline
{
    public class BufferAsPassInput { }
    public class BufferAsPassOutput { }
    
    public partial class FRPBufferNode : FRPNodeBase
    {
        public FlowRenderGraphData.BufferNode BufferNode => (FlowRenderGraphData.BufferNode)m_GraphNodeData;
        
        public override void Initialize(FRPGraphView view, Vector2 position, FlowRenderGraphData.BaseNode nodeData)
        {
            base.Initialize(view, position, nodeData);

            var lifeTime = FRPElementUtilities.CreateEnumField(BufferNode.lifeTime, "LifeTime", evt =>
            {
                // TODO: BufferData need to migrate to pipeline assets when lifetime is  PerFrame type ?  
                BufferNode.lifeTime = (FlowRenderGraphData.BufferLifeTime)evt.newValue;
            });
            lifeTime.style.top = 3;
            lifeTime.style.height = 20;
            
            mainContainer.Add(lifeTime);
            
            if (BufferNode.bufferType == FlowRenderGraphData.BufferType.ComputerBuffer)
            {
                DrawComputeBuffer();
                
            } else if (BufferNode.bufferType == FlowRenderGraphData.BufferType.TextureBuffer)
            {
                DrawTextureBuffer();
            }
        }
    }
}