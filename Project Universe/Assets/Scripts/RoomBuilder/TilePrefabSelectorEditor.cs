#if (UNITY_EDITOR)
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TilePrefabSelector))]
public class TilePrefabSelectorEditor : Editor
{
    private TilePrefabSelector tilePrefabSelector;
    private int selectedPrefabIndex;

    private void OnEnable()
    {
        tilePrefabSelector = (TilePrefabSelector)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Apply Prefab"))
        {
            tilePrefabSelector.UpdatePrefab();
            EditorUtility.SetDirty(tilePrefabSelector);
        }
    }
}
#endif