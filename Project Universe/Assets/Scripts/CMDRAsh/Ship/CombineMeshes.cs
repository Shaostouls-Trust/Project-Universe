using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class CombineMeshes : MonoBehaviour
{
    //public MeshFilter[] typeList = new MeshFilter[1];
    [Tooltip("All meshes parented to this object are combined.")]
    public GameObject parent;
    public MeshFilter mergedReturn;
    public MeshRenderer parentRenderer;
    [Tooltip("For non-standard scaling for parent at global level, invert the parent scale to cancel the global scale, or use the GSC below.")]
    //public Vector3 globalScaleCorrection = new Vector3(1f, 1f, 1f);

    public bool merge;

    Mesh CombineMeshesFiltered(MeshFilter[] meshes)
    {
        Vector3 oldPos = transform.position;
        Quaternion oldRot = transform.rotation;
        
        transform.rotation = Quaternion.identity;
        transform.position = Vector3.zero;
        List<Material> materials = new List<Material>();

        // Key: shared mesh instance ID, Value: arguments to combine meshes
        var helper = new Dictionary<int, List<CombineInstance>>();
        var helper2 = new Dictionary<Material, List<CombineInstance>>();

        // Build combine instances for each type of mesh
        foreach (var m in meshes)
        {
            List<CombineInstance> tmp;
            //if (!helper.TryGetValue(m.sharedMesh.GetInstanceID(), out tmp))
            //{
            //    tmp = new List<CombineInstance>();
            //    helper.Add(m.sharedMesh.GetInstanceID(), tmp);
            //    Debug.Log("!helper: "+m.sharedMesh.name);
            //}

            Material mat = m.gameObject.GetComponent<MeshRenderer>().sharedMaterial;
            if (mat != null)
            {
                if (!helper2.TryGetValue(mat, out tmp))
                {
                    tmp = new List<CombineInstance>();
                    helper2.Add(mat, tmp);
                    materials.Add(mat);
                    Debug.Log("!helper2: " + mat);
                }

                var ci = new CombineInstance();
                ci.mesh = m.sharedMesh;
                ci.transform = m.transform.localToWorldMatrix;
                Debug.Log("ci34: " + ci.mesh.name);
                tmp.Add(ci);
            }
            else
            {
                Debug.Log("Fault: " + m.gameObject);
            }
        }

        // Combine meshes and build combine instance for combined meshes
        var list = new List<CombineInstance>();
        foreach (var e in helper2)
        {
            var m = new Mesh();
            m.CombineMeshes(e.Value.ToArray());
            var ci = new CombineInstance();
            ci.mesh = m;
            //Debug.Log("ci46: " + ci.mesh);
            list.Add(ci);
        }

        // And now combine everything
        var result = new Mesh();
        Debug.Log("list: " + list.Count);
        result.CombineMeshes(list.ToArray(), false, false);
        result.name = "combinedRoom";

        transform.rotation = oldRot;
        transform.position = oldPos;
        //transform.localScale = globalScaleCorrection; //needs 1,1,1?
        parentRenderer.sharedMaterials = materials.ToArray();

        // It is a good idea to clean unused meshes now
        //foreach (var m in list)
        foreach(var m in meshes)
        {
            m.gameObject.GetComponent<MeshRenderer>().enabled = false;
            //Destroy(m.mesh);
        }

        return result;

    }

    /// <summary>
    /// Get all meshes to be combined and sort into an array.
    /// Pass to combiner. Final mesh is the returned result.
    /// </summary>
    public void Generate()
    {
        if(parent == null)
        {
            parent = transform.gameObject;
        }
        if (mergedReturn == null)
        {
            mergedReturn = transform.GetComponent<MeshFilter>();
        }
        if (parentRenderer == null)
        {
            parentRenderer = transform.GetComponent<MeshRenderer>();
        }
        //MeshFilter[] meshfilters = 
        List<MeshFilter> meshFilters = new List<MeshFilter>();
        parent.GetComponentsInChildren<MeshFilter>(false, meshFilters);
        //scrub self. only include children filters
        for(int i = meshFilters.Count-1; i >= 0; i--)
        {
            //Debug.Log(meshFilters[i]);
            if (meshFilters[i] == mergedReturn)
            {
                meshFilters.RemoveAt(i);
            }
        }
        //only include LOD0
        //modify to work with LODs? Display different meshes by LOD? of would that be too many combinedmeshes?
        //are combined meshes runtime or are they serialized?
        /*for (int i = meshFilters.Count - 1; i >= 0; i--)
        {
            //Debug.Log(meshFilters[i]);
            string[] splitLen = meshFilters[i].name.Split('_');
            if (splitLen[splitLen.Length-1] != "LOD0")
            {
                meshFilters.RemoveAt(i);
            }
        }*/
        Debug.Log(meshFilters.Count);
        Mesh final = CombineMeshesFiltered(meshFilters.ToArray());
        mergedReturn.mesh = final;
    }

    public void Update()
    {
        if (merge)
        {
            merge = false;
            //Debug.Log("");
            Generate();
        }
    }
}
