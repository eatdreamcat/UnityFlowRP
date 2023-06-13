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
        
        public FRPNodeBase CreateNode(FlowRenderGraphData.FRPNodeType nodeType, Vector2 position, string guid = "", string name = "", bool shouldDraw = true)
        {
            string nodeTypeName = "";
            switch (nodeType)
            {
                case FlowRenderGraphData.FRPNodeType.Entry:
                    nodeTypeName = "FRPNodeBase";
                    break;
                default:
                    nodeTypeName = nodeType.ToString();
                    break;
            }
            Type nodeClass = Type.GetType($"UnityEditor.Rendering.FlowPipeline.{nodeTypeName}");
            FRPNodeBase node = (FRPNodeBase)Activator.CreateInstance(nodeClass);
            
            node.Initialize(name == "" ? nodeType.ToString() : name, this, position, nodeType, guid);

            if (shouldDraw)
            {
                node.Draw();
            }

            if (guid == "")
            {
                // if no guid, means that node is created from CreateNode action , not from data loading
                // so we need to call this function to save the new node.
                OnElementCreated(node, position);
            }
            return node;
        }
        
        public FRPNodeGroup CreateGroup(string title, Vector2 position)
        {
            FRPNodeGroup group = new FRPNodeGroup(title, position);
            
            foreach (var selectedElement in selection)
            {
                if (!(selectedElement is FRPNodeBase))
                {
                    continue;
                }
                
                group.AddElement(selectedElement as FRPNodeBase);
            }
            OnElementCreated(group, position);
            return group;
        }
        
    }
}