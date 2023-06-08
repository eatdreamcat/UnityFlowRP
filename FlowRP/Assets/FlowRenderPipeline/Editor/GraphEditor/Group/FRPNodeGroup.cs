using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace UnityEditor.Rendering.FlowPipeline
{
    
    public class FRPNodeGroup : Group
    {
        
        public string ID { get; set; }

        public FRPNodeGroup(string groupTitle, Vector2 position)
        {
            ID = Guid.NewGuid().ToString();
            
            title = groupTitle;
            
            SetPosition(new Rect(position, Vector2.zero));
        }
    }
}