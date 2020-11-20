using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Grow : MonoBehaviour
{
    
    public float scalemultiplier = 0.02f;
    private Vector3 scale;
    public float lockscale = 2.5f;
    private bool canscale = true;
    public float DefaultScale = 1f;
    private void Start()
    {
        scale.y = DefaultScale;
    }
    private void Update()
    {
        if (canscale)
        {         
            transform.localScale = scale;
            scale.y += scalemultiplier;
            Debug.Log(scale.y);
        }
        
        if (scale.y >= lockscale)
        {

            scale.y = lockscale;
            canscale = false;

        }

    }
}
