using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FloorMeshCombiner))]
public class FloorMeshCombinerEditor : Editor
{
    
    void OnSceneGUI()
    {
        FloorMeshCombiner fmc = target as FloorMeshCombiner;
        if (Handles.Button(fmc.transform.position+new Vector3(0,1,0),Quaternion.LookRotation(Vector3.up),0.5f,1f,Handles.CubeHandleCap))
        {
            //fmc.CombineMeshes();
            //fmc.CombineMeshesAdv();
            fmc.RestoreOriginalTiles();
        }
    }
}
