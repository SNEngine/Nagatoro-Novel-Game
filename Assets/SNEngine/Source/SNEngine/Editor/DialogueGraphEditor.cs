using SNEngine.Graphs;
using UnityEngine;
using XNodeEditor;
using UnityEditor;
using SiphoinUnityHelpers.XNodeExtensions;

namespace SNEngine.Editor
{
    [CustomNodeGraphEditor(typeof(DialogueGraph))]
    public class DialogueGraphEditor : NodeGraphEditor
    {
        public override void OnGUI()
        {
            // Draw the toolbar buttons at the top
            DrawToolbarButtons();

            // Call the base OnGUI to draw the graph content
            base.OnGUI();
        }

        private void DrawToolbarButtons()
        {
            // Create a toolbar-style button at the top
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUILayout.Space(5); // Add some left padding

            if (GUILayout.Button("Add Node", EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                // Get the current mouse position in grid coordinates
                Vector2 graphPos = NodeEditorWindow.current.WindowToGridPosition(Event.current.mousePosition);

                // Open the node selector window
                NodeSelectorWindow.Open((nodeType, position) => {
                    // Create the selected node at the specified position
                    CreateNode(nodeType, position);
                }, graphPos);
            }

            if (GUILayout.Button("Search Nodes", EditorStyles.toolbarButton, GUILayout.Width(120)))
            {
                // Open the node search window
                NodeSearchWindow.Open(target as BaseGraph);
            }

            GUILayout.FlexibleSpace(); // Push everything else to the left

            EditorGUILayout.EndHorizontal();
        }
    }
}