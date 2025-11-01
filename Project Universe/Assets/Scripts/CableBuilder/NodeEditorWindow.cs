#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace ProjectUniverse.PowerSystem.Editor
{
    public class NodeEditorWindow : EditorWindow
    {
        private PowerNode selectedNode;
        private Vector2 scrollPosition;
        private bool showConnections = true;
        private bool showRouting = true;

        [MenuItem("Tools/Cable System/Node Editor")]
        public static void ShowWindow()
        {
            GetWindow<NodeEditorWindow>("Node Editor");
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            // Node selection
            EditorGUILayout.LabelField("Node Selection", EditorStyles.boldLabel);
            selectedNode = EditorGUILayout.ObjectField("Node", selectedNode, typeof(PowerNode), true) as PowerNode;

            if (selectedNode == null)
            {
                EditorGUILayout.HelpBox("Select a Node object to edit", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // Node Configuration
            EditorGUILayout.LabelField("Node Configuration", EditorStyles.boldLabel);
            Undo.RecordObject(selectedNode, "Edit Node");

            selectedNode.nodeId = EditorGUILayout.TextField("Node ID", selectedNode.nodeId);

            EditorGUI.BeginChangeCheck();
            int newConnectionCount = EditorGUILayout.IntSlider("Connection Count", selectedNode.connectionCount, 1, 8);

            if (EditorGUI.EndChangeCheck() && newConnectionCount != selectedNode.connectionCount)
            {
                if (EditorUtility.DisplayDialog("Regenerate Node",
                    "Changing connection count will reset all connections and routing. Continue?",
                    "Yes", "No"))
                {
                    selectedNode.RegenerateWithNewCount(newConnectionCount);
                }
            }

            EditorGUILayout.Space();

            // Internal Routing Configuration
            showRouting = EditorGUILayout.Foldout(showRouting, "Internal Routing Configuration");
            if (showRouting)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                // Create routing matrix
                EditorGUILayout.LabelField("Connection Matrix", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Click to toggle connections between inputs and outputs");

                EditorGUILayout.Space();

                // Header row
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("", GUILayout.Width(60)); // Empty corner

                for (int o = 0; o < selectedNode.connectionCount; o++)
                {
                    EditorGUILayout.LabelField($"OUT {o}", GUILayout.Width(50));
                }
                EditorGUILayout.EndHorizontal();

                // Input rows
                for (int i = 0; i < selectedNode.connectionCount; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"IN {i}", GUILayout.Width(60));

                    for (int o = 0; o < selectedNode.connectionCount; o++)
                    {
                        bool isConnected = selectedNode.IsInputConnectedToOutput(i, o);

                        GUI.backgroundColor = isConnected ? Color.green : Color.gray;

                        if (GUILayout.Button(isConnected ? "●" : "○", GUILayout.Width(50), GUILayout.Height(20)))
                        {
                            if (isConnected)
                            {
                                // Disconnect
                                selectedNode.SetRoute(i, o, false);
                            }
                            else
                            {
                                // Connect (will automatically disconnect any existing connections)
                                selectedNode.SetRoute(i, o, true);
                            }
                        }

                        GUI.backgroundColor = Color.white;
                    }

                    // Show current route
                    var route = selectedNode.GetRouteFromInput(i);
                    if (route != null && route.isConnected)
                    {
                        EditorGUILayout.LabelField($"→ OUT {route.outputIndex}", GUILayout.Width(70));
                    }
                    else
                    {
                        EditorGUILayout.LabelField("(Disconnected)", GUILayout.Width(70));
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.Space();

                if (GUILayout.Button("Reset to Default Routing"))
                {
                    for (int i = 0; i < selectedNode.connectionCount; i++)
                    {
                        selectedNode.SetRoute(i, i, true);
                    }
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space();

            // Active Connections
            showConnections = EditorGUILayout.Foldout(showConnections,
                $"External Connections ({selectedNode.activeConnections.Count})");
            if (showConnections && selectedNode.activeConnections.Count > 0)
            {
                EditorGUI.indentLevel++;

                // Show input connections
                EditorGUILayout.LabelField("Input Connections:", EditorStyles.boldLabel);
                for (int i = 0; i < selectedNode.connectionCount; i++)
                {
                    var inputConn = selectedNode.activeConnections.Find(c => c.isInput && c.pointIndex == i);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"Input {i}:", GUILayout.Width(60));

                    if (inputConn != null)
                    {
                        EditorGUILayout.LabelField($"{inputConn.connectedTemplate.name} - {inputConn.connectedPath.pathId}");

                        string sizeInfo = inputConn.connectedPath.assignedCableSize.HasValue ?
                            $"[{inputConn.connectedPath.assignedCableSize.Value}]" : "[Unassigned]";
                        EditorGUILayout.LabelField(sizeInfo, GUILayout.Width(100));

                        if (GUILayout.Button("Select", GUILayout.Width(60)))
                        {
                            Selection.activeGameObject = inputConn.connectedTemplate.gameObject;
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("(Not connected)");
                    }
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.Space();

                // Show output connections
                EditorGUILayout.LabelField("Output Connections:", EditorStyles.boldLabel);
                for (int i = 0; i < selectedNode.connectionCount; i++)
                {
                    var outputConn = selectedNode.activeConnections.Find(c => !c.isInput && c.pointIndex == i);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"Output {i}:", GUILayout.Width(60));

                    if (outputConn != null)
                    {
                        EditorGUILayout.LabelField($"{outputConn.connectedTemplate.name} - {outputConn.connectedPath.pathId}");

                        string sizeInfo = outputConn.connectedPath.assignedCableSize.HasValue ?
                            $"[{outputConn.connectedPath.assignedCableSize.Value}]" : "[Unassigned]";
                        EditorGUILayout.LabelField(sizeInfo, GUILayout.Width(100));

                        if (GUILayout.Button("Select", GUILayout.Width(60)))
                        {
                            Selection.activeGameObject = outputConn.connectedTemplate.gameObject;
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("(Not connected)");
                    }
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            // Visualization Settings
            EditorGUILayout.LabelField("Visualization Settings", EditorStyles.boldLabel);
            selectedNode.showGizmos = EditorGUILayout.Toggle("Show Gizmos", selectedNode.showGizmos);
            selectedNode.inputPointColor = EditorGUILayout.ColorField("Input Point Color", selectedNode.inputPointColor);
            selectedNode.outputPointColor = EditorGUILayout.ColorField("Output Point Color", selectedNode.outputPointColor);
            selectedNode.nodeColor = EditorGUILayout.ColorField("Node Color", selectedNode.nodeColor);
            selectedNode.routeColor = EditorGUILayout.ColorField("Route Color", selectedNode.routeColor);
            selectedNode.disconnectedRouteColor = EditorGUILayout.ColorField("Disconnected Route Color", selectedNode.disconnectedRouteColor);
            selectedNode.gizmoSize = EditorGUILayout.Slider("Gizmo Size", selectedNode.gizmoSize, 0.1f, 0.5f);

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(selectedNode);
                SceneView.RepaintAll();
            }
        }

        public void OnSelectionChange()
        {
            if (Selection.activeGameObject != null)
            {
                if (Selection.activeGameObject.TryGetComponent<PowerNode>(out var node))
                {
                    selectedNode = node;
                    Repaint();
                }
            }
        }
    }

    [CustomEditor(typeof(PowerNode))]
    public class NodeInspectorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Node Editor", GUILayout.Height(30)))
            {
                NodeEditorWindow window = EditorWindow.GetWindow<NodeEditorWindow>("Node Editor");
                window.Show();
                window.OnSelectionChange();
            }

            EditorGUILayout.Space();
            DrawDefaultInspector();
        }
    }
}
#endif