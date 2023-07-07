using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace UnityEditor.Rendering.SuperAdvanced
{
    
    public class SAGroup : Group
    {
        
        public string ID { get; set; }

        public SAGroup(string groupTitle, Vector2 position, string guid = "")
        {
            ID = guid != "" ? guid : Guid.NewGuid().ToString();
            
            title = groupTitle;
            
            SetPosition(new Rect(position, Vector2.zero));
        }
    }
}