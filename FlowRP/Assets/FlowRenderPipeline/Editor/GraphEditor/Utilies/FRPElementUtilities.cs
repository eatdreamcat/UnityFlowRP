using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.Rendering.FlowPipeline
{
    public enum MyEnum
    {
        A,
        B
    }
    public static class FRPElementUtilities
    {
        public static Button CreateButton(string text, Action onClick = null)
        {
            Button button = new Button(onClick)
            {
                text = text,
                // style =
                // {
                //     backgroundImage = (EditorGUIUtility.Load(FRPPathUtility.kIconPath + "Save_Icon.png") as Texture2D),
                //      
                // }
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

        public static bool CanAcceptConnector(Port startPort, Port endPort)
        {
            if (endPort.node != null && startPort.portType == endPort.portType)
            {
                return true; // Allow connection
            }
            else
            {
                return false; // Block connection
            } 
        }
        
        public static Port CreatePort(
            this FRPNodeBase node, 
            string portName = "",
            Orientation orientation = Orientation.Horizontal, 
            Direction direction = Direction.Output, 
            Port.Capacity capacity = Port.Capacity.Single,
            Type allowedNodeType = null
            )
        {
            allowedNodeType = allowedNodeType == null ? typeof(FRPNodeBase) : allowedNodeType;
            
            Port port = node.InstantiatePort(orientation, direction, capacity, allowedNodeType);

            port.portName = portName;

            return port;
        }

        public static VisualElement CreateCullingParameter(
            bool isAllowPassCulling,
            EventCallback<ChangeEvent<bool>> isAllowPassCullingChanged, 
            bool isAllowRendererCulling,
            EventCallback<ChangeEvent<bool>> isAllowRendererCullingChanged, 
            LayerMask layerMask,
            EventCallback<int> cullingMaskChanged, bool isReadonly = false)
        {
            VisualElement cullingRoot = new VisualElement();

            // pass culling toggle
            Toggle passCulling = new Toggle()
            {
                label = "AllowPassCulling",
                
            };
            passCulling.value = isAllowPassCulling;
            cullingRoot.Add(passCulling);

            // renderer culling toggle
            Toggle rendererCulling = new Toggle()
            {
                label = "AllowRendererCulling",
                
            };
            rendererCulling.value = isAllowRendererCulling;
           
            cullingRoot.Add(rendererCulling);
            
            // layer mask
            // Get the layer names and values
            List<string> layerNames = new List<string>();
            const int TotalBit = 32;
            for (int i = 0; i < TotalBit; ++i)
            {
                string layerName = InternalEditorUtility.GetLayerName(i);
                if (string.IsNullOrEmpty(layerName))
                {
                    continue;
                }
                layerNames.Add(layerName);
            }
            
            int MaskFieldValueToLayerValue(int maskFieldlValue)
            {
                // maskField (0 nothing, -1 everything)
                // layerMask (0 nothing, -1 everything, 1 default, ...)
                if (maskFieldlValue == 0 || maskFieldlValue == -1)
                {
                    return maskFieldlValue;
                }
                
                List<string> nameList = new List<string>();
                var bitMask = layerNames.Count;
                while (--bitMask >= 0)
                {
                    var layerBitMask = 1 << bitMask;
                    if ((maskFieldlValue & layerBitMask) == layerBitMask)
                    {
                        // Debug.Log($"BitMask :{bitMask} , Layer name:{layerNames[bitMask]}");
                        nameList.Add(layerNames[bitMask]);
                    }
                  
                }
                
                var layerValue = 0;
                for (int i = 0; i < nameList.Count; i++)
                {
                    layerValue += (1 << (LayerMask.NameToLayer(nameList[i])));
                }
                return layerValue;
            }

            int LayerValueToMaskFieldValue(int layer)
            {
                // maskField (0 nothing, -1 everything)
                // layerMask (0 nothing, -1 everything, 1 default, ...)
                if (layer == 0 || layer == -1)
                {
                    return layer;
                }
                List<string> nameList = new List<string>();
                var bitMask = TotalBit;
                while (--bitMask >= 0)
                {
                    var layerBitMask = 1 << bitMask;
                    if ((layer & layerBitMask) == layerBitMask)
                    {
                       // Debug.Log($"BitMask :{bitMask} , Layer name:{LayerMask.LayerToName(bitMask)}");
                        nameList.Add(LayerMask.LayerToName(bitMask));
                    }
                  
                }

                var maskFieldLayerValue = 0;
                for (int i = 0; i < nameList.Count; i++)
                {
                    maskFieldLayerValue += (1 << (layerNames.IndexOf(nameList[i])));
                }
                return maskFieldLayerValue;
            }
            
            MaskField layerMaskField = new MaskField(layerNames, LayerValueToMaskFieldValue(layerMask.value));
            layerMaskField.label = "LayerMask";

            if (!isReadonly)
            {
                layerMaskField.RegisterValueChangedCallback(evt =>
                {
                    cullingMaskChanged(MaskFieldValueToLayerValue(evt.newValue));
                });
                passCulling.RegisterValueChangedCallback(isAllowPassCullingChanged);
                rendererCulling.RegisterValueChangedCallback(isAllowRendererCullingChanged);
            }
            
            cullingRoot.Add(layerMaskField);
            cullingRoot.SetEnabled(!isReadonly);
            cullingRoot.style.left = 5;
            
            return cullingRoot;
        }
    }

}