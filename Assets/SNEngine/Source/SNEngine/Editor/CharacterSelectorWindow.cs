#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using SNEngine.CharacterSystem;

namespace SiphoinUnityHelpers.XNodeExtensions.Editor
{
    public class CharacterSelectorWindow : EditorWindow
    {
        private Action<Character> _onSelect;
        private string _searchQuery = "";
        private Vector2 _scrollPos;
        private List<Character> _characters = new List<Character>();

        public static void Open(Action<Character> onSelect)
        {
            var window = GetWindow<CharacterSelectorWindow>(true, "Character Selector", true);
            window._onSelect = onSelect;
            window.minSize = new Vector2(350, 450);
            window.RefreshCache();
            window.ShowAuxWindow();
        }

        private void RefreshCache()
        {
            _characters = Resources.LoadAll<Character>("").OrderBy(c => c.name).ToList();
        }

        private void OnGUI()
        {
            DrawHeader();
            DrawSearchBar();
            EditorGUILayout.Space(5);
            DrawCharacterList();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(2);
            EditorGUILayout.LabelField("Character Selector", EditorStyles.boldLabel);
            GUILayout.Space(2);
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

        private void DrawCharacterList()
        {
            var filtered = _characters
                .Where(c => string.IsNullOrEmpty(_searchQuery) || c.name.IndexOf(_searchQuery, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            if (filtered.Count == 0)
            {
                EditorGUILayout.HelpBox("No characters found in Resources.", MessageType.Info);
                return;
            }

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            foreach (var character in filtered)
            {
                float rowHeight = 48f;
                Rect rect = EditorGUILayout.BeginHorizontal(GUI.skin.button, GUILayout.Height(rowHeight));

                if (rect.Contains(Event.current.mousePosition))
                {
                    EditorGUI.DrawRect(rect, new Color(1, 1, 1, 0.05f));
                }

                GUILayout.Space(8);

                EditorGUILayout.BeginVertical(GUILayout.Width(32), GUILayout.Height(rowHeight));
                GUILayout.FlexibleSpace();

                Texture icon = AssetPreview.GetAssetPreview(character);
                if (icon == null)
                {
                    icon = EditorGUIUtility.ObjectContent(character, typeof(Character)).image;
                }

                Rect iconRect = GUILayoutUtility.GetRect(26, 26);
                if (icon != null)
                {
                    GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();

                GUILayout.Space(4);

                EditorGUILayout.BeginVertical(GUILayout.Height(rowHeight));
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(character.name, EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Character Asset", EditorStyles.miniLabel);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(GUILayout.Width(75), GUILayout.Height(rowHeight));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Select", GUILayout.Height(26)))
                {
                    _onSelect?.Invoke(character);
                    Close();
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();

                GUILayout.Space(4);
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(2);
            }

            EditorGUILayout.EndScrollView();
        }
    }
}
#endif