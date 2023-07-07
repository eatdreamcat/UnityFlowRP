using System;
using UnityEngine;
using UnityEngine.Rendering.SuperAdvanced;

namespace UnityEditor.Rendering.SuperAdvanced
{
    
    /// <summary>
    /// Holding the serialized camera data
    /// </summary>
    
    public class SASerializedCamera : ISerializedCamera
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
        public SAAdditionalCameraData[] camerasAdditionalData { get; }
        
        SASerializedCamera[] cameraSerializedObjects { get; set; }
        
        public int numCameras => cameras?.arraySize ?? 0;

        #endregion
        
       
        
        public (Camera camera, SASerializedCamera serializedCamera) this[int index]
        {
            get
            {
                if (index < 0 || index >= numCameras)
                    throw new ArgumentOutOfRangeException($"{index} is out of bounds [0 - {numCameras}]");

                // Return the camera on that index
                return (cameras.GetArrayElementAtIndex(index).objectReferenceValue as Camera, cameraSerializedObjects[index]);
            }
        }
        
       

        public SASerializedCamera(SerializedObject serializedObject,
            CameraEditor.Settings settings = null)
        {
            
            this.serializedObject = serializedObject;
            
            camerasAdditionalData = CoreEditorUtils
                .GetAdditionalData<SAAdditionalCameraData>(serializedObject.targetObjects);
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