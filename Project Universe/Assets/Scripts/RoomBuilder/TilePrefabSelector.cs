using Unity.Transforms;
using UnityEditor;
using UnityEngine;
#if (UNITY_EDITOR)
public class TilePrefabSelector : MonoBehaviour
{
    [SerializeField] private GameObject currentPrefab;
    private GameObject instanceGO;
    [SerializeField] private GridSystem gridSystem;

    public GameObject CurrentPrefab
    {
        get => currentPrefab;
        set => currentPrefab = value;
    }

    public GameObject InstanceObject
    {
        get => instanceGO;
        set => instanceGO = value;
    }

    public GridSystem GridSystem
    {
        get => gridSystem;
        set => gridSystem = value;
    }

    /// <summary>
    /// Updates the current prefab by destroying the existing instance and creating a new one.
    /// </summary>
    public void UpdatePrefab()
    {
        if (currentPrefab == null)
        {
            Debug.LogError("CurrentPrefab is null. Cannot update prefab.");
            return;
        }

        if (instanceGO != null && instanceGO != currentPrefab)
        {
            DestroyImmediate(instanceGO);
        }
        
        if (PrefabUtility.GetPrefabAssetType(currentPrefab) == PrefabAssetType.Regular
            || PrefabUtility.GetPrefabAssetType(currentPrefab) == PrefabAssetType.Variant)
        {
            //get parent
            Transform parent = transform.parent;
            GridSystem.EditingSection section = GridSystem.DetermineSectionForPlacement(parent);
            //
            GameObject newPrefab = Instantiate(currentPrefab, transform.parent);
            Vector3 snappedPosition = gridSystem.SnapTileToGrid(newPrefab, gameObject.transform.position, gameObject.transform.rotation.eulerAngles.y, section);
            
            //Vector3 defaultAngles = newPrefab.transform.localRotation.eulerAngles;
            newPrefab.transform.SetLocalPositionAndRotation(snappedPosition, gameObject.transform.rotation);
            instanceGO = newPrefab;
        }
    }
}
#endif