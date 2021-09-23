using Impact.Materials;
using Impact.Objects;
using UnityEditor;
using UnityEngine;

namespace Impact.EditorScripts
{
    [CustomEditor(typeof(ImpactTerrain))]
    public class ImpactTerrainEditor : Editor
    {
        private ImpactTerrain terrain;
        private TerrainLayer[] terrainLayers;

        private void OnEnable()
        {
            terrain = target as ImpactTerrain;

            if (terrain.Terrain == null)
                terrain.Terrain = terrain.GetComponent<Terrain>();

            terrain.SyncTerrainLayersAndMaterialsList();
            terrainLayers = terrain.TerrainData.terrainLayers;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Terrain", EditorStyles.boldLabel);
            terrain.Terrain = EditorGUILayout.ObjectField(new GUIContent("", "The Terrain this object is associated with."), terrain.Terrain, typeof(Terrain), true) as Terrain;

            if (terrain.Terrain == null)
            {
                EditorGUILayout.HelpBox("Assign a Terrain to begin editing Terrain Materials.", MessageType.Info);
            }
            else
            {
                ImpactEditorUtilities.Separator();

                drawTerrainLayersList();
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(terrain);
            }
        }

        private void drawTerrainLayersList()
        {
            EditorGUILayout.LabelField("Terrain Layer Materials", EditorStyles.boldLabel);

            for (int i = 0; i < terrain.TerrainMaterials.Count; i++)
            {
                EditorGUILayout.BeginVertical();

                drawTerrainLayerMaterial(i);

                GUILayout.Space(4);

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Separator();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent("Refresh Terrain Layers", "Manually re-sync the stored materials with the terrain layers.")))
            {
                terrain.SyncTerrainLayersAndMaterialsList();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(2);
        }

        private void drawTerrainLayerMaterial(int index)
        {
            EditorGUILayout.BeginHorizontal();

            TerrainLayer layer = terrainLayers[index];

            if (layer != null)
                GUILayout.Box(layer.diffuseTexture, GUILayout.Height(40), GUILayout.Width(40));
            else
                GUILayout.Box(new GUIContent(), GUILayout.Height(40), GUILayout.Width(40));

            EditorGUILayout.BeginVertical();

            if (layer != null)
                EditorGUILayout.LabelField(layer.diffuseTexture.name);
            else
                EditorGUILayout.LabelField("Missing Terrain Layer");

            terrain.TerrainMaterials[index] = EditorGUILayout.ObjectField("", terrain.TerrainMaterials[index], typeof(ImpactMaterialBase), false) as ImpactMaterialBase;

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }
    }
}