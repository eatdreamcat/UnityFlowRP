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
                    if (!string.IsNullOrEmpty(Name))
                    {
                        // ++graphView.NameErrorsAmount;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(Name))
                    {
                        //  --graphView.NameErrorsAmount;
                    }
                }

                if (Group == null)
                {
                    //  m_View.RemoveUngroupedNode(this);

                    Name = target.value;

                    //    graphView.AddUngroupedNode(this);

                    return;
                }

                FRPNodeGroup currentGroup = Group;

                //       m_View.RemoveGroupedNode(this, Group);

                Name = target.value;

                //        m_View.AddGroupedNode(this, currentGroup);
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
                /* INPUT CONTAINER */

                Port inputPort = this.CreatePort("Input", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
                inputContainer.Add(inputPort);
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
        public bool EntryPoint { get; set; }

        public FlowRenderGraphData.FRPNodeType Type { get; set; }

        public virtual void Initialize(string name, FRPGraphView view, Vector2 position, FlowRenderGraphData.FRPNodeType type, bool isEntryPoint = false)
        {
            ID = Guid.NewGuid().ToString();

            m_View = view;

            Type = type;
            
            Name = name;
            
            m_DefaultBackgroundColor = new Color(29f / 255f, 29f / 255f, 30f / 255f);

            mainContainer.AddToClassList("ds-node__main-container");
            extensionContainer.AddToClassList("ds-node__extension-container");
            
            SetPosition(new Rect(position, new Vector2(100, 150)));

            EntryPoint = isEntryPoint;
            
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