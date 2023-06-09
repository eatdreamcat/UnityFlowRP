using UnityEditor;
using UnityEngine.UIElements;

namespace UnityEditor.Rendering.FlowPipeline
{
    public static class FRPViewStyleUtility
    {
        public static VisualElement AddClasses(this VisualElement element, params string[] classNames)
        {
            foreach (string className in classNames)
            {
                element.AddToClassList(className);
            }

            return element;
        }

        public static VisualElement AddStyleSheets(this VisualElement element, params string[] styleSheetNames)
        {
            foreach (string styleSheetName in styleSheetNames)
            {
                StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(FRPPathUtility.kStyleSheetsPath + styleSheetName);

                element.styleSheets.Add(styleSheet);
            }

            return element;
        }
    }

    public static class FRPPathUtility
    {
        public static readonly string kStyleSheetsPath = "Assets/FlowRenderPipeline/Editor/GraphEditor/StyleSheets/";
        public static readonly string kGraphViewDataSavedPath = "Assets/FlowRenderPipeline/Editor/GraphEditor/Data/";
        public static readonly string kIconPath = "Assets/FlowRenderPipeline/Editor/Resources/Icon/";
    }
}