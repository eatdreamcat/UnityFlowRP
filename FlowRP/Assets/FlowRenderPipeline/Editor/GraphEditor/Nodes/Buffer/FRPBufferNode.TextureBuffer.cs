using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.FlowPipeline;
using UnityEngine.UIElements;

namespace UnityEditor.Rendering.FlowPipeline
{
    public partial class FRPBufferNode
    {
        private FlowRenderGraphData.TextureBufferNode TextureBufferNode =>
            m_View.GraphData.TryGetTextureBufferNode(BufferNode.bufferID);
        
        private void AddFoldout(string title, VisualElement child, VisualElement parent = null, float left = 0)
        {
            var foldout = new Foldout()
            {
                text = title,
            };
            
            if (parent == null)
            {
                parent = mainContainer;
            }

            foldout.style.left = left;
            parent.Add(foldout);
            foldout.Add(child);
            foldout.value = false;
        }
        
        private void DrawTextureBuffer()
        {
            AddFoldout("Header", Header());
            AddFoldout("Size", Size());
            AddFoldout("Init State", InitState());
            AddFoldout("Format", Format());
            AddFoldout("Filter & Addressing", FilterAddressing());
            AddFoldout("Mipmap", Mipmap());
            AddFoldout("Memory", Memory());
            AddFoldout("MSAA", MSAA());
        }

        private VisualElement Header()
        {
            var root = new VisualElement();
            
            var isShadowMap = FRPElementUtilities.CreateToggle(TextureBufferNode.isShadowMap, "Shadow Map", evt =>
            {
                TextureBufferNode.isShadowMap = evt.newValue;
            });
            root.Add(isShadowMap);
            
            var fallbackToBlack = FRPElementUtilities.CreateToggle(TextureBufferNode.fallBackToBlackTexture, "Fall Back To Black", evt =>
            {
                TextureBufferNode.fallBackToBlackTexture = evt.newValue;
            });
            root.Add(fallbackToBlack);
            
            return root;
        }
        
       
        private VisualElement Size()
        {
            var root = new VisualElement();

            var sizeRoot = new VisualElement();
            
            var width = FRPElementUtilities.CreateIntegerField(TextureBufferNode.width, "Width", evt =>
            {
                TextureBufferNode.width = evt.newValue;
            });
            
            var height = FRPElementUtilities.CreateIntegerField(TextureBufferNode.height, "Height", evt =>
            {
                TextureBufferNode.height = evt.newValue;
            });

            var scale = FRPElementUtilities.CreateVector2Field(TextureBufferNode.scale, "Scale", evt =>
            {
                TextureBufferNode.scale = evt.newValue;
            });
            
            var sizeMode = FRPElementUtilities.CreateEnumField(TextureBufferNode.sizeMode, "Size Mode", evt =>
            {
                // not support for Functor..
                if ((TextureSizeMode) evt.newValue == TextureSizeMode.Functor)
                {
                    (evt.target as EnumField).value = evt.previousValue;
                    return;
                }
                TextureBufferNode.sizeMode = (TextureSizeMode)evt.newValue;

                sizeRoot.Clear();
                if (TextureBufferNode.sizeMode == TextureSizeMode.Explicit)
                {
                    sizeRoot.Add(width);
                    sizeRoot.Add(height);
                    
                } else if (TextureBufferNode.sizeMode == TextureSizeMode.Scale)
                {
                    sizeRoot.Add(scale);
                }
                
            });
            root.Add(sizeMode);
            root.Add(sizeRoot);
            
            if (TextureBufferNode.sizeMode == TextureSizeMode.Explicit)
            {
                sizeRoot.Add(width);
                sizeRoot.Add(height);
                    
            } else if (TextureBufferNode.sizeMode == TextureSizeMode.Scale)
            {
                sizeRoot.Add(scale);
            }

            var useDynamicScale = FRPElementUtilities.CreateToggle(TextureBufferNode.useDynamicScale,
                "Dynamic Scale",
                evt =>
                {
                    TextureBufferNode.useDynamicScale = evt.newValue;
                });
            root.Add(useDynamicScale);
            
            return root;
        }
        
        private VisualElement InitState()
        {
            var root = new VisualElement();

            var clearBuffer = FRPElementUtilities.CreateToggle(TextureBufferNode.clearBuffer, "Clear", evt =>
            {
                TextureBufferNode.clearBuffer = evt.newValue;
            });
            root.Add(clearBuffer);

            var colorField = FRPElementUtilities.CreateField<ColorField, Color>(TextureBufferNode.clearColor,
                "Clear Color",
                evt =>
                {
                    TextureBufferNode.clearColor = evt.newValue;
                });
            root.Add(colorField);

            return root;
        }
        
