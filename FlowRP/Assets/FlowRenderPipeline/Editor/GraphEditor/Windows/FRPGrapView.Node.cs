using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering.FlowPipeline;

namespace UnityEditor.Rendering.FlowPipeline
{
    public partial class FRPGraphView
    {
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            
            List<Port> compatiblePorts = new List<Port>();
            ports.ForEach(port =>
            {
                if (startPort == port)
                {
                    return;
                }
            
                if (startPort.node == port.node)
                {
                    return;
                }
            
                if (startPort.direction == port.direction)
                {
                    return;
                }
            
                compatiblePorts.Add(port);
            });
            return compatiblePorts;
        }
        
        public FRPNodeBase CreateNode(FlowRenderGraphData.FRPNodeType nodeType, Vector2 position, bool shouldDraw = true, bool isEntry = false)
        {
            Type nodeClass = Type.GetType($"UnityEditor.Rendering.FlowPipeline.{nodeType}");
            FRPNodeBase node = (FRPNodeBase)Activator.CreateInstance(nodeClass);
            
            node.Initialize(isEntry ? "Entry" : nodeType.ToString(), this, position, nodeType, isEntry);
            if (shouldDraw)
            {
                node.Draw();
            }
            OnElementCreated(node);
            return node;
        }
        
        public Group CreateGroup(string title, Vector2 position)
        {
            Group group = new Group()
            {
                title = title
            };

            group.SetPosition(new Rect(position, Vector2.zero));

            foreach (var selectedElement in selection)
            {
                if (!(selectedElement is FRPNodeBase))
                {
                    continue;
                }
                
                group.AddElement(selectedElement as FRPNodeBase);
            }
            OnElementCreated(group);
            return group;
        }
        
    }
}