namespace UnityEditor.Rendering.FlowPipeline
{
    public class FRPRenderRequestNode : FRPNodeBase
    {
        public override void Draw(bool refresh = true)
        {
            base.Draw(false);
            
            
            RefreshExpandedState();
        }
    }
}