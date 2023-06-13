using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.Rendering.FlowPipeline
{
    public partial class FRPGraphView
    {

        /// graphViewChanged
        /// groupTitleChanged
        /// elementsAddedToGroup
        /// elementsRemovedFromGroup
        /// elementsInsertedToStackNode
        /// elementsRemovedFromStackNode
        /// elementResized
        /// viewTransformChanged
        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
           
            if (graphViewChange.movedElements != null && graphViewChange.movedElements.Count > 0)
            {
                foreach (var element in graphViewChange.movedElements)
                {
                    if (element is FRPNodeBase)
                    {
                        UpdateNodePositionData((FRPNodeBase)element);
                    }
                }
            }
            
            
            if (graphViewChange.elementsToRemove != null && graphViewChange.elementsToRemove.Count > 0)
            {
                foreach (var element in graphViewChange.elementsToRemove)
                {
                    Debug.Log("to remove element:" + element.name);
                }
            }

            if (graphViewChange.edgesToCreate != null && graphViewChange.edgesToCreate.Count > 0)
            {
                foreach (var element in graphViewChange.edgesToCreate)
                {
                    Debug.Log("edges To Create:" + element.name);
                }
            }
            
            
            return graphViewChange;
        }

        /* this is called by manully in GraphView.Node file. */
        private void OnGroupTitleChanged(Group group, string title)
        {
            Debug.Log($"{group.name} -OnGroupTitleChanged- {title}");
        }

        private void OnElementCreated(GraphElement element, Vector2 position)
        {
            if (element is FRPNodeBase)
            {
                AddNewNodeToData((FRPNodeBase) element, position);
            }
        }
        private void OnElementsAddedToGroup(Group group, IEnumerable<GraphElement> elements)
        {
            Debug.Log($"{group.name} -OnGroupTitleChanged- {elements.Count()}");
        }
        
        private void OnElementsRemovedFromGroup(Group group, IEnumerable<GraphElement> elements)
        {
            Debug.Log($"{group.name} -OnElementsRemovedFromGroup- {elements.Count()}");
        }

        private void OnElementsInsertedToStackNode(StackNode stackNode, int index, IEnumerable<GraphElement> elements)
        {
            Debug.Log($"{stackNode.name} -OnElementsInsertedToStackNode- {elements.Count()}");
        }
        
        private void OnElementsRemovedFromStackNode(StackNode stackNode, IEnumerable<GraphElement> elements)
        {
            
            Debug.Log($"{stackNode.name} -OnElementsRemovedFromStackNode- {elements.Count()}");
        }

        private void OnElementResized(VisualElement visualElement)
        {
            Debug.Log($"{visualElement.name} -OnElementResized- ");
        }

        private void AddHook()
        {
            graphViewChanged += OnGraphViewChanged;
            groupTitleChanged += OnGroupTitleChanged;
            elementsAddedToGroup += OnElementsAddedToGroup;
            elementsRemovedFromGroup += OnElementsRemovedFromGroup;
            elementsInsertedToStackNode += OnElementsInsertedToStackNode;
            elementsRemovedFromStackNode += OnElementsRemovedFromStackNode;
            elementResized += OnElementResized;
        }

        private void RemoveHook()
        {
            graphViewChanged -= OnGraphViewChanged;
            groupTitleChanged -= OnGroupTitleChanged;
            elementsAddedToGroup -= OnElementsAddedToGroup;
            elementsRemovedFromGroup -= OnElementsRemovedFromGroup;
            elementsInsertedToStackNode -= OnElementsInsertedToStackNode;
            elementsRemovedFromStackNode -= OnElementsRemovedFromStackNode;
            elementResized -= OnElementResized;
        }
        
    }
}