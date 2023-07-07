using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering.SuperAdvanced;

namespace UnityEditor.Rendering.SuperAdvanced
{
    public partial class SAGraphView
    {
        private static readonly string PackageNameSpace = "SuperAdvanced";
        private static readonly string NodeClassNamePrefix = "SA";
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

                if (!SAElementUtilities.CanAcceptConnector(startPort, port))
                {
                    return;
                }
            
                compatiblePorts.Add(port);
                
            });
            return compatiblePorts;
        }
        
        public SANodeBase CreateNode(Vector2 position, SARenderGraphData.BaseNode baseNode, bool shouldDraw = true, bool isCreatedByAction = false)
        {
            string nodeTypeName = "";
            switch (baseNode.type)
            {
                case SARenderGraphData.NodeType.EntryNode:
                    nodeTypeName = BaseNodeClassName;
                    break;
                case SARenderGraphData.NodeType.ComputerBuffer:
                case SARenderGraphData.NodeType.TextureBuffer:
                    nodeTypeName = SARenderGraphData.NodeType.BufferNode.ToString();
                    break;
                default:
                    nodeTypeName = baseNode.type.ToString();
                    break;
            }

            var className = $"UnityEditor.Rendering.{PackageNameSpace}.{NodeClassNamePrefix + nodeTypeName}";
            
            Type nodeClass = Type.GetType(className);
            
            Debug.Assert(nodeClass != null, $"Node class type is null : {NodeClassNamePrefix + nodeTypeName}");
            
            SANodeBase node = (SANodeBase)Activator.CreateInstance(nodeClass);
            
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
        
        public SAGroup CreateGroup(string title, Vector2 position, string guid = "")
        {
            SAGroup group = new SAGroup(title, position, guid);
            
            if (guid == "")
            {
                // if no guid, means that group is created from CreateGroup action , not from data loading
                // so we need to call this function to save the new group.
                OnElementCreated(group, position);
                
                foreach (var selectedElement in selection)
                {
                    if (!(selectedElement is SANodeBase))
                    {
                        continue;
                    }

                    OnElementsAddedToGroup(group, new [] { (GraphElement)selectedElement });
                    
                    group.AddElement(selectedElement as SANodeBase);
                    
                }
            }
            
            return group;
        }
        
    }
}