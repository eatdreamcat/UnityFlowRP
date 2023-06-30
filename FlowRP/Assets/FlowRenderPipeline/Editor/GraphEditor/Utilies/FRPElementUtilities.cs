using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.FlowPipeline;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace UnityEditor.Rendering.FlowPipeline
{
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

            textField.multiline = false;
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
        
        public static IntegerField CreateIntegerField(int value = 0, string label = null, EventCallback<ChangeEvent<int>> onValueChanged = null, bool isReadonly = false)
        {
            IntegerField integerField = new IntegerField()
            {
                value = value,
                label = label,
                isReadOnly = isReadonly,
                pickingMode = isReadonly ? PickingMode.Ignore : PickingMode.Position,
            };

            integerField.SetEnabled(!isReadonly);
            
            if (onValueChanged != null)
            {
                integerField.RegisterValueChangedCallback(onValueChanged);
            }

            return integerField;
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
            MaskField maskField = new MaskField(choices, value);
            maskField.label = label;

            if (onValueChanged != null)
            {
                maskField.RegisterValueChangedCallback(onValueChanged);
            }
            
            maskField.SetEnabled(!isReadonly);
            return maskField;
        }
        
        public static LayerMaskField CreateLayerMaskField(int value, string label,
            EventCallback<ChangeEvent<int>> onValueChanged = null, bool isReadonly = false)
        {
            LayerMaskField layerMaskField = new LayerMaskField()
            {
                value = value,
                label = label
            };

            if (onValueChanged != null)
            {
                layerMaskField.RegisterValueChangedCallback(onValueChanged);
            }
            
            layerMaskField.SetEnabled(!isReadonly);
            return layerMaskField;
        }

        public static EnumField CreateEnumField(Enum value, string label, EventCallback<ChangeEvent<Enum>> onValueChanged = null, bool isReadonly = false)
        {
            EnumField enumField = new EnumField();

            enumField.label = label;
            enumField.value = value;
            enumField.Init(value);

            if (onValueChanged != null)
            {
                enumField.RegisterValueChangedCallback(onValueChanged);
            }
            
            enumField.SetEnabled(!isReadonly);
            
            return enumField;
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

        public static VisualElement CreateListView<T, ItemElement>(
            List<T> data, string title, Func<VisualElement> makeItem, Action<VisualElement, int> bindItem,
            float itemHeight = 25, SelectionType selectionType = SelectionType.Single, bool reorderable = true,
            ListViewReorderMode reorderMode = ListViewReorderMode.Animated, bool showFoldoutHeader = true, bool showAddRemoveFooter = true) where ItemElement : VisualElement
        {
            VisualElement result;
            ListView listView = new ListView();
            listView.reorderMode = reorderMode;
            listView.headerTitle = title;
            listView.itemsSource = data;
            var extraHeightAdd = 0;
            listView.showFoldoutHeader = false;
            listView.showAddRemoveFooter = showAddRemoveFooter;
            listView.showAlternatingRowBackgrounds = AlternatingRowBackground.All;
            listView.userData =  new List<VisualElement>();

            float GetListHeight()
            {
                var totalHeight = 0f;
                totalHeight += extraHeightAdd * 23;
                // var scrollView = listView.Q<ScrollView>();
                // var viewport = scrollView.contentViewport;
                // var contentContainer = scrollView.contentContainer;   
                // totalHeight += contentContainer.worldBound.height;

                totalHeight += listView.itemsSource.Count * listView.fixedItemHeight;
                
                return totalHeight;
            }
            if (showFoldoutHeader)
            {
                
                var header = new VisualElement();
                // list foldout
                var foldout = new Foldout();
                header.Add(foldout);
                foldout.text = title;
                foldout.Add(listView);
                foldout.RegisterValueChangedCallback(evt =>
                {
                    if (itemHeight > 0)
                    {
                        if (evt.newValue)
                        {
                            listView.style.height = GetListHeight();
                        }
                        else
                        {
                            listView.style.height = 0;
                        }
                    }
                });
                listView.style.left = -6;
             
                // array size 
                var arraySize = CreateIntegerField(listView.itemsSource.Count, null, evt =>
                {
                    int itemCount = listView.itemsSource.Count;
                    if (evt.newValue > itemCount)
                    {
                        var list = new List<T>()
                        {
                            Capacity = evt.newValue - itemCount
                        };
                        for (int i = 0; i < list.Capacity; ++i)
                        {
                            list.Add(data[data.Count-1]);
                        }
                        listView.itemsSource.AddRange(list);
                    }
                    else
                    {
                        if (evt.newValue >= itemCount)
                        {
                            return;
                        }

                        for (int index = itemCount - 1; index >= evt.newValue; --index)
                        {
                            listView.itemsSource.RemoveAt(index);
                        }
                    }

                    if (itemHeight > 0)
                    {
                        listView.style.height = GetListHeight();
                    }
                    listView.Rebuild();
                    
                });

                listView.itemsAdded += ints =>
                { 
                    arraySize.value = listView.itemsSource.Count;
                };

                listView.itemsRemoved += ints =>
                {
                    arraySize.value = listView.itemsSource.Count;
                };
                
                arraySize.focusable = true;
                arraySize.isDelayed = true;
                arraySize.AddToClassList(ListView.arraySizeFieldUssClassName);
                header.Add(arraySize);

                result = header;
            }
            else
            {
                result = listView;
            }

            if (showAddRemoveFooter)
            {
                ++extraHeightAdd;
            }
            
            listView.makeItem = makeItem;
            listView.bindItem = bindItem;
            listView.selectionType = selectionType;
            
            
            listView.reorderable = reorderable;

            // to prevent the event propagation 
            listView.RegisterCallback<MouseDownEvent>(evt =>
            {
                evt.StopImmediatePropagation();
            });

            listView.RegisterCallback<DragEnterEvent>(evt =>
            {
                evt.StopImmediatePropagation();
            });

            if (itemHeight > 0)
            {
                listView.fixedItemHeight = itemHeight;
                listView.style.height = GetListHeight();
            }

            if (listView.showAddRemoveFooter && itemHeight > 0)
            {
                listView.itemsAdded += ints =>
                {
                    listView.style.height = GetListHeight();
                };

                listView.itemsRemoved += ints =>
                {
                    listView.style.height = GetListHeight();
                }; 
            }
            
            return result;
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
            LayerMaskField layerMaskField= CreateLayerMaskField(layerMask.value, "LayerMask", evt =>
            {
                if (!isReadonly)
                {
                    cullingMaskChanged(evt.newValue);
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

        public static VisualElement CreateMaterialParameter(
            FlowRenderGraphData.MaterialParameterNode materialParameter,
            EventCallback<ChangeEvent<Enum>> onQueueStartChanged, 
            EventCallback<ChangeEvent<Enum>> onQueueEndChanged, 
            bool isReadonly = false)
        {
            VisualElement materialRoot = new VisualElement();

            // render queue range
            var queueStartField = CreateEnumField(materialParameter.renderQueueRange.start, "Queue Start", onQueueStartChanged, isReadonly);
            materialRoot.Add(queueStartField);
            
            var queueEndField = CreateEnumField(materialParameter.renderQueueRange.end, "Queue End", onQueueEndChanged, isReadonly);
            materialRoot.Add(queueEndField);
            
            EventCallback<ChangeEvent<string>> callback = evt =>
            {
                var index = (int)((TextField) evt.target).userData;
                materialParameter.shaderTagList[index] = evt.newValue;
            };
            
            // shader tags 
            var shaderTagsField = CreateListView<string, TextField>(materialParameter.shaderTagList, "ShaderTags", () =>
            {
                return CreateTextField("New-Textfield", null, null);
                
            }, (element, i) =>
            {
                var textField = element as TextField;
                textField.value = materialParameter.shaderTagList[i];
                textField.userData = i;
                textField.UnregisterValueChangedCallback(callback);
                textField.RegisterValueChangedCallback(callback);

            }, 22);
            
            materialRoot.Add(shaderTagsField);
            
            // material object field
            var materialOverride = CreateObjectField(materialParameter.overrideMaterial, "Material", evt =>
            {
                materialParameter.overrideMaterial = (Material)evt.newValue;
                
            }, isReadonly);

            materialOverride.style.left = isReadonly ? 0 : 10;

            if (!isReadonly)
            {
                var isOverrideMaterialToggle =
                    CreateToggle(materialParameter.overrideMaterial != null, "OverrideMaterial", evt =>
                    {
                        if (evt.newValue)
                        {
                            materialRoot.Add(materialOverride);
                        }
                        else
                        {
                            materialRoot.Remove(materialOverride);
                            materialOverride.value = null;
                            materialParameter.overrideMaterial = null;
                        }
                    
                    }, isReadonly);

               
                materialRoot.Add(isOverrideMaterialToggle);
                
                isOverrideMaterialToggle.value = materialParameter.overrideMaterial != null;
                if (isOverrideMaterialToggle.value)
                {
                    materialRoot.Add(materialOverride);
                }
                
            } else {
                
                materialRoot.Add(materialOverride);
            }
            
            
            materialRoot.SetEnabled(!isReadonly);
            materialRoot.style.left = 5;
            return materialRoot;
        }

        public class BlendStateElement : VisualElement
        {
            private bool m_hasInitialized = false;
            private FlowRenderGraphData.BlendStateData m_BlendStateData;
            public void Initialize(FlowRenderGraphData.BlendStateData tempblendStateData)
            {
                if (m_hasInitialized) return;
                m_hasInitialized = true;
                
                m_WriteMask = CreateMaskField((int)tempblendStateData.writeMask, "WriteMask", new List<string>()
                    {
                        "Alpha", // 1   0001
                        /// <summary>
                        ///   <para>Write blue component.</para>
                        /// </summary>
                        "Blue", // 2    0010
                        /// <summary>
                        ///   <para>Write green component.</para>
                        /// </summary>
                        "Green", // 4   0100
                        /// <summary>
                        ///   <para>Write red component.</para>
                        /// </summary>
                        "Red", // 8     1000
                       //  "All" // 15
                    }, evt =>
                {
                    if (m_BlendStateData != null)
                    {
                        m_BlendStateData.writeMask = (ColorWriteMask)evt.newValue;
                    }
                });
             
                Add(m_WriteMask);

                // color blend mode
                m_SourceColorBlendMode = CreateEnumField(tempblendStateData.sourceColorBlendMode,
                    "Source Color Blend Mode",
                    evt =>
                    {
                        if (m_BlendStateData != null)
                        {
                            m_BlendStateData.sourceColorBlendMode = (BlendMode)evt.newValue;
                        }
                    });
                Add(m_SourceColorBlendMode);
                
                m_DestinationColorBlendMode = CreateEnumField(tempblendStateData.destinationColorBlendMode,
                    "Destination Color Blend Mode",
                    evt =>
                    {
                        if (m_BlendStateData != null)
                        {
                            m_BlendStateData.destinationColorBlendMode = (BlendMode)evt.newValue;
                        }
                    });
                Add(m_DestinationColorBlendMode);
                
                // alpha blend mode
                m_SourceAlphaBlendMode = CreateEnumField(tempblendStateData.sourceAlphaBlendMode,
                    "Source Alpha Blend Mode",
                    evt =>
                    {
                        if (m_BlendStateData != null)
                        {
                            m_BlendStateData.sourceAlphaBlendMode = (BlendMode)evt.newValue;
                        }
                    });
                Add(m_SourceAlphaBlendMode);
                
                m_DestinationAlphaBlendMode = CreateEnumField(tempblendStateData.destinationAlphaBlendMode,
                    "Destination Alpha Blend Mode",
                    evt =>
                    {
                        if (m_BlendStateData != null)
                        {
                            m_BlendStateData.destinationAlphaBlendMode = (BlendMode)evt.newValue;
                        }
                    });
                Add(m_DestinationAlphaBlendMode);
                
                m_ColorBlendOperation = CreateEnumField(tempblendStateData.colorBlendOperation,
                    "Color Blend Operation",
                    evt =>
                    {
                        if (m_BlendStateData != null)
                        {
                            m_BlendStateData.colorBlendOperation = (BlendOp)evt.newValue;
                        }
                    });
                Add(m_ColorBlendOperation);
                
                m_AlphaBlendOperation = CreateEnumField(tempblendStateData.alphaBlendOperation,
                    "Alpha Blend Operation",
                    evt =>
                    {
                        if (m_BlendStateData != null)
                        {
                            m_BlendStateData.alphaBlendOperation = (BlendOp)evt.newValue;
                        }
                    });
                Add(m_AlphaBlendOperation);
            }
            
          
            public void Update(FlowRenderGraphData.BlendStateData blendStateData)
            {
                if (!m_hasInitialized) return;

                m_BlendStateData = blendStateData;
                m_WriteMask.value = (int)blendStateData.writeMask;
                
                m_BlendStateData.sourceColorBlendMode = blendStateData.sourceColorBlendMode;
                m_BlendStateData.destinationColorBlendMode = blendStateData.destinationColorBlendMode;

                m_BlendStateData.sourceAlphaBlendMode = blendStateData.sourceAlphaBlendMode;
                m_BlendStateData.destinationAlphaBlendMode = blendStateData.destinationAlphaBlendMode;


                m_BlendStateData.colorBlendOperation = blendStateData.colorBlendOperation;
                m_BlendStateData.alphaBlendOperation = blendStateData.alphaBlendOperation;
                
            }

            
            private MaskField m_WriteMask;
            private EnumField m_SourceColorBlendMode;
            private EnumField m_DestinationColorBlendMode;
            
            private EnumField m_SourceAlphaBlendMode;
            private EnumField m_DestinationAlphaBlendMode;
            
            private EnumField m_ColorBlendOperation;
            private EnumField m_AlphaBlendOperation;

        }
        public static BlendStateElement CreateBlendStateParameter(FlowRenderGraphData.BlendStateData blendStateData)
        {
            var blendStateRoot = new BlendStateElement();
            blendStateRoot.RegisterCallback<MouseDownEvent>(evt =>
            {
                evt.StopImmediatePropagation();
            });

            blendStateRoot.Initialize(blendStateData);
            
            return blendStateRoot;
        }
        public static VisualElement CreateStateParameter(FlowRenderGraphData.RenderStateNode stateParameter, bool isReadonly = false)
        {
            VisualElement stateRoot = new VisualElement();

            #region Raster State

            var rasterFoldout = new Foldout()
            {
                text = "Raster State",
                value = isReadonly
            };
            stateRoot.Add(rasterFoldout);

            var cullModeField = CreateEnumField(stateParameter.rasterState.cullingMode, "CullMode", evt =>
            {
                stateParameter.rasterState.cullingMode = (CullMode)evt.newValue;
            }, isReadonly);
            rasterFoldout.Add(cullModeField);

            var offsetUnitsField = CreateIntegerField(stateParameter.rasterState.offsetUnits, "OffsetUnits", evt =>
            {
                stateParameter.rasterState.offsetUnits = evt.newValue;
            }, isReadonly);
            rasterFoldout.Add(offsetUnitsField);

            var offsetFactorField = CreateFloatField(stateParameter.rasterState.offsetFactor, "OffsetFactor", evt =>
            {
                stateParameter.rasterState.offsetFactor = evt.newValue;
            }, isReadonly);
            rasterFoldout.Add(offsetFactorField);

            var depthClipToggle = CreateToggle(stateParameter.rasterState.depthClip, "DepthClip", evt =>
            {
                stateParameter.rasterState.depthClip = evt.newValue;
            }, isReadonly);
            rasterFoldout.Add(depthClipToggle);

            #endregion
            
            #region Depth State

            var depthFoldout = new Foldout()
            {
                text = "Depth State",
                value = isReadonly
            };
            stateRoot.Add(depthFoldout);

            var writeEnableToggle = CreateToggle(stateParameter.depthState.writeEnabled, "WriteEnabled", evt =>
            {
                stateParameter.depthState.writeEnabled = evt.newValue;
                
            }, isReadonly);
            depthFoldout.Add(writeEnableToggle);

            var depthCompareFuncField = CreateEnumField(stateParameter.depthState.compareFunction, "DepthCompare", evt =>
            {
                stateParameter.depthState.compareFunction = (CompareFunction)evt.newValue;
            }, isReadonly);
            depthFoldout.Add(depthCompareFuncField);

            #endregion
            
            #region Stencil State

            var stencilFoldout = new Foldout()
            {
                text = "Stencil State",
                value = isReadonly
            };
            stateRoot.Add(stencilFoldout);

            var enabledField = CreateToggle(stateParameter.stencilState.enabled, "Enabled", evt =>
            {
                stateParameter.stencilState.enabled = evt.newValue;
            }, isReadonly);
            stencilFoldout.Add(enabledField);

            var readMaskField = CreateIntegerField(stateParameter.stencilState.readMask, "ReadMask", evt =>
            {
                stateParameter.stencilState.readMask = (byte)evt.newValue;
                ((IntegerField) evt.target).value = stateParameter.stencilState.readMask;
                
            }, isReadonly);
            stencilFoldout.Add(readMaskField);
            
            var writeMaskField = CreateIntegerField(stateParameter.stencilState.writeMask, "WriteMask", evt =>
            {
                stateParameter.stencilState.writeMask = (byte)evt.newValue;
                ((IntegerField) evt.target).value = stateParameter.stencilState.writeMask;
                
            }, isReadonly);
            stencilFoldout.Add(writeMaskField);

            #region Front

            var compareFunctionFrontField = CreateEnumField(stateParameter.stencilState.compareFunctionFront,
                "CompareFunctionFront",
                evt =>
                {
                    stateParameter.stencilState.compareFunctionFront = (CompareFunction) evt.newValue;
                    
                }, isReadonly);
            stencilFoldout.Add(compareFunctionFrontField);

            var passOpFrontField = CreateEnumField(stateParameter.stencilState.passOperationFront, "PassOperationFront",
                evt =>
                {
                    stateParameter.stencilState.passOperationFront = (StencilOp) evt.newValue;

                }, isReadonly);
            stencilFoldout.Add(passOpFrontField);
            passOpFrontField.style.left = 10;
            
            var failOpFrontField = CreateEnumField(stateParameter.stencilState.failOperationFront, "FailOperationFront",
                evt =>
                {
                    stateParameter.stencilState.failOperationFront = (StencilOp) evt.newValue;

                }, isReadonly);
            stencilFoldout.Add(failOpFrontField);
            failOpFrontField.style.left = 10;
            
            var zFailOpFrontField = CreateEnumField(stateParameter.stencilState.zFailOperationFront, "Z-FailOperationFront",
                evt =>
                {
                    stateParameter.stencilState.zFailOperationFront = (StencilOp) evt.newValue;

                }, isReadonly);
            stencilFoldout.Add(zFailOpFrontField);
            zFailOpFrontField.style.left = 10;

            #endregion

            #region Back

            var compareFunctionBackField = CreateEnumField(stateParameter.stencilState.compareFunctionBack,
                "CompareFunctionBack",
                evt =>
                {
                    stateParameter.stencilState.compareFunctionBack = (CompareFunction) evt.newValue;
                    
                }, isReadonly);
            stencilFoldout.Add(compareFunctionBackField);

            var passOpBackField = CreateEnumField(stateParameter.stencilState.passOperationBack, "PassOperationBack",
                evt =>
                {
                    stateParameter.stencilState.passOperationBack = (StencilOp) evt.newValue;

                }, isReadonly);
            stencilFoldout.Add(passOpBackField);
            passOpBackField.style.left = 10;
            
            var failOpBackField = CreateEnumField(stateParameter.stencilState.failOperationBack, "FailOperationBack",
                evt =>
                {
                    stateParameter.stencilState.failOperationBack = (StencilOp) evt.newValue;

                }, isReadonly);
            stencilFoldout.Add(failOpBackField);
            failOpBackField.style.left = 10;
            
            var zFailOpBackField = CreateEnumField(stateParameter.stencilState.zFailOperationBack, "Z-FailOperationBack",
                evt =>
                {
                    stateParameter.stencilState.zFailOperationBack = (StencilOp) evt.newValue;

                }, isReadonly);
            stencilFoldout.Add(zFailOpBackField);
            zFailOpBackField.style.left = 10;

            #endregion
            
            #endregion
            
            #region Blend State
            
            var tempData = new FlowRenderGraphData.BlendStateData();
           
            var blendStatesListView = CreateListView<FlowRenderGraphData.BlendStateData, BlendStateElement>(stateParameter.blendStates, "Blend States", () =>
            {
                return CreateBlendStateParameter(tempData);
                
            }, (element, i) =>
            {
                stateParameter.blendStates[i] = stateParameter.blendStates[i] is null
                    ? new FlowRenderGraphData.BlendStateData()
                    : stateParameter.blendStates[i];
                
                var blendStateElement = element as BlendStateElement;
                var data = stateParameter.blendStates[i];
                blendStateElement.Update(data);
                
            }, 23 * 7);
            stateRoot.Add(blendStatesListView);

            #endregion

            stateRoot.SetEnabled(!isReadonly);
            return stateRoot;
        }
        #endregion
    }

}