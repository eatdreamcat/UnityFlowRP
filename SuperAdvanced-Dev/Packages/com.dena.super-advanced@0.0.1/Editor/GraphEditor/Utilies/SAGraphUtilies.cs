using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.Rendering.SuperAdvanced
{
    public static class SAViewStyleUtility
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
                var stylePath = SAPathUtility.kStyleSheetsPath + styleSheetName;
                
                StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(stylePath);

                if (styleSheet == null)
                {
                    Debug.LogError($"style sheet is null: {stylePath}");
                    continue;
                }
                
                element.styleSheets.Add(styleSheet);
            }

            return element;
        }
    }

    public static class SAPathUtility
    {
        public static readonly string kPackageName = "com.dena.render-pipelines.super-advanced";
        public static readonly string kStyleSheetsPath = $"Packages/{kPackageName}/Editor/GraphEditor/StyleSheets/";
        public static readonly string kGraphViewDataSavedFullPath =  $"Packages/{kPackageName}/Editor/GraphEditor/Data/SAGraphViewSavedData.asset";
    }

    public static class SAAssetsUtility
    {
        public static void SaveAsset(ScriptableObject so) {
            
            EditorUtility.SetDirty(so);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}