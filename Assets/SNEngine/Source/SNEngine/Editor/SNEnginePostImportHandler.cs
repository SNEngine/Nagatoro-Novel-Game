using UnityEngine;
using UnityEditor;

namespace SNEngine.Editor
{
    /// <summary>
    /// This script runs after assets are imported to check if SNEngine was just imported
    /// and show the welcome window if needed
    /// </summary>
    public class SNEnginePostImportHandler : AssetPostprocessor
    {
        private static bool hasShownWelcomeWindow = false;

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            // Check if we've already shown the window in this session
            if (hasShownWelcomeWindow) return;

            // Check if SNEngine-related assets were imported
            bool snEngineImported = false;
            foreach (string assetPath in importedAssets)
            {
                if (assetPath.Contains("SNEngine") &&
                    (assetPath.EndsWith(".cs") || assetPath.EndsWith(".unitypackage") ||
                     assetPath.Contains("SNEngine/Source/SNEngine")))
                {
                    snEngineImported = true;
                    break;
                }
            }

            if (snEngineImported)
            {
                // Check if this is a fresh installation by verifying that setup hasn't been done yet
                string currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);

                // If SNENGINE_SUPPORT is not in the defines, this is likely a fresh install
                if (!currentDefines.Contains("SNENGINE_SUPPORT"))
                {
                    // Delay showing the window to ensure everything is properly loaded
                    EditorApplication.delayCall += () =>
                    {
                        // Small delay to ensure Unity is fully ready
                        EditorApplication.update += ShowWelcomeWindowOnce;
                    };

                    hasShownWelcomeWindow = true;
                }
            }
        }

        private static void ShowWelcomeWindowOnce()
        {
            // Remove this callback after calling once
            EditorApplication.update -= ShowWelcomeWindowOnce;

            // Only show if we're not in play mode and not during compilation
            if (!EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isCompiling)
            {
                // Show the welcome window
                var window = GetWindow<WelcomeWindow>("SNEngine Welcome");
                window.minSize = new Vector2(500, 400);
                window.ShowUtility();
            }
        }
    }
}