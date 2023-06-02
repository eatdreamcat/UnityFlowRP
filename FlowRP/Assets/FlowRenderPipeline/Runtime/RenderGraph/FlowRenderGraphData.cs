using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Rendering.FlowRP
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
    }
}
