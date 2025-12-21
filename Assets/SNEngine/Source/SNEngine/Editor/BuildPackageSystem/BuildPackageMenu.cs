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
        private const string DIALOGUES_PATH = "Assets/SNEngine/Source/SNEngine/Resources/Dialogues";
        private const string TEMPLATE_PATH = "Assets/SNEngine/Source/SNEngine/Editor/Templates/DialogueTemplate.asset";
        private const string PYTHON_SCRIPT_REL_PATH = "Assets/SNEngine/Source/SNEngine/Editor/Python/build_cleanup.py";
        private const string START_DIALOGUE_NAME = "_startDialogue.asset";

        [MenuItem(MENU_PATH)]
        public static async void BuildPackage()
        {
            if (IsOnMasterBranch())
            {
                EditorUtility.DisplayDialog("Build Package", "Switch branch from master first!", "OK");
                return;
            }

            string exportPath = EditorUtility.OpenFolderPanel("Save unitypackage", "", "");
            if (string.IsNullOrEmpty(exportPath)) return;
            string packagePath = Path.Combine(exportPath, "SNEngine.unitypackage");

            try
            {
                string gitState = GetGitState();

                EditorUtility.DisplayProgressBar("Building", "Running Python Cleanup...", 0.2f);
                await RunPythonCleanup();

                // После работы внешнего скрипта обязательно обновляем базу
                AssetDatabase.Refresh();
                await WaitUntilReady();

                EditorUtility.DisplayProgressBar("Building", "Creating Start Dialogue...", 0.4f);
                CreateStartDialogue();

                // Финальный Refresh перед экспортом
                AssetDatabase.Refresh();
                await WaitUntilReady();

                EditorUtility.DisplayProgressBar("Building", "Exporting Package...", 0.6f);
                ExportWorker.ExportPackage(packagePath);

                // Ожидаем физического завершения записи файла
                while (!File.Exists(packagePath)) await Task.Delay(500);
                await Task.Delay(1000);

                EditorUtility.DisplayProgressBar("Building", "Restoring Git State...", 0.8f);
                RestoreGitState(gitState);

                Debug.Log($"<color=green>[BuildPackage]</color> Done! Package: {packagePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Build failed: {e.Message}");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static async Task RunPythonCleanup()
        {
            string scriptPath = Path.Combine(GetProjectRoot(), PYTHON_SCRIPT_REL_PATH);

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"\"{scriptPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = GetProjectRoot()
            };

            using (Process process = Process.Start(startInfo))
            {
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();

                process.WaitForExit();

                if (!string.IsNullOrEmpty(output)) Debug.Log($"[Python]: {output}");
                if (!string.IsNullOrEmpty(error)) Debug.LogError($"[Python Error]: {error}");
            }
        }

        private static void CreateStartDialogue()
        {
            string dest = Path.Combine(DIALOGUES_PATH, START_DIALOGUE_NAME);
            if (File.Exists(TEMPLATE_PATH))
            {
                if (!Directory.Exists(DIALOGUES_PATH)) Directory.CreateDirectory(DIALOGUES_PATH);
                File.Copy(TEMPLATE_PATH, dest, true);
                AssetDatabase.ImportAsset(dest, ImportAssetOptions.ForceUpdate);
            }
            else
            {
                Debug.LogError($"Template not found at {TEMPLATE_PATH}");
            }
        }

        private static async Task WaitUntilReady()
        {
            while (EditorApplication.isUpdating || EditorApplication.isCompiling)
            {
                await Task.Delay(200);
            }
        }

        private static string GetProjectRoot() => Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

        private static bool IsOnMasterBranch()
        {
            ProcessStartInfo si = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = "-Command \"git branch --show-current\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WorkingDirectory = GetProjectRoot()
            };
            using (Process p = Process.Start(si))
            {
                return p.StandardOutput.ReadToEnd().Trim().Equals("master", StringComparison.OrdinalIgnoreCase);
            }
        }

        private static string GetGitState()
        {
            ProcessStartInfo si = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = "-Command \"git status --porcelain\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WorkingDirectory = GetProjectRoot()
            };
            using (Process p = Process.Start(si)) { return p.StandardOutput.ReadToEnd().Trim(); }
        }

        private static void RestoreGitState(string state)
        {
            ProcessStartInfo si = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = "-Command \"git checkout .; git clean -fd\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = GetProjectRoot()
            };
            using (Process p = Process.Start(si)) { p.WaitForExit(); }
            AssetDatabase.Refresh();
        }
    }
}