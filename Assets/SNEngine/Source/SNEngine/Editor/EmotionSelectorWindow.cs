#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using SNEngine.CharacterSystem;

namespace SiphoinUnityHelpers.XNodeExtensions.Editor
{
    public class EmotionSelectorWindow : EditorWindow
    {
        private Action<string> _onSelect;
        private Character _character;
        private string _searchQuery = "";
        private Vector2 _scrollPos;

        public static void Open(Character character, Action<string> onSelect)
        {
            var window = GetWindow<EmotionSelectorWindow>(true, "Emotion Selector", true);
            window._character = character;
            window._onSelect = onSelect;
            window.minSize = new Vector2(300, 450);
            window.ShowAuxWindow();
        }

        private void OnGUI()
        {
            if (_character == null)
            {
                EditorGUILayout.HelpBox("Character is null.", MessageType.Error);
                return;
            }

            DrawHeader();
            DrawSearchBar();
            EditorGUILayout.Space(5);
            DrawEmotionList();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(2);
            EditorGUILayout.LabelField($"Emotions: {_character.name}", EditorStyles.boldLabel);
            EditorGUILayout.EndVertical();
        }

        private void DrawSearchBar()
        {
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            _searchQuery = EditorGUILayout.TextField(new GUIContent("", EditorGUIUtility.FindTexture("Search Icon")), _searchQuery, GUILayout.Height(20));
            if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(20)))
            {
                _searchQuery = "";
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawEmotionList()
        {
            var emotions = _character.Emotions
                .Where(e => string.IsNullOrEmpty(_searchQuery) || e.Name.IndexOf(_searchQuery, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            if (emotions.Count == 0)
            {
                EditorGUILayout.HelpBox("No emotions found.", MessageType.Info);
                return;
            }

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            foreach (var emotion in emotions)
            {
                float rowHeight = 64f;
                Rect rect = EditorGUILayout.BeginHorizontal(GUI.skin.button, GUILayout.Height(rowHeight));

                if (rect.Contains(Event.current.mousePosition))
                {
                    EditorGUI.DrawRect(rect, new Color(1, 1, 1, 0.05f));
                }

                GUILayout.Space(5);

                EditorGUILayout.BeginVertical(GUILayout.Width(56), GUILayout.Height(rowHeight));
                GUILayout.FlexibleSpace();

                Texture preview = null;
                if (emotion.Sprite != null)
                {
                    preview = AssetPreview.GetAssetPreview(emotion.Sprite);
                }

                if (preview == null)
                {
                    var iconContent = EditorGUIUtility.IconContent("d_FilterByLabel");
                    preview = iconContent != null ? iconContent.image : null;
                }

                Rect iconRect = GUILayoutUtility.GetRect(52, 52);
                if (preview != null)
                {
                    GUI.DrawTexture(iconRect, preview, ScaleMode.ScaleToFit);
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(GUILayout.Height(rowHeight));
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(emotion.Name, EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Character Emotion", EditorStyles.miniLabel);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(GUILayout.Width(65), GUILayout.Height(rowHeight));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Select", GUILayout.Height(28)))
                {
                    _onSelect?.Invoke(emotion.Name);
                    Close();
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
                GUILayout.Space(2);
            }

            EditorGUILayout.EndScrollView();
        }
    }
}
#endif