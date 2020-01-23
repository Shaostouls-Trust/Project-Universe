using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (MeshGeneration))]
public class ProceduralCylinderGen : Editor
{
    public bool hasCollider = false;
    MeshCollider collider = null;
    static MeshGeneration meshReference = null;
    static MeshFilter meshFilter;
    static GameObject gameObject = null;
    [MenuItem ("GameObject/3D Object/Procedural/Cylinder")]

    static void Create()
    {
        GameObject gameObject = new GameObject("Procedural Mesh( Cylinder )");
        MeshGeneration c = gameObject.AddComponent<MeshGeneration>();
        meshReference = gameObject.GetComponent<MeshGeneration>();
        gameObject.AddComponent<MeshFilter>();
        meshFilter = gameObject.GetComponent<MeshFilter>();
        
        c.Rebuild();
        c.AssignDefaultShader();
    }

    public override void OnInspectorGUI()
    {
        MeshGeneration obj;
        obj = target as MeshGeneration;
        if (obj == null)
        {
            return;
        }

        base.DrawDefaultInspector();
        
        if (GUI.changed)
        {
            obj.Rebuild();
            if (meshReference.hasCollider)
            {
                meshReference.GenerateCollider(true , 0);
            }
            if (!meshReference.hasCollider)
            {
                meshReference.GenerateCollider(false , 0);
            }
        }
    }

}
