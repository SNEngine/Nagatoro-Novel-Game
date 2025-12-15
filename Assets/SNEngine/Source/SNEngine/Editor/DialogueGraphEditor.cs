using SNEngine.DialogSystem;
using SNEngine.Graphs;
using SNEngine.Services;
using UnityEditor;
using UnityEngine;
using XNodeEditor;
using static UnityEngine.GraphicsBuffer;

namespace SNEngine.Editor
{
    [CustomEditor(typeof(DialogueGraph), true)]
    public class DialogueGraphEditor : UnityEditor.Editor
    {
        private DialogueGraph _graph;
        private static DialogueService _dialogueService;

        private void OnEnable()
        {
            _graph = target as DialogueGraph;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (Application.isPlaying)
            {
                if (GUILayout.Button("Play Dialogue", GUILayout.Height(40)))
                {
                    if (_graph != null)
                    {
                        if (_dialogueService is null)
                        {
                            _dialogueService = NovelGame.Instance.GetService<DialogueService>();
                        }

                        NovelGame.Instance.ResetStateServices();
                        NovelGame.Instance.GetService<MainMenuService>().Hide();
                        _dialogueService.JumpToDialogue(_graph);
                    }
                    else
                    {
                        Debug.LogError("Dialogue Graph is null.");
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Enter Play Mode to test 'Play Dialogue'.", MessageType.Info);
            }

            if (GUILayout.Button("Edit graph", GUILayout.Height(40)))
            {
                NodeEditorWindow.Open(serializedObject.targetObject as XNode.NodeGraph);
            }

            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            GUILayout.Label("Raw data", "BoldLabel");

            DrawDefaultInspector();

            serializedObject.ApplyModifiedProperties();
        }
    }
}