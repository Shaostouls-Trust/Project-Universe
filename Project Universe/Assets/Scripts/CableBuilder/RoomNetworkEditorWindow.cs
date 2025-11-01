#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ProjectUniverse.PowerSystem.Editor
{
    public class RoomNetworkEditorWindow : EditorWindow
    {
        private RoomNetwork selectedNetwork;
        private Vector2 scrollPosition;
        private bool showConnections = true;
        private bool showEndpoints = true;
        private bool showCablePaths = true;
        private RoomNetwork.RoomPath selectedPath;

        [MenuItem("Tools/Cable System/Room Network Editor")]
        public static void ShowWindow()
        {
            GetWindow<RoomNetworkEditorWindow>("Room Network Editor");
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            // Network selection
            EditorGUILayout.LabelField("Room Network Selection", EditorStyles.boldLabel);
            selectedNetwork = EditorGUILayout.ObjectField("Network", selectedNetwork, typeof(RoomNetwork), true) as RoomNetwork;

            if (selectedNetwork == null)
            {
                EditorGUILayout.HelpBox("Select a Room Network object to edit", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // Network properties
            EditorGUILayout.LabelField("Network Configuration", EditorStyles.boldLabel);
            Undo.RecordObject(selectedNetwork, "Edit Room Network");

            selectedNetwork.roomId = EditorGUILayout.TextField("Room ID", selectedNetwork.roomId);
            selectedNetwork.connectionThreshold = EditorGUILayout.FloatField("Connection Threshold", selectedNetwork.connectionThreshold);
            selectedNetwork.autoDiscoverTemplates = EditorGUILayout.Toggle("Auto Discover Templates", selectedNetwork.autoDiscoverTemplates);
            selectedNetwork.showConnectionGizmos = EditorGUILayout.Toggle("Show Connection Gizmos", selectedNetwork.showConnectionGizmos);

            EditorGUILayout.Space();

            // Color settings
            EditorGUILayout.LabelField("Visualization Settings", EditorStyles.boldLabel);
            selectedNetwork.compatibleConnectionColor = EditorGUILayout.ColorField("Compatible Connection", selectedNetwork.compatibleConnectionColor);
            selectedNetwork.incompatibleConnectionColor = EditorGUILayout.ColorField("Incompatible Connection", selectedNetwork.incompatibleConnectionColor);
            selectedNetwork.entryPointColor = EditorGUILayout.ColorField("Entry Point", selectedNetwork.entryPointColor);
            selectedNetwork.exitPointColor = EditorGUILayout.ColorField("Exit Point", selectedNetwork.exitPointColor);
            selectedNetwork.selectedPathColor = EditorGUILayout.ColorField("Selected Path", selectedNetwork.selectedPathColor);

            EditorGUILayout.Space();

            // Templates
            EditorGUILayout.LabelField("Templates", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Templates Count: {selectedNetwork.templates.Count}");

            if (GUILayout.Button("Discover Templates"))
            {
                selectedNetwork.DiscoverTemplates();
            }

            if (GUILayout.Button("Refresh Connections"))
            {
                selectedNetwork.RefreshConnections();
                SceneView.RepaintAll();
            }

            EditorGUILayout.Space();

            // Connections
            var connections = selectedNetwork.GetConnections();
            showConnections = EditorGUILayout.Foldout(showConnections, $"Template Connections ({connections.Count})");
            if (showConnections && connections.Count > 0)
            {
                EditorGUI.indentLevel++;

                foreach (var connection in connections)
                {
                    string sourcePointType = connection.isSourceEntryPoint ? "Entry" : "Exit";
                    string targetPointType = connection.isTargetEntryPoint ? "Entry" : "Exit";
                    string compatibilityStatus = connection.isSizeCompatible ? "Compatible" : "Incompatible";

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    EditorGUILayout.LabelField($"{connection.sourceTemplate.name} ({connection.sourcePath.pathId} {sourcePointType}) → " +
                                              $"{connection.targetTemplate.name} ({connection.targetPath.pathId} {targetPointType})",
                                              EditorStyles.boldLabel);

                    // Display connection status with color
                    GUIStyle statusStyle = new(EditorStyles.label);
                    statusStyle.normal.textColor = connection.isSizeCompatible ?
                        selectedNetwork.compatibleConnectionColor :
                        selectedNetwork.incompatibleConnectionColor;

                    EditorGUILayout.LabelField($"Status: {compatibilityStatus}", statusStyle);

                    // Display cable size info
                    string sourceSize = connection.sourcePath.assignedCableSize.HasValue ?
                        connection.sourcePath.assignedCableSize.Value.ToString() :
                        "Unassigned";

                    string targetSize = connection.targetPath.assignedCableSize.HasValue ?
                        connection.targetPath.assignedCableSize.Value.ToString() :
                        "Unassigned";

                    EditorGUILayout.LabelField($"Source Size: {sourceSize}");
                    EditorGUILayout.LabelField($"Target Size: {targetSize}");

                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Select Source", GUILayout.Width(120)))
                    {
                        Selection.activeGameObject = connection.sourceTemplate.gameObject;
                        SceneView.lastActiveSceneView.FrameSelected();
                    }

                    if (GUILayout.Button("Select Target", GUILayout.Width(120)))
                    {
                        Selection.activeGameObject = connection.targetTemplate.gameObject;
                        SceneView.lastActiveSceneView.FrameSelected();
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.EndVertical();

                    EditorGUILayout.Space();
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            // Room Endpoints
            var endpoints = selectedNetwork.GetRoomEntries().Concat(selectedNetwork.GetRoomExits()).ToList();
            showEndpoints = EditorGUILayout.Foldout(showEndpoints, $"Room Endpoints ({endpoints.Count})");
            if (showEndpoints && endpoints.Count > 0)
            {
                EditorGUI.indentLevel++;

                foreach (var endpoint in endpoints)
                {
                    string type = endpoint.isEntryPoint ? "Entry" : "Exit";

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    EditorGUILayout.LabelField($"Room {type}: {endpoint.template.name} ({endpoint.path.pathId})",
                                              EditorStyles.boldLabel);

                    Vector3 worldPos = endpoint.GetWorldPosition();
                    EditorGUILayout.LabelField($"World Position: {worldPos}");

                    string sizeInfo = endpoint.path.assignedCableSize.HasValue ?
                        endpoint.path.assignedCableSize.Value.ToString() :
                        string.Join(", ", endpoint.path.supportedCableSizes.Select(s => s.ToString()));

                    EditorGUILayout.LabelField($"Cable Size: {sizeInfo}");

                    if (GUILayout.Button("Select Template", GUILayout.Width(120)))
                    {
                        Selection.activeGameObject = endpoint.template.gameObject;
                        SceneView.lastActiveSceneView.FrameSelected();
                    }

                    EditorGUILayout.EndVertical();

                    EditorGUILayout.Space();
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            // Cable Paths
            var paths = selectedNetwork.GetCablePaths();
            showCablePaths = EditorGUILayout.Foldout(showCablePaths, $"Cable Paths ({paths.Count})");
            if (showCablePaths && paths.Count > 0)
            {
                EditorGUI.indentLevel++;

                foreach (var path in paths)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    // Highlight selected path
                    GUIStyle pathStyle = new(EditorStyles.boldLabel);
                    if (path.isSelected)
                    {
                        pathStyle.normal.textColor = selectedNetwork.selectedPathColor;
                    }

                    EditorGUILayout.LabelField($"Path: {path.entry.template.name} → {path.exit.template.name}",
                                              pathStyle);

                    EditorGUILayout.LabelField($"Segments: {path.pathSegments.Count}");

                    EditorGUI.indentLevel++;
                    foreach (var segment in path.pathSegments)
                    {
                        string templateName = "";
                        foreach (var template in selectedNetwork.templates)
                        {
                            if (template.waypointPaths.Contains(segment))
                            {
                                templateName = template.name;
                                break;
                            }
                        }

                        string sizeInfo = segment.assignedCableSize.HasValue ?
                            $" ({segment.assignedCableSize.Value})" : "";

                        EditorGUILayout.LabelField($"{templateName}: {segment.pathId}{sizeInfo}");
                    }
                    EditorGUI.indentLevel--;

                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Select Path", GUILayout.Width(120)))
                    {
                        selectedPath = path;
                        selectedNetwork.SelectPath(path);
                        SceneView.RepaintAll();
                    }

                    if (GUILayout.Button("Select Entry", GUILayout.Width(120)))
                    {
                        Selection.activeGameObject = path.entry.template.gameObject;
                        SceneView.lastActiveSceneView.FrameSelected();
                    }

                    if (GUILayout.Button("Select Exit", GUILayout.Width(120)))
                    {
                        Selection.activeGameObject = path.exit.template.gameObject;
                        SceneView.lastActiveSceneView.FrameSelected();
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.EndVertical();

                    EditorGUILayout.Space();
                }

                EditorGUI.indentLevel--;

                if (selectedPath != null && GUILayout.Button("Clear Selection", GUILayout.Width(150)))
                {
                    selectedPath = null;
                    selectedNetwork.ClearSelection();
                    SceneView.RepaintAll();
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(selectedNetwork);
            }
        }

        public void OnSelectionChange()
        {
            if (Selection.activeGameObject != null)
            {
                if (Selection.activeGameObject.TryGetComponent<RoomNetwork>(out var network))
                {
                    selectedNetwork = network;
                    Repaint();
                }
            }
        }
    }

    // Add a menu item to select the network component from the GameObject
    [CustomEditor(typeof(RoomNetwork))]
    public class RoomNetworkInspectorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Room Network Editor", GUILayout.Height(30)))
            {
                RoomNetworkEditorWindow window = EditorWindow.GetWindow<RoomNetworkEditorWindow>("Room Network Editor");
                window.Show();
                window.OnSelectionChange();
            }

            EditorGUILayout.Space();
            DrawDefaultInspector();

            // Add refresh button
            RoomNetwork network = (RoomNetwork)target;
            if (GUILayout.Button("Refresh Connections"))
            {
                network.RefreshConnections();
                SceneView.RepaintAll();
            }
        }
    }
}
#endif