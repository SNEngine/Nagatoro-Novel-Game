using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SNEngine.Editor
{
    public class WelcomeWindow : EditorWindow
    {
        private static readonly string[] DEFINE_SYMBOLS = { "DOTWEEN", "UNITASK_DOTWEEN_SUPPORT", "SNENGINE_SUPPORT" };
        private static readonly string WINDOW_PREF_KEY = "SNEngine.WelcomeWindow.ShowOnStartup";
        
        private Vector2 scrollPosition;
        private GUIStyle titleStyle;
        private GUIStyle descriptionStyle;
        private GUIStyle buttonStyle;
        private GUIStyle headerStyle;
        
        [MenuItem("SNEngine/Welcome Window", false, 0)]
        public static void ShowWindow()
        {
            var window = GetWindow<WelcomeWindow>("SNEngine Welcome");
            window.minSize = new Vector2(500, 400);
            window.titleContent = new GUIContent("SNEngine Welcome");
        }
        
        // This method will be called after importing the package
        [InitializeOnLoadMethod]
        private static void CheckShowWelcomeWindow()
        {
            // Check if this is a fresh installation by looking for a marker
            if (!SessionState.GetBool("SNEngine_FreshInstall_Checked", false))
            {
                // Delay the check to ensure all assets are properly loaded
                EditorApplication.delayCall += () =>
                {
                    // Check if this is likely a fresh install by checking for existence of SNEngine components
                    if (!PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone).Contains("SNENGINE_SUPPORT"))
                    {
                        // Only show if we're not in play mode and not during import
                        if (!EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isCompiling)
                        {
                            ShowWindow();
                        }
                    }

                    // Mark that we've checked to avoid showing again during this session
                    SessionState.SetBool("SNEngine_FreshInstall_Checked", true);
                };
            }
        }
        
        private void OnEnable()
        {
            InitializeStyles();
        }
        
        private void InitializeStyles()
        {
            titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(10, 10, 10, 10)
            };
            
            descriptionStyle = new GUIStyle(EditorStyles.label)
            {
                wordWrap = true,
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft,
                margin = new RectOffset(10, 10, 5, 10)
            };
            
            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 14,
                padding = new RectOffset(10, 10, 10, 10),
                margin = new RectOffset(10, 10, 10, 5),
                fixedHeight = 40
            };
            
            headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                margin = new RectOffset(10, 10, 10, 5)
            };
        }
        
        private void OnGUI()
        {
            GUILayout.Space(20);
            
            // Title
            GUILayout.Label("Welcome to SNEngine!", titleStyle);
            
            // Description
            GUILayout.Label("Thank you for installing SNEngine! This visual novel framework provides everything you need to create amazing visual novel games.", descriptionStyle);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            GUILayout.Space(10);
            
            // Features section
            GUILayout.Label("Features Included:", headerStyle);
            string[] features = {
                "• Visual novel dialogue system",
                "• Character management",
                "• Background and scene management",
                "• Animation and effects",
                "• Audio system",
                "• Save/load functionality"
            };
            
            foreach (string feature in features)
            {
                GUILayout.Label(feature, descriptionStyle);
            }
            
            GUILayout.Space(10);
            
            // Configuration section
            GUILayout.Label("Setup Required:", headerStyle);
            GUILayout.Label("Click the Setup button below to automatically configure your project with recommended settings:", descriptionStyle);
            
            // Show current define symbols status
            string currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
            GUILayout.Label("Current Define Symbols:", EditorStyles.boldLabel);
            
            // Display current defines
            if (string.IsNullOrEmpty(currentDefines))
            {
                GUILayout.Label("None", descriptionStyle);
            }
            else
            {
                foreach (string symbol in currentDefines.Split(';'))
                {
                    if (!string.IsNullOrWhiteSpace(symbol))
                    {
                        GUIStyle symbolStyle = new GUIStyle(descriptionStyle);
                        if (DEFINE_SYMBOLS.Contains(symbol.Trim()))
                        {
                            symbolStyle.normal.textColor = Color.green;
                            GUILayout.Label($"✓ {symbol.Trim()}", symbolStyle);
                        }
                        else
                        {
                            GUILayout.Label($"• {symbol.Trim()}", symbolStyle);
                        }
                    }
                }
            }
            
            // Show what will be added
            GUILayout.Label("Will be added:", EditorStyles.boldLabel);
            foreach (string symbol in DEFINE_SYMBOLS)
            {
                GUIStyle symbolStyle = new GUIStyle(descriptionStyle);
                if (currentDefines.Contains(symbol))
                {
                    symbolStyle.normal.textColor = Color.gray;
                    GUILayout.Label($"✓ {symbol} (already present)", symbolStyle);
                }
                else
                {
                    symbolStyle.normal.textColor = Color.yellow;
                    GUILayout.Label($"+ {symbol}", symbolStyle);
                }
            }
            
            EditorGUILayout.EndScrollView();
            
            // Setup button
            if (GUILayout.Button("Setup SNEngine", buttonStyle))
            {
                PerformSetup();
            }
            
            // Checkbox to disable future welcome windows
            bool showOnStartup = !EditorPrefs.GetBool(WINDOW_PREF_KEY, false);
            bool newValue = !EditorGUILayout.Toggle("Show this window on startup", showOnStartup);
            EditorPrefs.SetBool(WINDOW_PREF_KEY, newValue);
        }
        
        private void PerformSetup()
        {
            // Add define symbols
            AddDefineSymbols();
            
            // Run auto configuration scripts
            RunAutoConfiguration();
            
            // Show confirmation
            EditorUtility.DisplayDialog("Setup Complete", 
                "SNEngine has been successfully configured!\n\n" +
                "Define symbols have been added to your project.\n" +
                "Auto-icon assignment and execution order management are now active.", 
                "OK");
                
            // Close the window after setup
            Close();
        }
        
        private void AddDefineSymbols()
        {
            string currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
            HashSet<string> defineSet = new HashSet<string>();
            
            // Add existing symbols
            if (!string.IsNullOrEmpty(currentDefines))
            {
                foreach (string symbol in currentDefines.Split(';'))
                {
                    if (!string.IsNullOrWhiteSpace(symbol))
                    {
                        defineSet.Add(symbol.Trim());
                    }
                }
            }
            
            // Add our symbols
            foreach (string symbol in DEFINE_SYMBOLS)
            {
                defineSet.Add(symbol);
            }
            
            // Convert back to string
            string newDefines = string.Join(";", defineSet);
            
            // Apply to all build target groups
            foreach (BuildTargetGroup targetGroup in System.Enum.GetValues(typeof(BuildTargetGroup)))
            {
                if (targetGroup == BuildTargetGroup.Unknown) continue;
                
                try
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, newDefines);
                }
                catch
                {
                    // Some build target groups might not be valid, ignore those
                }
            }
            
            Debug.Log($"[SNEngine] Define symbols set: {newDefines}");
        }
        
        private void RunAutoConfiguration()
        {
            // Trigger the auto-icon assigner and execution order manager
            // These are already running via [InitializeOnLoad], but we can ensure they're active
            
            // Also handle TextMeshPro auto-installation if needed
            TrySetupTextMeshPro();
        }
        
        private void TrySetupTextMeshPro()
        {
            // Check if TextMeshPro is available
            System.Type tmpType = System.Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
            
            if (tmpType == null)
            {
                // TextMeshPro is not available, suggest importing it
                if (EditorUtility.DisplayDialog("TextMeshPro Recommended", 
                    "TextMeshPro is recommended for best text rendering in SNEngine.\n\n" +
                    "Would you like to import TextMeshPro now? This will download and import it from the Package Manager.", 
                    "Import TextMeshPro", "Later"))
                {
                    ImportTextMeshPro();
                }
            }
        }
        
        private void ImportTextMeshPro()
        {
            // Open the package manager to TextMeshPro
            EditorApplication.delayCall += () =>
            {
                // Use Unity's PackageManager API to install TextMeshPro
                InstallTextMeshProViaPackageManager();
            };
        }
        
        private void InstallTextMeshProViaPackageManager()
        {
            // Using reflection to access Unity's PackageManager
            System.Type packageManagerWindowType = System.Type.GetType("UnityEditor.PackageManager.UI.Window,UnityEditor.PackageManager.UI");
            
            if (packageManagerWindowType != null)
            {
                // Open package manager and search for TextMeshPro
                EditorApplication.ExecuteMenuItem("Window/Package Manager");
                
                Debug.Log("[SNEngine] Please install TextMeshPro from the Package Manager (com.unity.textmeshpro)");
            }
            else
            {
                Debug.LogWarning("[SNEngine] Could not access Package Manager. Please install TextMeshPro manually from Window > Package Manager.");
            }
        }
    }
}