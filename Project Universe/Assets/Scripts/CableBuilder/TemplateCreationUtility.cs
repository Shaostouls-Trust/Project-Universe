#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ProjectUniverse.PowerSystem.Editor
{
    public class TemplateEditorWindow : EditorWindow
    {
        private Template selectedTemplate;
        private WaypointPath selectedPath;
        private int selectedPathIndex = -1;
        private Vector2 scrollPosition;
        private bool showPathSettings = true;
        private bool showWaypoints = true;
        private bool showVisualizationSettings = false;

        private Vector3 newWaypointPosition = Vector3.zero;

        [MenuItem("Tools/Cable System/Template Editor")]
        public static void ShowWindow()
        {
            GetWindow<TemplateEditorWindow>("Template Editor");
        }

        private void OnEnable()
        {
            // Restore selection when window opens
            if (selectedTemplate != null && selectedPathIndex >= 0 && selectedPathIndex < selectedTemplate.waypointPaths.Count)
            {
                selectedPath = selectedTemplate.waypointPaths[selectedPathIndex];
                UpdatePathSelection();
            }
        }

        private void OnDisable()
        {
            // Clear path selection when window closes
            if (selectedTemplate != null)
            {
                selectedTemplate.ClearSelectedPath();
                SceneView.RepaintAll();
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            // Template selection
            EditorGUILayout.LabelField("Template Selection", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            Template previousTemplate = selectedTemplate;
            selectedTemplate = EditorGUILayout.ObjectField("Template", selectedTemplate, typeof(Template), true) as Template;

            if (EditorGUI.EndChangeCheck() && previousTemplate != selectedTemplate)
            {
                // Clear previous template selection
                if (previousTemplate != null)
                {
                    previousTemplate.ClearSelectedPath();
                }

                // Update to new template
                if (selectedTemplate != null)
                {
                    selectedPathIndex = selectedTemplate.waypointPaths.Count > 0 ? 0 : -1;
                    selectedPath = selectedPathIndex >= 0 ? selectedTemplate.waypointPaths[selectedPathIndex] : null;
                    UpdatePathSelection();
                }
            }

            if (selectedTemplate == null)
            {
                EditorGUILayout.HelpBox("Select a Template object to edit", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // Template properties
            EditorGUILayout.LabelField("Template Configuration", EditorStyles.boldLabel);
            Undo.RecordObject(selectedTemplate, "Edit Template");

            selectedTemplate.templateId = EditorGUILayout.TextField("Template ID", selectedTemplate.templateId);

            EditorGUI.BeginChangeCheck();
            Template.TemplateType newType = (Template.TemplateType)EditorGUILayout.EnumPopup("Template Type", selectedTemplate.templateType);
            if (EditorGUI.EndChangeCheck() && newType != selectedTemplate.templateType)
            {
                selectedTemplate.templateType = newType;
                selectedTemplate.UpdatePathSupportedCableSizes();
            }

            // Display supported cable sizes based on template type
            EditorGUILayout.LabelField("Supported Cable Sizes:", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            foreach (var size in selectedTemplate.GetSupportedCableSizes())
            {
                EditorGUILayout.LabelField("• " + size.ToString());
            }
            EditorGUI.indentLevel--;

            selectedTemplate.maxCableCapacity = EditorGUILayout.IntField("Max Cable Capacity", selectedTemplate.maxCableCapacity);
            selectedTemplate.currentCableCount = EditorGUILayout.IntField("Current Cable Count", selectedTemplate.currentCableCount);

            EditorGUILayout.Space();

            // Path management
            EditorGUILayout.LabelField("Waypoint Paths", EditorStyles.boldLabel);

            if (selectedTemplate.waypointPaths.Count == 0)
            {
                EditorGUILayout.HelpBox("No paths defined. Create a new path to get started.", MessageType.Info);
            }
            else
            {
                string[] pathNames = new string[selectedTemplate.waypointPaths.Count];
                for (int i = 0; i < pathNames.Length; i++)
                {
                    string assignedInfo = selectedTemplate.waypointPaths[i].assignedCableSize.HasValue ?
                        $" ({selectedTemplate.waypointPaths[i].assignedCableSize.Value})" : "";
                    pathNames[i] = selectedTemplate.waypointPaths[i].pathId + assignedInfo;
                }

                EditorGUI.BeginChangeCheck();
                selectedPathIndex = EditorGUILayout.Popup("Select Path", selectedPathIndex, pathNames);

                if (EditorGUI.EndChangeCheck())
                {
                    if (selectedPathIndex >= 0 && selectedPathIndex < selectedTemplate.waypointPaths.Count)
                    {
                        selectedPath = selectedTemplate.waypointPaths[selectedPathIndex];
                        UpdatePathSelection();
                    }
                }

                if (selectedPathIndex >= 0 && selectedPathIndex < selectedTemplate.waypointPaths.Count)
                {
                    selectedPath = selectedTemplate.waypointPaths[selectedPathIndex];
                    DrawPathEditor();
                }
            }

            if (GUILayout.Button("Create New Path"))
            {
                CreateNewPath();
            }

            if (selectedPath != null && GUILayout.Button("Delete Selected Path"))
            {
                DeleteSelectedPath();
            }

            EditorGUILayout.Space();

            // Visualization settings
            showVisualizationSettings = EditorGUILayout.Foldout(showVisualizationSettings, "Visualization Settings");
            if (showVisualizationSettings)
            {
                selectedTemplate.showGizmos = EditorGUILayout.Toggle("Show Gizmos", selectedTemplate.showGizmos);
                selectedTemplate.entryPointColor = EditorGUILayout.ColorField("Entry Point Color", selectedTemplate.entryPointColor);
                selectedTemplate.exitPointColor = EditorGUILayout.ColorField("Exit Point Color", selectedTemplate.exitPointColor);
                selectedTemplate.waypointColor = EditorGUILayout.ColorField("Waypoint Color", selectedTemplate.waypointColor);
                selectedTemplate.pathColor = EditorGUILayout.ColorField("Path Color", selectedTemplate.pathColor);
                selectedTemplate.assignedPathColor = EditorGUILayout.ColorField("Assigned Path Color", selectedTemplate.assignedPathColor);
            }

            EditorGUI.EndChangeCheck();

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(selectedTemplate);
                SceneView.RepaintAll();
            }
        }

        private void UpdatePathSelection()
        {
            if (selectedTemplate != null)
            {
                selectedTemplate.SetSelectedPath(selectedPath);
                SceneView.RepaintAll();
            }
        }

        private void DrawPathEditor()
        {
            showPathSettings = EditorGUILayout.Foldout(showPathSettings, "Path Settings");
            if (showPathSettings)
            {
                selectedPath.pathId = EditorGUILayout.TextField("Path ID", selectedPath.pathId);

                EditorGUILayout.LabelField("Cable Size Configuration", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                // Display all available sizes from the template
                CableSize[] templateSupportedSizes = selectedTemplate.GetSupportedCableSizes();

                // If the path has an assigned cable size, show it prominently
                if (selectedPath.assignedCableSize.HasValue)
                {
                    GUIStyle highlightStyle = new GUIStyle(EditorStyles.boldLabel);
                    highlightStyle.normal.textColor = new Color(0.5f, 1f, 0.5f); // Light green
                    EditorGUILayout.LabelField("Assigned Cable Size: " + selectedPath.assignedCableSize.Value.ToString(), highlightStyle);

                    if (GUILayout.Button("Unassign Cable Size"))
                    {
                        selectedPath.UnassignCableSize();
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("No Cable Size Assigned", EditorStyles.miniLabel);
                    EditorGUILayout.Space();

                    // Show options for assigning a cable size
                    EditorGUILayout.LabelField("Available Cable Sizes:");
                    foreach (CableSize size in templateSupportedSizes)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(size.ToString(), GUILayout.Width(100));

                        if (GUILayout.Button("Assign", GUILayout.Width(60)))
                        {
                            selectedPath.AssignCableSize(size);
                        }

                        EditorGUILayout.EndHorizontal();
                    }
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space();

            // Entry point
            EditorGUILayout.LabelField("Entry Point", EditorStyles.boldLabel);
            selectedPath.entryPoint.position = EditorGUILayout.Vector3Field("Position", selectedPath.entryPoint.position);

            EditorGUILayout.Space();

            // Waypoints
            showWaypoints = EditorGUILayout.Foldout(showWaypoints, $"Waypoints ({selectedPath.waypoints.Count})");
            if (showWaypoints)
            {
                for (int i = 0; i < selectedPath.waypoints.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();

                    // Up/Down buttons for reordering
                    EditorGUI.BeginDisabledGroup(i == 0);
                    if (GUILayout.Button("↑", GUILayout.Width(25)))
                    {
                        selectedPath.MoveWaypointUp(i);
                    }
                    EditorGUI.EndDisabledGroup();

                    EditorGUI.BeginDisabledGroup(i == selectedPath.waypoints.Count - 1);
                    if (GUILayout.Button("↓", GUILayout.Width(25)))
                    {
                        selectedPath.MoveWaypointDown(i);
                    }
                    EditorGUI.EndDisabledGroup();

                    EditorGUILayout.LabelField($"Waypoint {i + 1}", GUILayout.Width(80));
                    selectedPath.waypoints[i].position = EditorGUILayout.Vector3Field("", selectedPath.waypoints[i].position);

                    if (GUILayout.Button("X", GUILayout.Width(25)))
                    {
                        selectedPath.waypoints.RemoveAt(i);
                        break;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                // Add new waypoint
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Add New Waypoint", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                newWaypointPosition = EditorGUILayout.Vector3Field("Position", newWaypointPosition);

                if (GUILayout.Button("Add", GUILayout.Width(50)))
                {
                    selectedPath.AddWaypoint(newWaypointPosition);
                    newWaypointPosition = Vector3.zero;
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();

            // Exit point
            EditorGUILayout.LabelField("Exit Point", EditorStyles.boldLabel);
            selectedPath.exitPoint.position = EditorGUILayout.Vector3Field("Position", selectedPath.exitPoint.position);
        }

        private void CreateNewPath()
        {
            string pathId = "path_" + System.Guid.NewGuid().ToString().Substring(0, 8);
            WaypointPath newPath = new WaypointPath(pathId, selectedTemplate.GetSupportedCableSizes());

            selectedTemplate.waypointPaths.Add(newPath);
            selectedPathIndex = selectedTemplate.waypointPaths.Count - 1;
            selectedPath = newPath;
            UpdatePathSelection();

            EditorUtility.SetDirty(selectedTemplate);
        }       

        private void DeleteSelectedPath()
        {
            if (selectedPathIndex >= 0 && selectedPathIndex < selectedTemplate.waypointPaths.Count)
            {
                selectedTemplate.waypointPaths.RemoveAt(selectedPathIndex);
                selectedPathIndex = Mathf.Clamp(selectedPathIndex - 1, -1, selectedTemplate.waypointPaths.Count - 1);
                selectedPath = selectedPathIndex >= 0 ? selectedTemplate.waypointPaths[selectedPathIndex] : null;
                UpdatePathSelection();

                EditorUtility.SetDirty(selectedTemplate);
            }
        }

        public void OnSelectionChange()
        {
            if (Selection.activeGameObject != null)
            {
                Template template = Selection.activeGameObject.GetComponent<Template>();
                if (template != null)
                {
                    // Clear previous selection
                    if (selectedTemplate != null && selectedTemplate != template)
                    {
                        selectedTemplate.ClearSelectedPath();
                    }

                    selectedTemplate = template;

                    // Find path with assigned cable size or default to first
                    selectedPathIndex = -1;
                    for (int i = 0; i < template.waypointPaths.Count; i++)
                    {
                        if (template.waypointPaths[i].assignedCableSize.HasValue)
                        {
                            selectedPathIndex = i;
                            break;
                        }
                    }

                    if (selectedPathIndex == -1 && template.waypointPaths.Count > 0)
                    {
                        selectedPathIndex = 0;
                    }

                    selectedPath = selectedPathIndex >= 0 ? template.waypointPaths[selectedPathIndex] : null;
                    UpdatePathSelection();
                    Repaint();
                }
            }
        }

        private void OnDestroy()
        {
            // Ensure selection is cleared when window is destroyed
            if (selectedTemplate != null)
            {
                selectedTemplate.ClearSelectedPath();
                SceneView.RepaintAll();
            }
        }
    }

    // Add a menu item to select the template component from the GameObject
    [CustomEditor(typeof(Template))]
    public class TemplateInspectorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Template Editor", GUILayout.Height(30)))
            {
                TemplateEditorWindow window = EditorWindow.GetWindow<TemplateEditorWindow>("Template Editor");
                window.Show();
                window.OnSelectionChange();
            }

            EditorGUILayout.Space();
            DrawDefaultInspector();
        }
    }
}
#endif