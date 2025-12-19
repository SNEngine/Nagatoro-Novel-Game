using UnityEditor;
using XNodeEditor;
using SNEngine.Graphs;
using SNEngine.Debugging;

namespace SNEngine.Editor
{
    public static class GlobalVaritablesMenu
    {
        [MenuItem("SNEngine/Open Global Varitables Window")]
        public static void OpenGlobalVaritables()
        {
            string path = "Assets/SNEngine/Source/SNEngine/Resources/VaritableContainerGraph.asset";
            VaritableContainerGraph graph = AssetDatabase.LoadAssetAtPath<VaritableContainerGraph>(path);

            if (graph != null)
            {
                NodeEditorWindow.Open(graph);
            }
            else
            {
               NovelGameDebug.LogError($"[SNEngine] Global varitables not found: {path}");
            }
        }
    }
}