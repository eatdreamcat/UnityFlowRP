namespace UnityEngine.Rendering.SuperAdvanced
{

    public partial class SARenderPipeline
    {
        bool NeedClearColorBuffer(SACamera saCamera)
        {
            // if (frpCamera.clearColorMode == HDAdditionalCameraData.ClearColorMode.Color ||
            //     // If the luxmeter is enabled, the sky isn't rendered so we clear the background color
            //     m_CurrentDebugDisplaySettings.data.lightingDebugSettings.debugLightingMode == DebugLightingMode.LuxMeter ||
            //     // If the matcap view is enabled, the sky isn't updated so we clear the background color
            //     m_CurrentDebugDisplaySettings.DebugHideSky(hdCamera) ||
            //     // If we want the sky but the sky don't exist, still clear with background color
            //     (hdCamera.clearColorMode == HDAdditionalCameraData.ClearColorMode.Sky && !m_SkyManager.IsVisualSkyValid(hdCamera)) ||
            //     // Special handling for Preview we force to clear with background color (i.e black)
            //     // Note that the sky use in this case is the last one setup. If there is no scene or game, there is no sky use as reflection in the preview
            //     HDUtils.IsRegularPreviewCamera(hdCamera.camera))
            // {
            //     return true;
            // }

            return false;
        }
        
        Color GetColorBufferClearColor(SACamera saCamera)
        {
            Color clearColor = Color.black;

            // // We set the background color to black when the luxmeter is enabled to avoid picking the sky color
            // if (m_CurrentDebugDisplaySettings.data.lightingDebugSettings.debugLightingMode == DebugLightingMode.LuxMeter ||
            //     m_CurrentDebugDisplaySettings.DebugHideSky(hdCamera))
            //     clearColor = Color.black;
            //
            // if (hdCamera.CameraIsSceneFiltering())
            //     clearColor.a = 0.0f;

            return clearColor;
        }
    }
}