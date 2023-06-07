using UnityEditor;
using UnityEngine.UIElements;

namespace UnityEditor.Rendering.FlowPipeline
{
    public static class DSStyleUtility
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
                StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/FlowRenderPipeline/Editor/GraphEditor/StyleSheets/" + styleSheetName);

                element.styleSheets.Add(styleSheet);
            }

            return element;
        }
    }
}