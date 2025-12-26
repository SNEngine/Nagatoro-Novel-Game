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
        private const string MENU_1 = "SNEngine/Package/1. Run C++ Cleanup";
        private const string MENU_2 = "SNEngine/Package/2. Create Blank Dialogue";
        private const string MENU_3 = "SNEngine/Package/3. Create Blank Character";
        private const string MENU_4 = "SNEngine/Package/4. Build Package";
        private const string MENU_5 = "SNEngine/Package/5. Restore Project (Git)";

        private const string CLEANER_EXE_REL_PATH = "Assets/SNEngine/Source/SNEngine/Editor/Utils/SNEngine_Cleaner.exe";

        [MenuItem(MENU_1)]
        public static async void Step1_Cleanup()
        {
            Debug.Log("[SNEngine] Step 1 Started");

            bool isBranchValid = ValidateBranch();
            Debug.Log("[SNEngine] Branch validation result: " + isBranchValid);

            if (!isBranchValid) return;

            if (!EditorUtility.DisplayDialog("Step 1", "Clean project using C++ utility?", "Yes", "Cancel"))
            {
                Debug.Log("[SNEngine] Cleanup cancelled by user");
                return;
            }

            try
            {
                AssetDatabase.StartAssetEditing();
                await RunCppCleanup();
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
                Debug.Log("<color=cyan>[Package]</color> C++ Cleanup complete.");
            }
            catch (Exception e)
            {
                AssetDatabase.StopAssetEditing();
                Debug.LogError("[SNEngine] Critical failure: " + e.Message);
            }
        }

        private static async Task RunCppCleanup()
        {
            string root = GetProjectRoot();
            string fullPath = Path.GetFullPath(Path.Combine(root, CLEANER_EXE_REL_PATH));
            string workDir = Path.GetDirectoryName(fullPath);

            Debug.Log("[SNEngine] EXE Path: " + fullPath);
            Debug.Log("[SNEngine] Project Root: " + root);

            if (!File.Exists(fullPath))
            {
                Debug.LogError("[SNEngine] EXE NOT FOUND at: " + fullPath);
                return;
            }

            ProcessStartInfo si = new ProcessStartInfo
            {
                FileName = fullPath,
                Arguments = "\"" + root + "\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                WorkingDirectory = workDir
            };

            using (Process p = Process.Start(si))
            {
                if (p == null) throw new Exception("Process failed to start");
                await Task.Run(() => p.WaitForExit());
                Debug.Log("[SNEngine] Process Exit Code: " + p.ExitCode);
            }
        }

        [MenuItem(MENU_2)] public static void Step2() => AssetDatabase.Refresh();
        [MenuItem(MENU_3)] public static void Step3() => Debug.Log("Step 3");

        [MenuItem(MENU_4)]
        public static void Step4_Build()
        {
            if (!ValidateBranch()) return;
            string exportPath = EditorUtility.OpenFolderPanel("Save Package", "", "");
            if (string.IsNullOrEmpty(exportPath)) return;

            string packagePath = Path.Combine(exportPath, "SNEngine.unitypackage");
            string[] assets = { "Assets/SNEngine", "Assets/WebGLTemplates" };
            AssetDatabase.ExportPackage(assets, packagePath, ExportPackageOptions.Recurse);
            Debug.Log("<color=green>[Package]</color> Exported.");
        }

        [MenuItem(MENU_5)]
        public static void Step5_Restore()
        {
            if (!ValidateBranch()) return;
            RestoreGitState();
        }

        private static bool ValidateBranch()
        {
            bool isMaster = IsOnMasterBranch();
            if (isMaster)
            {
                EditorUtility.DisplayDialog("Blocked", "Master branch blocked. Switch to a feature branch.", "OK");
                return false;
            }
            return true;
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
            try
            {
                using (Process p = Process.Start(si))
                {
                    string branch = p.StandardOutput.ReadToEnd().Trim();
                    Debug.Log("[SNEngine] Current branch: " + branch);
                    return branch.Equals("master", StringComparison.OrdinalIgnoreCase) || branch.Equals("main", StringComparison.OrdinalIgnoreCase);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[SNEngine] Git check failed: " + e.Message);
                return false;
            }
        }

        private static void RestoreGitState()
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