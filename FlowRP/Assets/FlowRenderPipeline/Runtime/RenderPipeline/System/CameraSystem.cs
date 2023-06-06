

using System;
using System.Collections.Generic;

namespace UnityEngine.Rendering.FlowPipeline
{
    
    internal class CameraSystem 
    {
        // Store active passes and avoid allocating memory every frames
        List<Camera> framePasses = new List<Camera>();

        
#if UNITY_2021_1_OR_NEWER
        internal List<Camera> SetupFrameAndSort(List<Camera> cameras)
#else
        internal List<(Camera)> SetupFrame(Camera[] cameras)
#endif
        {
            if (framePasses.Count > 0)
            {
                Debug.LogWarning("ReleaseFrame() was not called!");
                ReleaseFrame();
            }


            foreach (var camera in cameras)
            {
                if (camera == null)
                    continue;
                
                AddPassToFrame(camera);
            }

            SortCameras(framePasses);
            
            return framePasses;
        }
        
        internal void AddPassToFrame(Camera camera)
        {
         
            framePasses.Add(camera);
        }
        
        internal void ReleaseFrame()
        {
            for (int i = 0; i < framePasses.Count; i++)
            {
                // Pop from the back to keep initial ordering (see implementation of ObjectPool)
                Camera _ = framePasses[framePasses.Count - i - 1];

            }

            framePasses.Clear();
        }
        
        
        //// DO cleanup
        ///
        internal void Cleanup()
        {
           
        }

        #region Private

        /**
         * sort camera by depth
         */
        private Comparison<Camera> cameraComparison = ((camera1, camera2) =>
        {
            return (int) camera1.depth - (int) camera2.depth;
        });
        
        void SortCameras(List<Camera> cameras)
        {
            if (cameras.Count > 1)
                cameras.Sort(cameraComparison);
        }


        #endregion
    }
}