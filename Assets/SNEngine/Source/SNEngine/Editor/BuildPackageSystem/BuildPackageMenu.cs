using System;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace SNEngine.Editor.BuildPackageSystem
{
    public static class BuildPackageMenu
    {
        private const string MENU_PATH = "SNEngine/Build Packages";
        private const string CUSTOM_RESOURCES_PATH = "Assets/SNEngine/Source/SNEngine/Resources/Custom";
        private const string CHARACTERS_PATH = "Assets/SNEngine/Source/SNEngine/Resources/Characters";
        private const string DEMO_PATH = "Assets/SNEngine/Demo";
        private const string STREAMING_ASSETS_PATH = "Assets/StreamingAssets";
        private const string DIALOGUES_PATH = "Assets/SNEngine/Source/SNEngine/Resources/Dialogues";
        private const string TEMPLATE_PATH = "Assets/SNEngine/Source/SNEngine/Editor/Templates/DialogueTemplate.asset";
        private const string START_DIALOGUE_NAME = "_startDialogue.asset";

        [MenuItem(MENU_PATH)]
        public static async void BuildPackage()
        {
            if (IsOnMasterBranch())
            {
                EditorUtility.DisplayDialog("Build Package",
                    "You are currently on the master branch. Please switch to a different branch before building the package.",
                    "OK");
                return;
            }

            string exportPath = EditorUtility.OpenFolderPanel("Select folder to save unitypackage", "", "");
            if (string.IsNullOrEmpty(exportPath)) return;

            string packagePath = Path.Combine(exportPath, "SNEngine.unitypackage");

            try
            {
                string gitState = GetGitState();

                EditorUtility.DisplayProgressBar("Building Package", "Preparing assets...", 0.1f);
                AssetDatabase.SaveAssets();

                await DeleteAssetSafeAsync(CUSTOM_RESOURCES_PATH);
                await DeleteAssetSafeAsync(STREAMING_ASSETS_PATH);
                await DeleteAssetSafeAsync(DEMO_PATH);

                await ClearFolderAsync(CHARACTERS_PATH);
                await ClearFolderAsync(DIALOGUES_PATH);

                await CreateStartDialogueAsync();

                EditorUtility.DisplayProgressBar("Building Package", "Refreshing Database...", 0.4f);
                AssetDatabase.Refresh();
                await WaitUntilReady();

                EditorUtility.DisplayProgressBar("Building Package", "Exporting package...", 0.6f);
                ExportWorker.ExportPackage(packagePath);

                while (!File.Exists(packagePath))
                {
                    await Task.Delay(500);
                }

                EditorUtility.DisplayProgressBar("Building Package", "Restoring Git state...", 0.8f);
                RestoreGitState(gitState);

                Debug.Log($"<color=green>[BuildPackage]</color> Completed: {packagePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error during build: {e.Message}");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static async Task DeleteAssetSafeAsync(string path)
        {
            if (AssetDatabase.IsValidFolder(path) || File.Exists(path))
            {
                AssetDatabase.DeleteAsset(path);
                await WaitUntilReady();
            }
        }

        private static async Task ClearFolderAsync(string folderPath)
        {
            if (!AssetDatabase.IsValidFolder(folderPath)) return;

            string fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", folderPath));
            if (!Directory.Exists(fullPath)) return;

            string[] files = Directory.GetFiles(fullPath);
            foreach (string file in files)
            {
                if (file.EndsWith(".meta")) continue;
                string assetPath = folderPath + "/" + Path.GetFileName(file);
                AssetDatabase.DeleteAsset(assetPath);
            }

            string[] subFolders = AssetDatabase.GetSubFolders(folderPath);
            foreach (string subFolder in subFolders)
            {
                AssetDatabase.DeleteAsset(subFolder);
            }

            await WaitUntilReady();
        }

        private static async Task CreateStartDialogueAsync()
        {
            string startDialoguePath = Path.Combine(DIALOGUES_PATH, START_DIALOGUE_NAME);
            if (!File.Exists(TEMPLATE_PATH)) return;

            File.Copy(TEMPLATE_PATH, startDialoguePath, true);
            AssetDatabase.ImportAsset(startDialoguePath, ImportAssetOptions.ForceUpdate);
            await WaitUntilReady();
        }

        private static async Task WaitUntilReady()
        {
            while (EditorApplication.isUpdating || EditorApplication.isCompiling)
            {
                await Task.Delay(100);
            }
        }

        private static string GetProjectRoot() => Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

        private static bool IsOnMasterBranch()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = "-Command \"git branch --show-current\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WorkingDirectory = GetProjectRoot()
            };
            using (Process process = Process.Start(startInfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return output.Trim().Equals("master", StringComparison.OrdinalIgnoreCase);
            }
        }

        private static string GetGitState()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = "-Command \"git status --porcelain\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WorkingDirectory = GetProjectRoot()
            };
            using (Process process = Process.Start(startInfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return output.Trim();
            }
        }

        private static void RestoreGitState(string state)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = "-Command \"git checkout .; git clean -fd\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = GetProjectRoot()
            };
            using (Process process = Process.Start(startInfo)) { process.WaitForExit(); }
            AssetDatabase.Refresh();
        }
    }
}