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
        private static readonly string NodeClassNamePrefix = "FRP";
        private static readonly string BaseNodeClassName = "NodeBase";
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

                if (!FRPElementUtilities.CanAcceptConnector(startPort, port))
                {
                    return;
                }
            
                compatiblePorts.Add(port);
                
            });
            return compatiblePorts;
        }
        
        public FRPNodeBase CreateNode(Vector2 position, FlowRenderGraphData.BaseNode baseNode, bool shouldDraw = true, bool isCreatedByAction = false)
        {
            string nodeTypeName = "";
            switch (baseNode.type)
            {
                case FlowRenderGraphData.NodeType.EntryNode:
                    nodeTypeName = BaseNodeClassName;
                    break;
                default:
                    nodeTypeName = baseNode.type.ToString();
                    break;
            }
            
            Type nodeClass = Type.GetType($"UnityEditor.Rendering.FlowPipeline.{NodeClassNamePrefix + nodeTypeName}");
            
            Debug.Assert(nodeClass != null, $"Node class type is null : {NodeClassNamePrefix + nodeTypeName}");
            
            FRPNodeBase node = (FRPNodeBase)Activator.CreateInstance(nodeClass);
            
            node.Initialize(this, position, baseNode);

            if (shouldDraw)
            {
                node.Draw();
            }

            if (isCreatedByAction)
            {
                // if isCreatedByAction, means that node is created from CreateNode action , not from data loading
                // so we need to call this function to save the new node.
                OnElementCreated(node, position);
            }
            
            return node;
        }
        
        public FRPGroup CreateGroup(string title, Vector2 position, string guid = "")
        {
            FRPGroup group = new FRPGroup(title, position, guid);
            
            if (guid == "")
            {
                // if no guid, means that group is created from CreateGroup action , not from data loading
                // so we need to call this function to save the new group.
                OnElementCreated(group, position);
                
                foreach (var selectedElement in selection)
                {
                    if (!(selectedElement is FRPNodeBase))
                    {
                        continue;
                    }

                    OnElementsAddedToGroup(group, new [] { (GraphElement)selectedElement });
                    
                    group.AddElement(selectedElement as FRPNodeBase);
                    
                }
            }
            
            return group;
        }
        
    }
}