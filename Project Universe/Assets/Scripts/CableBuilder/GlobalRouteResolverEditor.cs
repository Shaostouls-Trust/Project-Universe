#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ProjectUniverse.PowerSystem.Editor
{
    public class GlobalRouteResolverEditorWindow : EditorWindow
    {
        [SerializeField] private GlobalRouteResolver resolver;
        private Vector2 scrollPosition;
        private bool showConnections = true;

        [MenuItem("Tools/Cable System/Global Route Resolver")]
        public static void ShowWindow()
        {
            GetWindow<GlobalRouteResolverEditorWindow>("Global Route Resolver");
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            // Resolver selection
            EditorGUILayout.LabelField("Global Route Resolver", EditorStyles.boldLabel);
            resolver = EditorGUILayout.ObjectField("Resolver", resolver, typeof(GlobalRouteResolver), true) as GlobalRouteResolver;

            if (resolver == null)
            {
                EditorGUILayout.HelpBox("Select or create a Global Route Resolver object", MessageType.Info);

                if (GUILayout.Button("Create Global Route Resolver"))
                {
                    GameObject go = new("Global Route Resolver");
                    resolver = go.AddComponent<GlobalRouteResolver>();
                    Selection.activeObject = go;
                }

                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.Space();

            // Controls
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Discover Room Networks"))
            {
                resolver.DiscoverRoomNetworks();
            }

            if( GUILayout.Button("Discover Power Components"))
            {
                resolver.DiscoverPowerComponents();
            }

            if (GUILayout.Button("Resolve Boundary Connections"))
            {
                resolver.ResolveBoundaryConnections();
                SceneView.RepaintAll();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Power System", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            //A2
            EditorGUILayout.Space();

            if(GUILayout.Button("Detect Power Paths"))
            {
                resolver.DetectAndCreatePowerPaths();
            }
            //B2
            if (GUILayout.Button("Clear All Power Paths"))
            {
                PowerSystemPathManager.Instance.ClearAllConnections();
                Debug.Log("Cleared all power path connections");
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // Configuration
            resolver.boundaryConnectionThreshold = EditorGUILayout.FloatField("Connection Threshold",
                resolver.boundaryConnectionThreshold);

            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // Room Networks
            EditorGUILayout.LabelField($"Room Networks ({resolver.roomNetworks.Count})", EditorStyles.boldLabel);

            foreach (var network in resolver.roomNetworks)
            {
                if (network == null) continue;

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField($"Network: {network.roomId}");

                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    Selection.activeObject = network.gameObject;
                }

                EditorGUILayout.EndHorizontal();

                var connectedPorts = network.GetConnectedBoundaryPorts();
                if (connectedPorts.Count > 0)
                {
                    EditorGUILayout.LabelField($"  Boundary Ports: {connectedPorts.Count} connected", EditorStyles.miniLabel);
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space();

            // Boundary Connections
            showConnections = EditorGUILayout.Foldout(showConnections,
                $"Boundary Connections ({resolver.boundaryConnections.Count})");

            if (showConnections && resolver.boundaryConnections.Count > 0)
            {
                EditorGUI.indentLevel++;

                foreach (var connection in resolver.boundaryConnections)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    GUIStyle statusStyle = new(EditorStyles.label);
                    statusStyle.normal.textColor = connection.isSizeCompatible ?
                        resolver.compatibleConnectionColor : resolver.incompatibleConnectionColor;

                    EditorGUILayout.LabelField(
                        $"{connection.networkA.roomId} [{connection.portA.boundaryName}] ↔ " +
                        $"{connection.networkB.roomId} [{connection.portB.boundaryName}]",
                        statusStyle);

                    EditorGUILayout.LabelField(
                        $"  Status: {(connection.isSizeCompatible ? "Compatible" : "Incompatible")} | " +
                        $"Distance: {Vector3.Distance(connection.portA.GetWorldPosition(), connection.portB.GetWorldPosition()):F2}",
                        EditorStyles.miniLabel);

                    EditorGUILayout.BeginHorizontal();

                    if (GUILayout.Button("Select Port A", GUILayout.Width(100)))
                    {
                        Selection.activeObject = connection.portA.gameObject;
                    }

                    if (GUILayout.Button("Select Port B", GUILayout.Width(100)))
                    {
                        Selection.activeObject = connection.portB.gameObject;
                    }

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.EndVertical();
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            // Visualization
            EditorGUILayout.LabelField("Visualization", EditorStyles.boldLabel);
            resolver.showBoundaryConnections = EditorGUILayout.Toggle("Show Connections", resolver.showBoundaryConnections);
            resolver.compatibleConnectionColor = EditorGUILayout.ColorField("Compatible Color", resolver.compatibleConnectionColor);
            resolver.incompatibleConnectionColor = EditorGUILayout.ColorField("Incompatible Color", resolver.incompatibleConnectionColor);

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(resolver);
                SceneView.RepaintAll();
            }
        }
    }

    [CustomEditor(typeof(GlobalRouteResolver))]
    public class GlobalRouteResolverInspectorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Global Route Resolver", GUILayout.Height(30)))
            {
                GlobalRouteResolverEditorWindow window = EditorWindow.GetWindow<GlobalRouteResolverEditorWindow>();
                window.Show();
            }

            EditorGUILayout.Space();
            DrawDefaultInspector();
        }
    }
}
#endif