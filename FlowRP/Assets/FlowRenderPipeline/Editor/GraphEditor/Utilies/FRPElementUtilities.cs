using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.Rendering.FlowPipeline
{
    public static class FRPElementUtilities
    {
        public static Button CreateButton(string text, Action onClick = null)
        {
            Button button = new Button(onClick)
            {
               // text = text,
                style =
                {
                    backgroundImage = (EditorGUIUtility.Load(FRPPathUtility.kIconPath + "Save_Icon.png") as Texture2D),
                     
                }
            };

            return button;
        }

        
        
        public static TextField CreateTextField(string value = null, string label = null, EventCallback<ChangeEvent<string>> onValueChanged = null, bool isReadonly = false)
        {
            TextField textField = new TextField()
            {
                value = value,
                label = label,
                isReadOnly = isReadonly,
                pickingMode = isReadonly ? PickingMode.Ignore : PickingMode.Position,
            };

           textField.SetEnabled(!isReadonly);
            
            if (onValueChanged != null)
            {
                textField.RegisterValueChangedCallback(onValueChanged);
            }

            return textField;
        }
        
        
        
        public static Port CreatePort(this FRPNodeBase node, string portName = "", Orientation orientation = Orientation.Horizontal, Direction direction = Direction.Output, Port.Capacity capacity = Port.Capacity.Single)
        {
            Port port = node.InstantiatePort(orientation, direction, capacity, typeof(bool));

            port.portName = portName;

            return port;
        }
    }

}