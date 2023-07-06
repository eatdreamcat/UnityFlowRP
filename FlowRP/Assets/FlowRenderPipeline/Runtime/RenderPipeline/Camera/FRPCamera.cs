using System;
using System.Collections.Generic;

namespace UnityEngine.Rendering.FlowPipeline
{
    public class FRPCamera
    {
        
        /// <summary>Camera component.</summary>
        public Camera camera;

        public string name;
        
        internal Rect finalViewport = new Rect(Vector2.zero, -1.0f * Vector2.one); // This will have the correct viewport position and the size will be full resolution (ie : not taking dynamic rez into account)
        
        /// <summary>Width actually used for rendering after dynamic resolution and XR is applied.</summary>
        public int actualWidth { get; private set; }
        /// <summary>Height actually used for rendering after dynamic resolution and XR is applied.</summary>
        public int actualHeight { get; private set; }
        
        FlowRPAdditionalCameraData m_AdditionalCameraData = null; // Init in Update
        internal ProfilingSampler profilingSampler => ProfilingSampler.Get(ProfileId.FlowRenderPipelineRenderCamera);
        

        static Dictionary<(Camera, int), FRPCamera> s_Cameras = new Dictionary<(Camera, int), FRPCamera>();
        public static FRPCamera GetOrCreate(Camera camera, int xrMultipassId = 0)
        {
            FRPCamera frpCamera;
            if (!s_Cameras.TryGetValue((camera, xrMultipassId), out frpCamera))
            {
                frpCamera = new FRPCamera(camera);
                s_Cameras.Add((camera, xrMultipassId), frpCamera);
            }

            return frpCamera;
        }

        internal FRPCamera(Camera cam)
        {
            camera = cam;

            name = cam.name;
        }

        // Updating RTHandle needs to be done at the beginning of rendering (not during update of HDCamera which happens in batches)
        // The reason is that RTHandle will hold data necessary to setup RenderTargets and viewports properly.
        internal void BeginRender(CommandBuffer cmd)
        {
            RTHandles.SetReferenceSize(actualWidth, actualHeight);
        }

        Rect GetPixelRect()
        {
            return new Rect(camera.pixelRect.x, camera.pixelRect.y, camera.pixelWidth, camera.pixelHeight);
        }
        internal void Update(FlowRenderPipeline flowRenderPipeline,
            bool allocateHistoryBuffers = true)
        {
            // store a shortcut on HDAdditionalCameraData (done here and not in the constructor as
            // we don't create HDCamera at every frame and user can change the HDAdditionalData later (Like when they create a new scene).
            camera.TryGetComponent<FlowRPAdditionalCameraData>(out m_AdditionalCameraData);

            // set viewport
            {
                finalViewport = GetPixelRect();
                actualWidth = Math.Max((int)finalViewport.size.x, 1);
                actualHeight = Math.Max((int)finalViewport.size.y, 1);
            }
            
            DynamicResolutionHandler.instance.finalViewport = new Vector2Int((int)finalViewport.width, (int)finalViewport.height);
        }
    }
}