        private VisualElement Format()
        {
            var root = new VisualElement();

            var graphicFormat = FRPElementUtilities.CreateEnumField(TextureBufferNode.colorFormat, "Format", evt =>
            {
                TextureBufferNode.colorFormat = (GraphicsFormat)evt.newValue;
            });
            root.Add(graphicFormat);
            
            var depthBits = FRPElementUtilities.CreateEnumField(TextureBufferNode.depthBits, "Depth Bits", evt =>
            {
                TextureBufferNode.depthBits = (DepthBits)evt.newValue;
            });
            root.Add(depthBits);

            var dimension = FRPElementUtilities.CreateEnumField(TextureBufferNode.dimension, "Dimension", evt =>
            {
                TextureBufferNode.dimension = (TextureDimension) evt.newValue;
            });
            root.Add(dimension);
            
            return root;
        }
        
        private VisualElement FilterAddressing()
        {
            var root = new VisualElement();

            var filterMode = FRPElementUtilities.CreateEnumField(TextureBufferNode.filterMode, "Filter Mode", evt =>
            {
                TextureBufferNode.filterMode = (FilterMode) evt.newValue;
            });
            root.Add(filterMode);

            var wrapMode = FRPElementUtilities.CreateEnumField(TextureBufferNode.wrapMode, "Wrap Mode", evt =>
            {
                TextureBufferNode.wrapMode = (TextureWrapMode) evt.newValue;
            });
            root.Add(wrapMode);

            var anisoLevel = FRPElementUtilities.CreateIntegerField(TextureBufferNode.anisoLevel, "Aniso Level", evt =>
            {
                TextureBufferNode.anisoLevel = evt.newValue;
            });
            root.Add(anisoLevel);

            return root;
        }
        
        private VisualElement Mipmap()
        {
            var root = new VisualElement();

            var useMipmap = FRPElementUtilities.CreateToggle(TextureBufferNode.useMipMap, "Mipmap", evt =>
            {
                TextureBufferNode.useMipMap = evt.newValue;
            });
            root.Add(useMipmap);
            
            var autoGenerateMip = FRPElementUtilities.CreateToggle(TextureBufferNode.autoGenerateMips, "Auto Generate Mipmap", evt =>
            {
                TextureBufferNode.autoGenerateMips = evt.newValue;
            });
            root.Add(autoGenerateMip);

            var mipMapBias = FRPElementUtilities.CreateFloatField(TextureBufferNode.mipMapBias, "Mipmap Bias", evt =>
            {
                TextureBufferNode.mipMapBias = evt.newValue;
            });
            root.Add(mipMapBias);
            
            return root;
        }
        
        private VisualElement Memory()
        {
            var root = new VisualElement();

            var enableRandomWrite = FRPElementUtilities.CreateToggle(TextureBufferNode.enableRandomWrite,
                "Random Write",
                evt =>
                {
                    TextureBufferNode.enableRandomWrite = evt.newValue;
                });
            root.Add(enableRandomWrite);

            var memoryless = FRPElementUtilities.CreateMaskField((int) TextureBufferNode.memoryless, "Memoryless",
                new List<string>()
                {
                    "Color", "Depth", "MSAA"
                }, evt =>
                {
                    TextureBufferNode.memoryless = (RenderTextureMemoryless) evt.newValue;
                });
            root.Add(memoryless);

            var fastMemoryRoot = new VisualElement();
            AddFoldout("Fast Memory", fastMemoryRoot, root, 15);
            var inFastMemory = FRPElementUtilities.CreateToggle(TextureBufferNode.fastMemoryDesc.inFastMemory,
                "In Fast Memory",
                evt =>
                {
                    TextureBufferNode.fastMemoryDesc.inFastMemory = evt.newValue;
                });
            fastMemoryRoot.Add(inFastMemory);
            var fastMemoryFlags = FRPElementUtilities.CreateMaskField((int)TextureBufferNode.fastMemoryDesc.flags, "Flags",
                new List<string>()
                {
                    "SpillTop", "SpillBottom"
                }, evt =>
                {
                    TextureBufferNode.fastMemoryDesc.flags = (FastMemoryFlags) evt.newValue;
                });
            fastMemoryRoot.Add(fastMemoryFlags);

            var residencySlider = FRPElementUtilities.CreateSlider(0f, 1f,
                TextureBufferNode.fastMemoryDesc.residencyFraction, SliderDirection.Horizontal, "Residency Fraction",
                evt =>
                {
                    TextureBufferNode.fastMemoryDesc.residencyFraction = evt.newValue;
                });
            fastMemoryRoot.Add(residencySlider);

            var depthAccess = FRPElementUtilities.CreateEnumField(TextureBufferNode.depthAccess, "Depth Access", evt =>
            {
                TextureBufferNode.depthAccess = (DepthAccess)evt.newValue;
            });
            root.Add(depthAccess);
            
            return root;
        }
        
        private VisualElement MSAA()
        {
            var root = new VisualElement();

            var msaa = FRPElementUtilities.CreateEnumField(TextureBufferNode.msaaSamples, "MSAA Samples", evt =>
            {
                TextureBufferNode.msaaSamples = (MSAASamples)evt.newValue;
            });
            root.Add(msaa);

            var bindSample = FRPElementUtilities.CreateToggle(TextureBufferNode.bindTextureMS, "Bind Multip-Sample",
                evt =>
                {
                    TextureBufferNode.bindTextureMS = evt.newValue;
                });
            
            root.Add(bindSample);
            
            return root;
        }
    }
}