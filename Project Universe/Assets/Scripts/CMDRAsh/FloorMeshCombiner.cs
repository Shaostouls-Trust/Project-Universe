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
        //for(int i = 0; i < transform.childCount; i++)
        //{
            MeshCombine();//this.transform.GetChild(i).gameObject
        //}
    }

    private void MeshCombine()//GameObject obj
    {
        //Inverts the whole thing, removes materials and colliders. :(

        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();//obj
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        int i = 1;
        while (i < meshFilters.Length)
        {
            //Vector3 position = meshFilters[i].transform.localPosition;
            //meshFilters[i].transform.localPosition = Vector3.zero;

            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);
           // meshFilters[i].transform.localPosition = position;
            i++;
        }
        Debug.Log("MeshCombine: " + i);
        //all obj
        transform.GetComponent<MeshFilter>().mesh = new Mesh();
        transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine,true,true);
        transform.gameObject.SetActive(true);
        //transform.position = position;
    }
}
