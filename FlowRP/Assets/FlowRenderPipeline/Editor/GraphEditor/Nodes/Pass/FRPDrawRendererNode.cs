using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering.FlowPipeline;
using UnityEngine.UIElements;

namespace UnityEditor.Rendering.FlowPipeline
{
    public class FRPDrawRendererNode : FRPNodeBase
    {

        /**
         *   * flow-in      flow-out*                        mainContainer
         *    ------------------------                        titleContainer
         *    Culling                                        inputContainer  -  outputContainer
         *     - PassCulling     |_|                           
         *     - RendererCulling |_| 
         *     - CullingMask     |_|
         *     - Probe...
         *    ------------------------
         *    State
         *     - RasterState
         *     - DepthStencilState
         *     - BlendOpt
         *    ------------------------
         *    Material
         *     -Queue
         *     -ShaderTagList
         *     -OverrideMat
         *    -------------------------
         *    Camera
         *     -FOV
         *     -Offset
         *    -------------------------
         *    Input      |       Output
         *      -Slot0   |     -Slot0      
         *      -Slot1   |     -Slot1
         *   -------------------------
         *    Custom Parameters
         *
         *   -------------------------
         */
        struct FoldoutBlock
        {
            public VisualElement block;
            public Foldout foldout;
            public Port assignIn; // maybe null 
        }

        public static readonly string kCullingFoldoutName = "Culling";
        public static readonly string kStateFoldoutName = "State";
        public static readonly string kMaterialFoldoutName = "Material";
        public static readonly string kCameraFoldoutName = "Camera";
        
        public static readonly string kInputOutputBlockName = "Input-Output";
        public static readonly string kParametersBlockName = "Parameters";

        public struct FoldoutDisplay
        {
            public string title;
            public string suffix;
            public bool needReferencePort;
            public Type portContraintedType;
            public float indentValue;
        }
                                   /// <summary>
                                   /// foldout - need reference port
                                   /// </summary>
        private static readonly List<FoldoutDisplay> FoldoutList = new List<FoldoutDisplay>()
        {
            new FoldoutDisplay()
            {
                title = kCullingFoldoutName,
                suffix = " Preview",
                needReferencePort = true,
                portContraintedType = typeof(FRPCullingParameterNode),
                indentValue = Indent
            },
           
            new FoldoutDisplay()
            {
                title = kStateFoldoutName,
                suffix = " Preview",
                needReferencePort = true,
                portContraintedType = typeof(FRPRenderStateNode),
                indentValue = Indent
            },
            
            new FoldoutDisplay()
            {
                title = kMaterialFoldoutName,
                suffix = " Preview",
                needReferencePort = true,
                portContraintedType = typeof(FRPRenderMaterialNode),
                indentValue = Indent
            },
          
            new FoldoutDisplay()
            {
                title = kCameraFoldoutName,
                suffix = " Preview",
                needReferencePort = true,
                portContraintedType = typeof(FRPCameraParameterNode),
                indentValue = Indent
            },
        };
            
        private Dictionary<string, FoldoutBlock> m_FoldoutBlocks = new Dictionary<string, FoldoutBlock>();

        private VisualElement m_ContentContainer = new VisualElement();
        private VisualElement m_RenderConfigRoot = new VisualElement();
        // private Foldout m_ConfigFoldout = new Foldout()
        // {
        //     value = true,
        //     text = "Settings"
        // };

        private static readonly float Indent = 10;
        public FlowRenderGraphData.DrawRendererNode RenderRequest => (FlowRenderGraphData.DrawRendererNode)m_GraphNodeData;
      
        public override void Initialize(FRPGraphView view, Vector2 position, FlowRenderGraphData.BaseNode nodeData)
        {
            base.Initialize(view, position, nodeData);

            // initialize blocks
            
            m_ContentContainer.Add(m_RenderConfigRoot);
            // mainContainer.Add(m_ConfigFoldout);
            mainContainer.Add(m_ContentContainer);
            m_ContentContainer.style.left = Indent;
          
            
            foreach (var foldoutInfo in FoldoutList)
            {
                var block = new VisualElement();
                var foldout = new Foldout()
                {
                    value = false,
                    text = foldoutInfo.title + foldoutInfo.suffix
                };

                Port port = null;
                if (foldoutInfo.needReferencePort)
                {
                    port = this.CreatePort(foldoutInfo.title, Orientation.Horizontal, Direction.Input, Port.Capacity.Single,
                        foldoutInfo.portContraintedType);
                    m_RenderConfigRoot.Add(port);
                }

                m_FoldoutBlocks.Add(foldoutInfo.title, new FoldoutBlock()
                {
                    block = block,
                    foldout = foldout,
                    assignIn = port
                });


                foldout.style.left = Indent;
                m_RenderConfigRoot.Add(foldout);
                m_RenderConfigRoot.Add(block);
            }
            
            // culling 
            m_FoldoutBlocks[kCullingFoldoutName].foldout.RegisterValueChangedCallback(DrawCullingBlock);
            // State
            m_FoldoutBlocks[kStateFoldoutName].foldout.RegisterValueChangedCallback(DrawStateBlock);
            // Material
            m_FoldoutBlocks[kMaterialFoldoutName].foldout.RegisterValueChangedCallback(DrawMaterialBlock);
            // Camera
            m_FoldoutBlocks[kCameraFoldoutName].foldout.RegisterValueChangedCallback(DrawCameraBlock);
          
            
        }

