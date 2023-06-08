using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace UnityEditor.Rendering.FlowPipeline
{
    public partial class FRPGraphView
    {
        
        public enum FRPNodeType
        {
            FRPNodeBase,
            FRPBranchNode,
            FRPRenderRequestNode,
            FRPRenderTargetNode,
            FRPResourceNode,
            FRPRenderTextureNode,
        }

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
        
        // private FRPSerializableDictionary<string, DSNodeErrorData> m_UngroupedNodes;
        // private FRPSerializableDictionary<string, DSGroupErrorData> m_Groups;
        // private FRPSerializableDictionary<Group, FRPSerializableDictionary<string, DSNodeErrorData>> m_GroupedNodes;
        
        FRPNodeBase CreateNode(string nodeName, FRPNodeType nodeType, Vector2 position, bool shouldDraw = true, bool isEntry = false)
        {
            Type nodeClass = Type.GetType($"UnityEditor.Rendering.FlowPipeline.{nodeType}");
            FRPNodeBase node = (FRPNodeBase)Activator.CreateInstance(nodeClass);
            
            node.Initialize(nodeName, this, position, isEntry);
            if (shouldDraw)
            {
                node.Draw();
            }
            
            return node;
        }
        
        public Group CreateGroup(string title, Vector2 position)
        {
            Group group = new Group()
            {
                title = title
            };

            group.SetPosition(new Rect(position, Vector2.zero));
            
            return group;
        }
        
        
        
        // static void AddUngroupedNode(FRPNodeBase node)
        // {
        //     string nodeName = node.DialogueName.ToLower();
        //
        //     if (!ungroupedNodes.ContainsKey(nodeName))
        //     {
        //         DSNodeErrorData nodeErrorData = new DSNodeErrorData();
        //
        //         nodeErrorData.Nodes.Add(node);
        //
        //         ungroupedNodes.Add(nodeName, nodeErrorData);
        //
        //         return;
        //     }
        //
        //     List<DSNode> ungroupedNodesList = ungroupedNodes[nodeName].Nodes;
        //
        //     ungroupedNodesList.Add(node);
        //
        //     Color errorColor = ungroupedNodes[nodeName].ErrorData.Color;
        //
        //     node.SetErrorStyle(errorColor);
        //
        //     if (ungroupedNodesList.Count == 2)
        //     {
        //         ++NameErrorsAmount;
        //
        //         ungroupedNodesList[0].SetErrorStyle(errorColor);
        //     }
        // }

    }
}