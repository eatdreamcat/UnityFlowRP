using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering.FlowPipeline;
using UnityEngine.UIElements;

namespace UnityEditor.Rendering.FlowPipeline
{
  
    public class FRPNodeBase : Node
    {

        #region Private

        private FRPGraphView m_View;
        private Color m_DefaultBackgroundColor;


        private void DrawTitle()
        {

            TextField dialogueNameTextField = FRPElementUtilities.CreateTextField(Name, null, callback =>
            {
                TextField target = (TextField) callback.target;

                target.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();

                if (string.IsNullOrEmpty(target.value))
                {
                    EditorUtility.DisplayDialog("Damn!", "Name cant be null or empty, OK? ", "Yes Sir!");
                    return;
                }

                if (target.value == Name)
                {
                    return;
                }
               
                Name = target.value;
                m_View.UpdateNodeTitle(Name, this);
                
            }, EntryPoint);

            
            dialogueNameTextField.AddClasses(
                "ds-node__text-field",
                "ds-node__text-field__hidden",
                "ds-node__filename-text-field"
            );
                
            titleContainer.Insert(0, dialogueNameTextField);

        }

        private void DrawPorts()
        {
            if (EntryPoint)
            {
                Port nextPort = this.CreatePort("Next");
                outputContainer.Add(nextPort);
            }
            else 
            {
                switch (Type)
                {
                    case FlowRenderGraphData.FRPNodeType.FRPResourceNode:
                    {
                        Port outPort = this.CreatePort("Assign-To");
                        outputContainer.Add(outPort);
                    }
                        break;
                    case FlowRenderGraphData.FRPNodeType.FRPRenderRequestNode:
                    {
                        /* INPUT CONTAINER */
                        Port inPort = this.CreatePort("Flow-In", Orientation.Horizontal, Direction.Input, Port.Capacity.Single);
                        inputContainer.Add(inPort);
                
                        Port outPort = this.CreatePort("Flow-Out");
                        outputContainer.Add(outPort);
                    }
                        break;
                }
            }
        }

        private void DrawExtensionContent()
        {
            
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
        
        public string Name { get; set; }
        public string ID { get; set; }

        public FRPNodeGroup Group { get; set; }

        public bool EntryPoint
        {
            get
            {
                return Type == FlowRenderGraphData.FRPNodeType.Entry;
            }
        }

        public FlowRenderGraphData.FRPNodeType Type { get; set; }

        public virtual void Initialize(string name, FRPGraphView view, Vector2 position, FlowRenderGraphData.FRPNodeType type, string guid)
        {
            ID = guid != "" ? guid : Guid.NewGuid().ToString();

            m_View = view;

            Type = type;
            
            Name = name;
            
            m_DefaultBackgroundColor = new Color(29f / 255f, 29f / 255f, 30f / 255f);

            mainContainer.AddToClassList("ds-node__main-container");
            extensionContainer.AddToClassList("ds-node__extension-container");
            
            SetPosition(new Rect(position, new Vector2(100, 150)));

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