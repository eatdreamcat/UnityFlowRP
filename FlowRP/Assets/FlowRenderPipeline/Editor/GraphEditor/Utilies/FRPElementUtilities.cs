using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

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
        
        public static FloatField CreateFloatField(float value = 0f, string label = null, EventCallback<ChangeEvent<float>> onValueChanged = null, bool isReadonly = false)
        {
            FloatField floatField = new FloatField()
            {
                value = value,
                label = label,
                isReadOnly = isReadonly,
                pickingMode = isReadonly ? PickingMode.Ignore : PickingMode.Position,
            };

            floatField.SetEnabled(!isReadonly);
            
            if (onValueChanged != null)
            {
                floatField.RegisterValueChangedCallback(onValueChanged);
            }

            return floatField;
        }
        
        public static Vector2Field CreateVector2Field(Vector2 value, string label = null, EventCallback<ChangeEvent<Vector2>> onValueChanged = null, bool isReadonly = false)
        {
            var vector2 = new Vector2Field()
            {
                value = value,
                label = label,
                pickingMode = isReadonly ? PickingMode.Ignore : PickingMode.Position,
            };
            vector2.SetEnabled(!isReadonly);
            if (onValueChanged != null)
            {
                vector2.RegisterValueChangedCallback(onValueChanged);
            }
                    
            return vector2;
        }
        
        public static Vector3Field CreateVector3Field(Vector3 value, string label = null, EventCallback<ChangeEvent<Vector3>> onValueChanged = null, bool isReadonly = false)
        {
            var vector3 = new Vector3Field()
            {
                value = value,
                label = label,
                pickingMode = isReadonly ? PickingMode.Ignore : PickingMode.Position,
                
            };
            vector3.SetEnabled(!isReadonly);
            if (onValueChanged != null)
            {
                vector3.RegisterValueChangedCallback(onValueChanged);
            }
                    
            return vector3;
        }
        
        public static Vector4Field CreateVector4Field(Vector4 value, string label = null, EventCallback<ChangeEvent<Vector4>> onValueChanged = null, bool isReadonly = false)
        {
            var vector4 = new Vector4Field()
            {
                value = value,
                label = label,
                pickingMode = isReadonly ? PickingMode.Ignore : PickingMode.Position,
            };
            vector4.SetEnabled(!isReadonly);
            if (onValueChanged != null)
            {
                vector4.RegisterValueChangedCallback(onValueChanged);
            }
                    
            return vector4;
        }

        public static MaskField CreateMaskField(int value, string label, List<string> choices,
            EventCallback<ChangeEvent<int>> onValueChanged = null, bool isReadonly = false)
        {
            MaskField layerMaskField = new MaskField(choices, value);
            layerMaskField.label = label;

            if (onValueChanged != null)
            {
                layerMaskField.RegisterValueChangedCallback(onValueChanged);
            }
            
            layerMaskField.SetEnabled(!isReadonly);
            return layerMaskField;
        }

        public static ObjectField CreateObjectField<T>(T value, string label, EventCallback<ChangeEvent<Object>> onValueChanged = null, bool isReadonly = false) where T : UnityEngine.Object
        {
            ObjectField objectField = new ObjectField();
            objectField.objectType = typeof(T);
            objectField.value = value;
            objectField.label = label;
            if (onValueChanged != null)
            {
                objectField.RegisterValueChangedCallback(onValueChanged);
            }

            return objectField;
        }

        public static Toggle CreateToggle(bool value, string label, EventCallback<ChangeEvent<bool>> onValueChanged,
            bool isReadonly = false)
        {
            Toggle toggle = new Toggle()
            {
                value = value,
                label = label
            };

            if (onValueChanged != null)
            {
                toggle.RegisterValueChangedCallback(onValueChanged);
            }
            
            toggle.SetEnabled(!isReadonly);

            return toggle;
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


        #region Node Parameters

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
            Toggle passCulling = CreateToggle(isAllowPassCulling, "PassCulling", isAllowPassCullingChanged, isReadonly);
            cullingRoot.Add(passCulling);

            // renderer culling toggle
            Toggle rendererCulling = CreateToggle(isAllowRendererCulling, "AllowRendererCulling", isAllowRendererCullingChanged, isReadonly);
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

            MaskField layerMaskField = CreateMaskField(LayerValueToMaskFieldValue(layerMask.value), "LayerMask",
                layerNames,
                evt =>
                {
                    if (!isReadonly)
                    {
                        cullingMaskChanged(MaskFieldValueToLayerValue(evt.newValue));
                    }
                    
                }, isReadonly);
            
            cullingRoot.Add(layerMaskField);
            cullingRoot.SetEnabled(!isReadonly);
            cullingRoot.style.left = 5;
            
            return cullingRoot;
        }

        public static VisualElement CreateCameraParameter(
            float fov, EventCallback<ChangeEvent<float>> fovChanged,
            Vector3 offset, EventCallback<ChangeEvent<Vector3>> offsetChanged,
            bool isReadonly = false)
        {
            VisualElement cameraRoot = new VisualElement();
            cameraRoot.style.overflow = Overflow.Visible;

            // fov
            var fovField = CreateFloatField(fov, "Fov", fovChanged, isReadonly);
            cameraRoot.Add(fovField);
            
            // offset
            var offsetField = CreateVector3Field(offset, "Offset", offsetChanged, isReadonly);
            cameraRoot.Add(offsetField);

            cameraRoot.SetEnabled(!isReadonly);    
            return cameraRoot;
        }

        #endregion
    }

}