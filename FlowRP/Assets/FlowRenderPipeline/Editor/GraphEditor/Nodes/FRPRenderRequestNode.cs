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
    public class FRPRenderRequestNode : FRPNodeBase
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

        private static readonly string kCullingFoldoutName = "Culling";
        private static readonly string kStateFoldoutName = "State";
        private static readonly string kMaterialFoldoutName = "Material";
        private static readonly string kCameraFoldoutName = "Camera";
        private static readonly string kInputOutputFoldoutName = "Input-Output";
        private static readonly string kParametersFoldoutName = "Parameters";
                                   /// <summary>
                                   /// foldout - need reference port
                                   /// </summary>
        private static readonly List<(string, bool)> FoldoutList = new List<(string, bool)>()
        {
            (kCullingFoldoutName, true),
            (kStateFoldoutName, true),
            (kMaterialFoldoutName, true),
            (kCameraFoldoutName, true),
            (kInputOutputFoldoutName, false),
            (kParametersFoldoutName, false)
        };
            
        private Dictionary<string, FoldoutBlock> m_FoldoutBlocks = new Dictionary<string, FoldoutBlock>();
      
        public override void Initialize(string name, FRPGraphView view, Vector2 position,
            FlowRenderGraphData.FRPNodeType type, string guid)
        {
            base.Initialize(name, view, position, type, guid);

            // initialize blocks
            
            foreach (var foldoutInfo in FoldoutList)
            {
                var block = new VisualElement();
                var foldout = new Foldout()
                {
                    value = false,
                    text = foldoutInfo.Item1
                };
                m_FoldoutBlocks.Add(foldoutInfo.Item1, new FoldoutBlock()
                {
                    block = block,
                    foldout = foldout,
                    assignIn = foldoutInfo.Item2 ? this.CreatePort("Assign-In", Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(FRPBranchNode)) : null
                });
                
                mainContainer.Add(foldout);
                mainContainer.Add(block);
            }
            
            // culling 
            m_FoldoutBlocks[kCullingFoldoutName].foldout.RegisterValueChangedCallback(DrawCullingBlock);
            // State
            m_FoldoutBlocks[kStateFoldoutName].foldout.RegisterValueChangedCallback(DrawStateBlock);
            // Material
            m_FoldoutBlocks[kMaterialFoldoutName].foldout.RegisterValueChangedCallback(DrawMaterialBlock);
            // Camera
            m_FoldoutBlocks[kCameraFoldoutName].foldout.RegisterValueChangedCallback(DrawCameraBlock);
            // Input-Output
            m_FoldoutBlocks[kInputOutputFoldoutName].foldout.RegisterValueChangedCallback(DrawInputOutputBlock);
            // Parameters
            m_FoldoutBlocks[kParametersFoldoutName].foldout.RegisterValueChangedCallback(DrawCustomParametersBlock);
        }

        #region Culling
        private void DrawCullingBlock(ChangeEvent<bool> evt)
        {
            var isExpended = evt.newValue;
            var foldoutBlock = m_FoldoutBlocks[kCullingFoldoutName];    
            foldoutBlock.block.Clear();
            if (isExpended)
            {
                if (foldoutBlock.assignIn != null)
                {
                    foldoutBlock.block.Add(foldoutBlock.assignIn);
                }
            }
        }


        #endregion

        #region State
        private void DrawStateBlock(ChangeEvent<bool> evt)
        {
            var isExpended = evt.newValue;
        }

        #endregion

        #region Material
        private void DrawMaterialBlock(ChangeEvent<bool> evt)
        {
            var isExpended = evt.newValue;
        }

        #endregion
        
        #region Camera

        private void DrawCameraBlock(ChangeEvent<bool> evt)
        {
            var isExpended = evt.newValue;
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