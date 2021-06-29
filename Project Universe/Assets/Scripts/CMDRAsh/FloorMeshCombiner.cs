using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class FloorMeshCombiner : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void CheckForChildrenRenderers(GameObject parent, ref List<MeshRenderer> meshRenderers)
    {
        if(parent.transform.childCount == 0)
        {
            return;
        }

        MeshRenderer renderInst;
        for (int r = 0; r < parent.transform.childCount; r++)
        {
            if (!parent.transform.GetChild(r).TryGetComponent(out renderInst))
            {
                //if there were no MeshRenderers in this object, check it's children for submeshes
                CheckForChildrenRenderers(parent.transform.GetChild(r).gameObject, ref meshRenderers);
            }
            else
            {
                meshRenderers.Add(renderInst);
                CheckForChildrenRenderers(parent.transform.GetChild(r).gameObject, ref meshRenderers);
            }
        }
    }

    private void CheckForChildrenFilters(GameObject parent, ref List<MeshFilter> meshFilters)
    {
        if (parent.transform.childCount == 0)
        {
            return;
        }

        MeshFilter filterInst;
        for (int r = 0; r < parent.transform.childCount; r++)
        {
            if (!parent.transform.GetChild(r).TryGetComponent(out filterInst))
            {
                //if there were no MeshRenderers in this object, check it's children for submeshes
                CheckForChildrenFilters(parent.transform.GetChild(r).gameObject, ref meshFilters);
            }
            else
            {
                //if there were, look for more anyway
                meshFilters.Add(filterInst);
                CheckForChildrenFilters(parent.transform.GetChild(r).gameObject, ref meshFilters);
            }
        }
    }

    public void CombineMeshesAdv()
    {
        //get all child meshes,materials,renderers
        MeshFilter[] filters = GetComponentsInChildren<MeshFilter>(false);
        List<Material> materials = new List<Material>();
        //MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>(false);
        List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
        List<MeshFilter> meshFilters = new List<MeshFilter>();

        CheckForChildrenFilters(gameObject, ref meshFilters);
        //get renderer on every submesh (this is a recursive func)
        CheckForChildrenRenderers(gameObject, ref meshRenderers);
        Debug.Log("---");
        Debug.Log(meshFilters.Count);
        Debug.Log(meshRenderers.Count);

        //Get every material, which will be used to determine which meshes will be combined
        foreach (MeshRenderer renderer in meshRenderers)
        {
            if (renderer.transform != transform)
            {
                Material[] localMats = renderer.sharedMaterials;
                foreach (Material localMat in localMats)
                {
                    if (!materials.Contains(localMat))
                    {
                        materials.Add(localMat);
                    }
                }
            }               
        }
        //Debug.Log(materials.Count);
        //create a list of submeshes for each material
        List<Mesh> submeshes = new List<Mesh>();
        foreach(Material material in materials)
        {
            //create a combine instance for each material
            List<CombineInstance> combiners = new List<CombineInstance>();
            for (int f = 0; f < filters.Length; f++)
            {
                MeshRenderer renderer = filters[f].GetComponent<MeshRenderer>();
                if (filters[f].transform == transform) continue;
                //if (renderer != null) continue;//meshRenderers[f]
                Material[] localMaterials = renderer.sharedMaterials;//meshRenderers[f]
                Debug.Log("Localmaterials length:" + localMaterials.Length);
                for (int matIndex = 0; matIndex < localMaterials.Length; matIndex++)
                {
                    if (localMaterials[matIndex] != material)//!=
                    {
                        CombineInstance ci = new CombineInstance();
                        ci.mesh = filters[f].sharedMesh;
                        ci.subMeshIndex = matIndex;
                        ci.transform = filters[f].transform.localToWorldMatrix;
                        combiners.Add(ci);
                        Debug.Log("Combining mat "+matIndex);
                    }               
                }
            }
            // Flatten into a single mesh.
            Mesh mesh = new Mesh();
            mesh.CombineMeshes(combiners.ToArray(), true);
            submeshes.Add(mesh);
        }

        //final mesh
        List<CombineInstance> finalCombiners = new List<CombineInstance>();
        foreach (Mesh mesh in submeshes)
        {
            CombineInstance ci = new CombineInstance();
            ci.mesh = mesh;
            ci.subMeshIndex = 0;
            ci.transform = Matrix4x4.identity;
            finalCombiners.Add(ci);
        }
        Mesh finalMesh = new Mesh();
        finalMesh.CombineMeshes(finalCombiners.ToArray(), false);
        GetComponent<MeshFilter>().sharedMesh = finalMesh;
        Debug.Log("Final mesh has " + submeshes.Count + " submeshes/mats.");

        foreach(MeshRenderer renderer in meshRenderers)
        {
            renderer.enabled = false;
        }
    }

    public void CombineMeshes()
    {
        MeshFilter[] filters = GetComponentsInChildren<MeshFilter>(false);
        Debug.Log("I should be combining " + name + "'s " + filters.Length + " meshes");
        Mesh finalMesh = new Mesh();
        //Material floorMaterial = gameObject.GetComponent<Material>();

        Vector3 oldPos = transform.position;
        Quaternion oldRot = transform.rotation;
        transform.rotation = Quaternion.identity;
        transform.position = Vector3.zero;

        CombineInstance[] combiner = new CombineInstance[filters.Length];
        for(int i = 0; i < filters.Length; i++)
        {
            if (filters[i].transform == transform) continue;
            combiner[i].subMeshIndex = 0;
            combiner[i].mesh = filters[i].sharedMesh;
            combiner[i].transform = filters[i].transform.localToWorldMatrix;
        }
        finalMesh.CombineMeshes(combiner);
        GetComponent<MeshFilter>().sharedMesh = finalMesh;

        transform.rotation = oldRot;
        transform.position = oldPos;

        //disable all meshes
        List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
        CheckForChildrenRenderers(gameObject, ref meshRenderers);
        for (int j = 0; j < meshRenderers.Count; j++)
        {
            meshRenderers[j].enabled = false;
        }
    }

    public void RestoreOriginalTiles()
    {
        try
        {
            GetComponent<MeshFilter>().sharedMesh.Clear();
        }
        catch (Exception) { }

        List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
        CheckForChildrenRenderers(gameObject, ref meshRenderers);
        for (int j = 0; j < meshRenderers.Count; j++)
        {
            meshRenderers[j].enabled = true;
        }
    }
}
