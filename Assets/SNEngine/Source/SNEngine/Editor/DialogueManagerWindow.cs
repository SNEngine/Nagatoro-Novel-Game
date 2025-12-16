using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using XNode;
using SNEngine.Graphs;
using SNEngine.Services;
using SNEngine.DialogSystem;

namespace SNEngine.Editor
{
    public class DialogueManagerWindow : EditorWindow
    {
        private const string DialoguesResourcePath = "Dialogues";
        private const string TargetFolderPath = "Assets/SNEngine/Source/SNEngine/Resources/Dialogues";
        private string _newDialogueName = "NewDialogue";
        private string _searchQuery = "";
        private Vector2 _scrollPosition;

        private static DialogueService _dialogueService;

        [MenuItem("SNEngine/Dialogue Manager")]
        public static void ShowWindow()
        {
            var window = GetWindow<DialogueManagerWindow>("Dialogue Manager");
            window.minSize = new Vector2(400, 300);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(5);

            DrawHeader();
            DrawCreationSection();
            DrawListSection();

            EditorGUILayout.Space(10);
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("Dialogue Management", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
        }

        private void DrawCreationSection()
        {
            GUI.backgroundColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUI.backgroundColor = Color.white;

            EditorGUILayout.LabelField("Create New Dialogue", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name:", GUILayout.Width(45));
            _newDialogueName = EditorGUILayout.TextField(_newDialogueName);
            if (GUILayout.Button("Create Asset", GUILayout.Width(100), GUILayout.Height(20)))
            {
                CreateNewDialogue();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10);
        }

        private void DrawListSection()
        {
            EditorGUILayout.LabelField("Existing Dialogues", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            _searchQuery = EditorGUILayout.TextField(_searchQuery, EditorStyles.toolbarSearchField);
            if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(20)))
            {
                _searchQuery = "";
                GUI.FocusControl(null);
            }
            EditorGUILayout.EndHorizontal();

            var allDialogueAssets = Resources.LoadAll<DialogueGraph>(DialoguesResourcePath)
                .Where(d => d != null)
                .ToArray();

            var filteredAssets = string.IsNullOrEmpty(_searchQuery)
                ? allDialogueAssets
                : allDialogueAssets.Where(d =>
                    d.name.IndexOf(_searchQuery, System.StringComparison.OrdinalIgnoreCase) >= 0).ToArray();

            if (filteredAssets.Length == 0)
            {
                EditorGUILayout.HelpBox(
                    string.IsNullOrEmpty(_searchQuery)
                        ? "No dialogues found in Resources/Dialogues."
                        : "No dialogues match the search query.", MessageType.Info);
                return;
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            for (int i = 0; i < filteredAssets.Length; i++)
            {
                DrawDialogueItem(filteredAssets[i]);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawDialogueItem(DialogueGraph graph)
        {
            if (graph == null) return;

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            Texture2D icon = AssetPreview.GetMiniThumbnail(graph);
            GUILayout.Label(icon, GUILayout.Width(20), GUILayout.Height(20));

            EditorGUILayout.LabelField(graph.name, EditorStyles.boldLabel, GUILayout.Height(20), GUILayout.ExpandWidth(true));

            if (Application.isPlaying)
            {
                GUI.backgroundColor = new Color(0.5f, 1f, 0.5f);
                if (GUILayout.Button("▶️ Play", EditorStyles.miniButton, GUILayout.Width(50), GUILayout.Height(20)))
                {
                    PlayDialogue(graph);
                }
                GUI.backgroundColor = Color.white;
            }

            if (GUILayout.Button("Rename", EditorStyles.miniButton, GUILayout.Width(70), GUILayout.Height(20)))
            {
                Selection.activeObject = graph;
                EditorGUIUtility.PingObject(graph);
                Debug.Log($"[Dialogue Manager] Asset '{graph.name}' selected. Press F2 in the Project window to rename it.");
            }

            if (GUILayout.Button("Open", EditorStyles.miniButton, GUILayout.Width(60), GUILayout.Height(20)))
            {
                OpenGraphInEditor(graph);
            }

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Delete", EditorStyles.miniButton, GUILayout.Width(60), GUILayout.Height(20)))
            {
                if (EditorUtility.DisplayDialog("Confirm Deletion",
                    $"Are you sure you want to delete dialogue '{graph.name}'?", "Yes", "No"))
                {
                    DeleteDialogue(graph);
                }
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();
        }

        private void CreateNewDialogue()
        {
            if (string.IsNullOrWhiteSpace(_newDialogueName))
            {
                EditorUtility.DisplayDialog("Error", "Dialogue name cannot be empty.", "OK");
                return;
            }

            string uniqueAssetName = _newDialogueName.EndsWith(".asset") ? _newDialogueName : $"{_newDialogueName}.asset";
            string existingPath = Path.Combine(TargetFolderPath, uniqueAssetName);

            if (File.Exists(existingPath))
            {
                EditorUtility.DisplayDialog("Error", $"Dialogue '{_newDialogueName}' already exists.", "OK");
                return;
            }

            DialogueCreatorEditor.CreateNewDialogueAssetFromName(_newDialogueName);

            Repaint();
        }

        private void DeleteDialogue(NodeGraph graph)
        {
            string assetPath = AssetDatabase.GetAssetPath(graph);
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogError($"Could not find asset path for: {graph.name}");
                return;
            }

            AssetDatabase.DeleteAsset(assetPath);
            AssetDatabase.Refresh();

            Repaint();
        }

        private void OpenGraphInEditor(NodeGraph graph)
        {
            Selection.activeObject = graph;
            DialogueCreatorEditor.OpenGraph(graph);
        }

        private void PlayDialogue(DialogueGraph graph)
        {
            if (graph == null) return;

            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Error", "Enter Play Mode to run the dialogue.", "OK");
                return;
            }

            if (_dialogueService is null)
            {
                _dialogueService = NovelGame.Instance.GetService<DialogueService>();
            }

            NovelGame.Instance.ResetStateServices();
            NovelGame.Instance.GetService<MainMenuService>().Hide();
            _dialogueService.StopCurrentDialogue();
            _dialogueService.JumpToDialogue(graph);
        }
    }
}