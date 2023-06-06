using System.Collections.Generic;

namespace UnityEngine.Rendering.FlowPipeline
{
    public class FRPCamera
    {
        
        /// <summary>Camera component.</summary>
        public Camera camera;

        public string name;
        
        
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
            RTHandles.SetReferenceSize(1920, 1080);
        }
    }
}