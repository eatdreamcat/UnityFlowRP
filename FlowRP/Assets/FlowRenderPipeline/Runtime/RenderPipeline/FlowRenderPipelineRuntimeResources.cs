

using System;

namespace UnityEngine.Rendering.FlowPipeline
{
    public class FlowRenderPipelineRuntimeResources : FlowRenderPipelineResources
    {
        
        // What is ReloadGroup
        [Serializable, ReloadGroup]
        public sealed class ShaderResources
        {
            // Defaults
            [Reload("Runtime/Material/Lit/Lit.shader")]
            public Shader defaultPS;
        }

    }

}