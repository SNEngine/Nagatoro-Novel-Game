using UnityEditor;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using SNEngine.Debugging;
namespace SNEngine.Editor
{
    public abstract class BaseToolLauncher
    {
        protected static void LaunchExecutable(string toolFolderName, string windowsExeName, string linuxExeName)
        {
            string projectPath = Application.dataPath;
            string editorFolder = Directory.GetParent(projectPath).FullName;

            string basePath = $"Assets/SNEngine/Source/SNEngine/Editor/Utils/{toolFolderName}";

            string platformFolder = Application.platform == RuntimePlatform.WindowsEditor ? "Windows" : "Linux";
            string exeName = Application.platform == RuntimePlatform.WindowsEditor ? windowsExeName : linuxExeName;

            string relativePath = Path.Combine(basePath, platformFolder, exeName);
            string fullPath = Path.Combine(editorFolder, relativePath).Replace('/', Path.DirectorySeparatorChar);

            if (!File.Exists(fullPath))
            {
                NovelGameDebug.LogError($"[Launcher] Error: Executable not found at: {fullPath}");
                EditorUtility.DisplayDialog("Launch Error", $"File not found: {relativePath}", "OK");
                return;
            }

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(fullPath)
                {
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(fullPath)
                };

                Process.Start(startInfo);
                NovelGameDebug.Log($"[Launcher] Started: {fullPath}");
            }
            catch (System.Exception e)
            {
                NovelGameDebug.LogError($"[Launcher] Failed to start {toolFolderName}: {e.Message}");
            }
        }
    }
}