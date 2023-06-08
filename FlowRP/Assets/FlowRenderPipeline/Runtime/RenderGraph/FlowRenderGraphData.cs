

using System;
using UnityEngine.UIElements;

namespace UnityEngine.Rendering.FlowPipeline
{
    public class FlowRenderGraphData : ScriptableObject
    {
        protected FlowRenderGraphData Create()
        {
            if (!Application.isPlaying)
            {
                // todo : reload all
            }
            return new FlowRenderGraphData();
        }
        
        
        ///
        ///
        ///    GraphData 
        ///
        ///

        [Serializable]
        class BaseNode
        {
            /* using for tracking layout data*/
            public string guid;
        }
        [Serializable]
        class PassNode : BaseNode
        {
            
        }

        [Serializable]
        class ResourceNode : BaseNode
        {
            
        }
        
        [Serializable]
        class BranchNode : BaseNode
        {
            
        }
    }
}