        public Port GetBlockPort(string blockName)
        {
            if (m_FoldoutBlocks.TryGetValue(blockName, out var block))
            {
                return block.assignIn;
            }

            Debug.LogError($"Cant find a proper port named by {blockName}");
            return null;
        }
        
        #region Culling
        private void DrawCullingBlock(ChangeEvent<bool> evt)
        {
            var isExpended = evt.newValue;
            var foldoutBlock = m_FoldoutBlocks[kCullingFoldoutName];
            var cullingContentRoot = foldoutBlock.block;
            cullingContentRoot.Clear();
            if (isExpended)
            {
                if (foldoutBlock.assignIn != null && !string.IsNullOrEmpty(RenderRequest.culling))
                {
                    var cullingParameter = m_View.GraphData.TryGetCullingParameterNode(RenderRequest.culling);
                    
                    Debug.Assert(cullingParameter != null, $"Culling Parameter is null: {RenderRequest.culling}");
                    
                    var previewElement = FRPElementUtilities.CreateCullingParameter(
                        cullingParameter.isAllowPassCulling, null,
                        cullingParameter.isAllowRendererCulling, null,
                        cullingParameter.cullingMask, null,
                        true);
                    previewElement.style.left = Indent * 2;
                    cullingContentRoot.Add(previewElement);
                }
            }
        }


        #endregion

        #region State
        private void DrawStateBlock(ChangeEvent<bool> evt)
        {
            var isExpended = evt.newValue;
            var foldoutBlock = m_FoldoutBlocks[kStateFoldoutName];    
            foldoutBlock.block.Clear();
            if (isExpended)
            {
                if (foldoutBlock.assignIn != null && !string.IsNullOrEmpty(RenderRequest.state))
                {
                    var stateParameter = m_View.GraphData.TryGetRenderStateNode(RenderRequest.state);
                    Debug.Assert(stateParameter != null, 
                        $"RenderState Parameter is null: {RenderRequest.state}");
                    var previewElement = FRPElementUtilities.CreateStateParameter(stateParameter, true);
                    previewElement.style.left = Indent * 2;
                    foldoutBlock.block.Add(previewElement);
                }
            }
        }

        #endregion

        #region Material
        private void DrawMaterialBlock(ChangeEvent<bool> evt)
        {
            var isExpended = evt.newValue;
            var foldoutBlock = m_FoldoutBlocks[kMaterialFoldoutName];    
            foldoutBlock.block.Clear();
            if (isExpended)
            {
                if (foldoutBlock.assignIn != null && !string.IsNullOrEmpty(RenderRequest.material))
                {
                    var materialParameter = m_View.GraphData.TryGetMaterialParameterNode(RenderRequest.material);
                    
                    Debug.Assert(materialParameter != null, 
                        $"Material Parameter is null: {RenderRequest.material}");
                    
                    var previewElement = FRPElementUtilities.CreateMaterialParameter(materialParameter, 
                        null, null, true);
                    previewElement.style.left = Indent * 2;
                    foldoutBlock.block.Add(previewElement);
                }
            }
        }

        #endregion
        
        #region Camera

        private void DrawCameraBlock(ChangeEvent<bool> evt)
        {
            var isExpended = evt.newValue;
            var foldoutBlock = m_FoldoutBlocks[kCameraFoldoutName];
            var cameraContentRoot = foldoutBlock.block;
            cameraContentRoot.Clear();
            if (isExpended)
            {
                if (foldoutBlock.assignIn != null && !string.IsNullOrEmpty(RenderRequest.camera))
                {
                    var cameraParameter = m_View.GraphData.TryGetCameraParameterNode(RenderRequest.camera);
                    
                    Debug.Assert(cameraParameter != null, 
                        $"Camera Parameter is null: {RenderRequest.camera}");

                    var previewElement = FRPElementUtilities.CreateCameraParameter(cameraParameter.fov, null,
                        cameraParameter.offset, null, true);
                    previewElement.style.left = Indent * 2;
                    cameraContentRoot.Add(previewElement);
                   
                }
            }
        }

        #endregion

        #region Input-Output

        private void DrawInputOutputBlock(ChangeEvent<bool> evt)
        {
            var isExpended = evt.newValue;
        }

        #endregion

        #region Custom Parmeters

        private void DrawCustomParametersBlock(ChangeEvent<bool> evt)
        {
            var isExpended = evt.newValue;
        }

        #endregion
        
        public override void Draw(bool refresh = true)
        {
            base.Draw(false);
            
            RefreshExpandedState();
        }
    }
}