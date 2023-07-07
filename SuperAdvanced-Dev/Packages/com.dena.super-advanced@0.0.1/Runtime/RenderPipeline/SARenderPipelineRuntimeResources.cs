

using System;

namespace UnityEngine.Rendering.SuperAdvanced
{
    public class FlowRenderPipelineRuntimeResources : FlowRenderPipelineResources
    {
        
        // What is ReloadGroup
        [Serializable, ReloadGroup]
        public sealed class ShaderResources
        {
            // Defaults
            [Reload("Runtime/Shaders/Lit/Lit.shader")]
            public Shader defaultPS;
        }

    }

}