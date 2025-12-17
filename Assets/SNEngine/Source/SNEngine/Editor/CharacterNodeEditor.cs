#if UNITY_EDITOR
using SNEngine.CharacterSystem;
using SNEngine.Editor;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace SiphoinUnityHelpers.XNodeExtensions.Editor
{
    [CustomNodeEditor(typeof(CharacterNode))]
    public class CharacterNodeEditor : NodeEditor
    {
        public override void OnBodyGUI()
        {
            serializedObject.Update();

            CharacterNode node = target as CharacterNode;
            SerializedProperty characterProp = serializedObject.FindProperty("_character");

            foreach (var tag in NodeEditorGUILayout.GetFilteredFields(serializedObject))
            {
                if (tag.name == "_character") continue;
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty(tag.name));
            }

            GUILayout.Space(8);

            string displayName = node.Character != null ? node.Character.name : "Select Character";
            Color btnColor = node.Character != null ? new Color(0.3f, 0.5f, 0.3f) : new Color(0.4f, 0.2f, 0.2f);

            Color prevColor = GUI.backgroundColor;
            GUI.backgroundColor = btnColor;

            if (GUILayout.Button(displayName, GUILayout.Height(30)))
            {
                CharacterSelectorWindow.Open((selectedCharacter) =>
                {
                    serializedObject.Update();
                    characterProp.objectReferenceValue = selectedCharacter;
                    serializedObject.ApplyModifiedProperties();
                });
            }

            GUI.backgroundColor = prevColor;
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif