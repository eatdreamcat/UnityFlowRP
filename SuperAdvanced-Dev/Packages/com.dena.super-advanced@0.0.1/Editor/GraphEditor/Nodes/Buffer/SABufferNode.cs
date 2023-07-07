using UnityEngine;
using UnityEngine.Rendering.SuperAdvanced;

namespace UnityEditor.Rendering.SuperAdvanced
{
    public class BufferAsPassInput { }
    public class BufferAsPassOutput { }
    
    public partial class SABufferNode : SANodeBase
    {
        public SARenderGraphData.BufferNode BufferNode => (SARenderGraphData.BufferNode)m_GraphNodeData;
        
        public override void Initialize(SAGraphView view, Vector2 position, SARenderGraphData.BaseNode nodeData)
        {
            base.Initialize(view, position, nodeData);

            var lifeTime = SAElementUtilities.CreateEnumField(BufferNode.lifeTime, "LifeTime", evt =>
            {
                // TODO: BufferData need to migrate to pipeline assets when lifetime is  PerFrame type ?  
                BufferNode.lifeTime = (SARenderGraphData.BufferLifeTime)evt.newValue;
            });
            lifeTime.style.top = 3;
            lifeTime.style.height = 20;
            
            mainContainer.Add(lifeTime);
            
            if (BufferNode.bufferType == SARenderGraphData.BufferType.ComputerBuffer)
            {
                DrawComputeBuffer();
                
            } else if (BufferNode.bufferType == SARenderGraphData.BufferType.TextureBuffer)
            {
                DrawTextureBuffer();
            }
        }
    }
}