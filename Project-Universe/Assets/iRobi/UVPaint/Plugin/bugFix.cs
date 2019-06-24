using UnityEngine;
using System.Collections;

public class bugFix : MonoBehaviour {

#if UNITY_ANDROID
    void OnApplicationPause(bool paused) {
        Object[] objects = GameObject.FindObjectsOfType(typeof(SkinnedMeshRenderer));
        foreach(SkinnedMeshRenderer s in objects) {
            s.sharedMesh.vertices = s.sharedMesh.vertices;
            s.sharedMesh.colors = s.sharedMesh.colors;
            s.sharedMesh.colors32 = s.sharedMesh.colors32;
            s.sharedMesh.uv = s.sharedMesh.uv;
        }
    }
#endif
}
