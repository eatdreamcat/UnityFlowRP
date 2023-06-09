using UnityEngine;

namespace UnityEditor.Rendering.FlowPipeline
{
    /**
     *
     *   note: for GraphViewSavedData, we only focus on the view side informations.
     *         Relation informations between nodes and Node Data (like properties, type , connections ... ) we will save to FlowRenderGraphData as these data are necessary on runtime.
     * 
     *   FRPGraphViewSavedData
     *      RendererData - GraphView
     *        NodesMap - GUID   
     *          Position
     *          Group - GUID
     *          Size ?
     *        GroupsMap
     *          Position
     *          Name
     *          Size ? 
     *
     *   
     */
    public class FRPGraphViewSavedData : ScriptableObject
    {
        
    }
}