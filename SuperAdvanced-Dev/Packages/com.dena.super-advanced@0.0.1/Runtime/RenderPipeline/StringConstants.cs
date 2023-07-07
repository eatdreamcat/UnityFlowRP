namespace UnityEngine.Rendering.SuperAdvanced
{
    /// <summary>Pass names and shader ids used in HDRP. these names can be used as filters when rendering objects in a custom pass or a DrawRenderers() call.</summary>
    public static class FlowShaderPassNames
    {
        
        
        /// <summary>Legacy Unlit cross pipeline pass name.</summary>
        public static readonly string s_SRPDefaultUnlitStr = "SRPDefaultUnlit";
        /// <summary>Forward pass name.</summary>
        public static readonly string s_ForwardStr = "Forward";
        /// <summary>Forward Only pass name.</summary>
        public static readonly string s_ForwardOnlyStr = "ForwardOnly";
        /// <summary>Decal Mesh Forward Emissive pass name.</summary>
        public static readonly string s_DecalMeshForwardEmissiveStr = "ForwardDecalEmissive";// TODO:DecalSystem.s_MaterialDecalPassNames[(int)DecalSystem.MaterialDecalPass.DecalMeshForwardEmissive];
        
        /*
                   _ooOoo_
                  o8888888o
                  88" . "88
                  (| -_- |)
                  O\  =  /O
               ____/`---'\____
             .'  \\|     |//  `.
            /  \\|||  :  |||//  \
           /  _||||| -:- |||||-  \
           |   | \\\  -  /// |   |
           | \_|  ''\---/''  |   |
           \  .-\__  `-`  ___/-. /
         ___`. .'  /--.--\  `. . __
      ."" '<  `.___\_<|>_/___.'  >'"".
     | | :  `- \`.;`\ _ /`;.`/ - ` : | |
     \  \ `-.   \_ __\ /__ _/   .-` /  /
======`-.____`-.___\_____/___.-`____.-'======
                   `=---='
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            佛祖保佑       永无BUG
            
            Shader Tag ID
*/
        /// <summary>Empty shader tag id.</summary>
        public static readonly ShaderTagId s_ForwardOnlyName = new ShaderTagId(s_ForwardOnlyStr);
        /// <summary>Forward shader tag id.</summary>
        public static readonly ShaderTagId s_ForwardName = new ShaderTagId(s_ForwardStr);
        /// <summary>Legacy Unlit cross pipeline shader tag id.</summary>
        public static readonly ShaderTagId s_SRPDefaultUnlitName = new ShaderTagId(s_SRPDefaultUnlitStr);
        /// <summary>Decal Mesh Forward Emissive shader tag id.</summary>
        public static readonly ShaderTagId s_DecalMeshForwardEmissiveName = new ShaderTagId(s_DecalMeshForwardEmissiveStr);
    }
}