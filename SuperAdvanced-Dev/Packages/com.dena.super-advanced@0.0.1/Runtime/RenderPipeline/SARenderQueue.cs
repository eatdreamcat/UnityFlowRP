namespace UnityEngine.Rendering.SuperAdvanced
{
    internal static class FlowRenderQueue
    {
        public enum Priority
        {
            
            Background = RenderQueue.Background,
            
            // Warning: we must not change Geometry last value to stay compatible with occlusion
            OpaqueLast = RenderQueue.GeometryLast,
        }

        public static readonly RenderQueueRange k_RenderQueue_AllOpaque = new RenderQueueRange { lowerBound = (int)Priority.Background, upperBound = (int)Priority.OpaqueLast };
        
    }

}