using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Reflection;
[RequireComponent(typeof(Light))]
[ExecuteInEditMode]
public class MFFlareLauncher : MonoBehaviour
{
    public bool directionalLight;
    public bool useLightIntensity;
    public MFFlareAsset assetModel;
    [HideInInspector]public Light lightSource;
    // [HideInInspector]public Texture2D tex;
    private void OnEnable()
    {
        lightSource = GetComponent<Light>();
        // Add self to awake function: AddLight in URPLensFlare.cs on camera in render;
        bool lightIncluded = false;
        for(int i=0;i< Camera.main.GetComponent<MFLensFlare>().lightSource.Count; i++)
        {
            if(this == Camera.main.GetComponent<MFLensFlare>().lightSource[i])
            {
                lightIncluded = true;
            }
        }
        if (!lightIncluded)
        {
            Camera.main.GetComponent<MFLensFlare>().AddLight(this);
        }
    }
    
    private void Reset()
    {
        lightSource = GetComponent<Light>();
    }

    private void OnDestroy()
    {
        // Add self to awake function: RemoveLight in URPLensFlare.cs on camera in render;
        //Camera.main.GetComponent<URPLensFlare>().RemoveLight(this);
    }
    
}
