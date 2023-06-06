using System;
using UnityEngine;
using UnityEngine.Rendering.FlowPipeline;

namespace UnityEditor.Rendering.FlowPipeline
{
    
    /// <summary>
    /// Holding the serialized camera data
    /// </summary>
    
    public class FlowRenderPipelineSerializedCamera : ISerializedCamera
    {
        public SerializedObject serializedObject { get; }
        public SerializedObject serializedAdditionalDataObject { get; }
        public CameraEditor.Settings baseCameraSettings { get; }
        public SerializedProperty projectionMatrixMode { get; }
        public SerializedProperty dithering { get; }
        public SerializedProperty stopNaNs { get; }
        public SerializedProperty allowDynamicResolution { get; }
        public SerializedProperty volumeLayerMask { get; }
        public SerializedProperty clearDepth { get; }
        public SerializedProperty antialiasing { get; }

        #region Custom Properties

        public SerializedProperty cameras { get; set; }
        public FlowRPAdditionalCameraData[] camerasAdditionalData { get; }
        
        FlowRenderPipelineSerializedCamera[] cameraSerializedObjects { get; set; }
        
        public int numCameras => cameras?.arraySize ?? 0;

        #endregion
        
       
        
        public (Camera camera, FlowRenderPipelineSerializedCamera serializedCamera) this[int index]
        {
            get
            {
                if (index < 0 || index >= numCameras)
                    throw new ArgumentOutOfRangeException($"{index} is out of bounds [0 - {numCameras}]");

                // Return the camera on that index
                return (cameras.GetArrayElementAtIndex(index).objectReferenceValue as Camera, cameraSerializedObjects[index]);
            }
        }
        
       

        public FlowRenderPipelineSerializedCamera(SerializedObject serializedObject,
            CameraEditor.Settings settings = null)
        {
            
            this.serializedObject = serializedObject;
            
            camerasAdditionalData = CoreEditorUtils
                .GetAdditionalData<FlowRPAdditionalCameraData>(serializedObject.targetObjects);
        }

        public void Update()
        {
            // throw new System.NotImplementedException();
        }

        public void Apply()
        {
            throw new System.NotImplementedException();
        }

        public void Refresh()
        {
            throw new System.NotImplementedException();
        }
    }
}