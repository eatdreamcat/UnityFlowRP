using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering.SuperAdvanced;
using UnityEngine.UIElements;

namespace UnityEditor.Rendering.SuperAdvanced
{
  
    public class SANodeBase : Node 
    {

        #region Private

        protected SAGraphView m_View;

        protected SARenderGraphData.BaseNode m_GraphNodeData;

        private void DrawTitle()
        {

            TextField dialogueNameTextField = SAElementUtilities.CreateTextField(m_GraphNodeData.name, null, callback =>
            {
                TextField target = (TextField) callback.target;

                target.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();

                if (string.IsNullOrEmpty(target.value))
                {
                    EditorUtility.DisplayDialog("Damn!", "Name cant be null or empty, OK? ", "Yes Sir!");
                    return;
                }

                if (target.value == m_GraphNodeData.name)
                {
                    return;
                }
               
                m_GraphNodeData.name = target.value;
                m_View.UpdateNodeTitle(m_GraphNodeData.name, this);
                
            }, EntryPoint);

            
            dialogueNameTextField.AddClasses(
                "SA-node__text-field",
                "SA-node__text-field__hidden",
                "SA-node__filename-text-field"
            );
                
            titleContainer.Insert(0, dialogueNameTextField);

        }

        private void DrawPorts()
        {
            if (EntryPoint)
            {
                FlowOut = this.CreatePort("Start");
                outputContainer.Add(FlowOut);
            }
            else 
            {
                switch (Type)
                {
                    case SARenderGraphData.NodeType.CameraParameterNode:
                    {
                        FlowOut = this.CreatePort("Assign-To", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(SACameraParameterNode));
                        outputContainer.Add(FlowOut);
                    }
                        break;
                    
                    case SARenderGraphData.NodeType.CullingParameterNode:
                    {
                        FlowOut = this.CreatePort("Assign-To", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(SACullingParameterNode));
                        outputContainer.Add(FlowOut);
                    }
                        break;
                    
                    case SARenderGraphData.NodeType.RenderMaterialNode:
                    {
                        FlowOut = this.CreatePort("Assign-To", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(SARenderMaterialNode));
                        outputContainer.Add(FlowOut);
                    }
                        break;
                    
                    case SARenderGraphData.NodeType.RenderStateNode:
                    {
                        FlowOut = this.CreatePort("Assign-To", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(SARenderStateNode));
                        outputContainer.Add(FlowOut);
                    }
                        break;
                    case SARenderGraphData.NodeType.BufferNode:
                    {
                        FlowOut = this.CreatePort("Assign-To", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(BufferAsPassInput));
                        outputContainer.Add(FlowOut);
                        
                        FlowIn = this.CreatePort("Write-To", Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(BufferAsPassOutput));
                        inputContainer.Add(FlowIn);
                    }
                        break;
                    
                    case SARenderGraphData.NodeType.DrawRendererNode:
                    case SARenderGraphData.NodeType.DrawFullScreenNode:   
                    {
                        /* INPUT CONTAINER */
                        FlowIn = this.CreatePort("Flow-In", Orientation.Horizontal, Direction.Input, Port.Capacity.Single);
                        inputContainer.Add(FlowIn);

                        FlowOut = this.CreatePort("Flow-Out");
                        outputContainer.Add(FlowOut);
                    }
                        break;
                }
            }
        }

        protected virtual void DrawExtensionContent()
        {
            // do nothing
        }
        
        
        private void DisconnectInputPorts()
        {
            DisconnectPorts(inputContainer);
        }

        private void DisconnectOutputPorts()
        {
            DisconnectPorts(outputContainer);
        }
        
        private void DisconnectPorts(VisualElement container)
        {
            foreach (Port port in container.Children())
            {
                if (!port.connected)
                {
                    continue;
                }

                m_View.DeleteElements(port.connections);
            }
        }
        
       
        #endregion

        #region Protected
        
        

        #endregion



        #region Public

        public string Name => m_GraphNodeData.name;
        public string ID => m_GraphNodeData.guid;

        public Port FlowIn { get; set; }
        public Port FlowOut { get; set; }

        public SAGroup Group { get; set; }

        public bool EntryPoint
        {
            get
            {
                return Type == SARenderGraphData.NodeType.EntryNode;
            }
        }

        public SARenderGraphData.NodeType Type => m_GraphNodeData.type;

        public virtual void Initialize(SAGraphView view, Vector2 position, SARenderGraphData.BaseNode nodeData)
        {
            m_View = view;
            SetPosition(new Rect(position, new Vector2(0, 0 )));
            mainContainer.AddToClassList("SA-node__main-container");
            extensionContainer.AddToClassList("SA-node__extension-container");
            m_GraphNodeData = nodeData;
            
            layer = 0;
          
        }

        public virtual void Draw(bool refresh = true)
        {
            
            /* TITLE CONTAINER */

            DrawTitle();
           
            /* PORTS */
            
            DrawPorts();
            
            /* EXTENSION CONTAINER */
            
            DrawExtensionContent();


            if (refresh)
            {
                RefreshExpandedState();
            }
        }
        

        #endregion
    }
}