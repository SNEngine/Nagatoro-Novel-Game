using SiphoinUnityHelpers.XNodeExtensions.NodesControlExecutes;
using SiphoinUnityHelpers.XNodeExtensions.NodesControlExecutes.Switch;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

namespace SiphoinUnityHelpers.XNodeExtensions.Editor
{
    public abstract class BaseSwitchNodeEditor<T> : NodeEditor
    {
        // Сколько места резервируем под порт
        private const float PortWidth = 24f;

        public override void OnBodyGUI()
        {
            serializedObject.Update();

            var node = target as SwitchNode<T>;
            if (node == null) return;

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_enter"));
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_value"));

            EditorGUILayout.Space(6);
            DrawInlineCases(node);
            EditorGUILayout.Space(6);

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_default"));

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawInlineCases(SwitchNode<T> node)
        {
            SerializedProperty casesProp = serializedObject.FindProperty("_cases");
            int indexToRemove = -1;

            for (int i = 0; i < casesProp.arraySize; i++)
            {
                SerializedProperty element = casesProp.GetArrayElementAtIndex(i);
                string portName = GetPortNameFromProperty(element);
                NodePort port = node.GetOutputPort(portName);

                // ─────────────────────────────
                // LAYOUT
                // ─────────────────────────────
                EditorGUILayout.BeginHorizontal();

                // Кнопка удаления
                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    indexToRemove = i;
                }

                // Контейнер поля (динамическая высота)
                EditorGUILayout.BeginVertical();
                EditorGUILayout.PropertyField(element, GUIContent.none, true);
                EditorGUILayout.EndVertical();

                // 🔥 РЕЗЕРВ МЕСТА ПОД ПОРТ
                GUILayout.Space(PortWidth);

                EditorGUILayout.EndHorizontal();

                // ─────────────────────────────
                // PORT DRAW
                // ─────────────────────────────
                Rect rowRect = GUILayoutUtility.GetLastRect();

                if (port != null)
                {
                    Vector2 portPos = new Vector2(
                        rowRect.xMax - PortWidth * 0.5f,
                        rowRect.center.y - 8f
                    );

                    NodeEditorGUILayout.PortField(portPos, port);
                }

                EditorGUILayout.Space(2);
            }

            // Удаление case
            if (indexToRemove != -1)
            {
                casesProp.DeleteArrayElementAtIndex(indexToRemove);
                serializedObject.ApplyModifiedProperties();
                SyncPorts();
            }

            // Добавление case
            if (GUILayout.Button("Add Case", EditorStyles.miniButton))
            {
                casesProp.InsertArrayElementAtIndex(casesProp.arraySize);
                serializedObject.ApplyModifiedProperties();
                SyncPorts();
            }
        }

        protected void SyncPorts()
        {
            var node = target as SwitchNode<T>;
            if (node == null) return;

            SerializedProperty casesProperty = serializedObject.FindProperty("_cases");

            HashSet<string> requiredPorts = new HashSet<string>();

            for (int i = 0; i < casesProperty.arraySize; i++)
            {
                string portName = GetPortNameFromProperty(
                    casesProperty.GetArrayElementAtIndex(i)
                );

                requiredPorts.Add(portName);

                if (!node.HasPort(portName))
                {
                    node.AddDynamicOutput(
                        typeof(NodeControlExecute),
                        Node.ConnectionType.Multiple,
                        Node.TypeConstraint.None,
                        portName
                    );
                }
            }

            List<string> portsToRemove = new List<string>();

            foreach (NodePort port in node.DynamicOutputs)
            {
                if (!requiredPorts.Contains(port.fieldName))
                {
                    portsToRemove.Add(port.fieldName);
                }
            }

            foreach (string portName in portsToRemove)
            {
                node.RemoveDynamicPort(portName);
            }
        }

        /// <summary>
        /// Должен вернуть УНИКАЛЬНОЕ имя порта для case
        /// </summary>
        protected abstract string GetPortNameFromProperty(SerializedProperty prop);
    }
}
