namespace UnityEngine.Rendering.FlowPipeline
{
    /// <summary>Helper to handle Deferred or Forward but not both</summary>
    public enum LitShaderMode
    {
        /// <summary>Lit shader uses forward rendering.</summary>
        Forward,
        /// <summary>Lit shader uses deferred rendering.</summary>
        Deferred
    }
    
}