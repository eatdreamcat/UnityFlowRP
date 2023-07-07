using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.Rendering.SuperAdvanced
{
    public partial class SAGraphView
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
                    if (element is SANodeBase)
                    {
                        UpdateNodePositionData((SANodeBase)element);
                    } else if (element is SAGroup)
                    {
                        UpdateGroupPositionData((SAGroup)element);
                    }
                }
            }
            
            
            if (graphViewChange.elementsToRemove != null && graphViewChange.elementsToRemove.Count > 0)
            {
                foreach (var element in graphViewChange.elementsToRemove)
                {
                    if (element is SANodeBase)
                    {
                        DeleteNode((SANodeBase) element);
                        
                    } else if (element is Edge)
                    {
                        DeleteEdge((Edge) element);
                    } 
                }
            }

            if (graphViewChange.edgesToCreate != null && graphViewChange.edgesToCreate.Count > 0)
            {
                foreach (var element in graphViewChange.edgesToCreate)
                {
                    element.layer = 1;
                    
                    // we only hook edge created by editing.
                    if (element.userData == null || (bool) element.userData == false)
                    {
                        AddEdge(element);
                    }
                }
            }
            
            
            return graphViewChange;
        }

       
        private void OnGroupTitleChanged(Group group, string title)
        {
            UpdateGroupTitle(title, group as SAGroup);
        }
        
        /* this is called by manully in GraphView.Node file. */
        private void OnElementCreated(GraphElement element, Vector2 position)
        {
            if (element is SANodeBase)
            {
                // do nothing
                
            } else if (element is SAGroup)
            {
                AddNewGroupToData((SAGroup) element, position);
            }
        }
        public void OnElementsAddedToGroup(Group group, IEnumerable<GraphElement> elements)
        {
            SAGroup saGroup = group as SAGroup;

            AddNodesToGroup(saGroup, elements.ToList());
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

        private void OnViewTransformChanged(GraphView view)
        {
            m_GraphViewSavedData.UpdateViewTransform(m_CurrentRenderGraphData.GraphGuid,
                new SAGraphViewSavedData.ViewTransformData()
                {
                    position = viewTransform.position,
                    scale = viewTransform.scale,
                });
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
            viewTransformChanged += OnViewTransformChanged;
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
            viewTransformChanged -= OnViewTransformChanged;
        }
        
    }
}