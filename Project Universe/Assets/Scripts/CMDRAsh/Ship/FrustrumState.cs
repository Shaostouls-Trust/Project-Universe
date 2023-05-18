using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class FrustrumState : MonoBehaviour
{
    public bool visibleInFrustrum = false;
    public bool hidByOccluder = false;
    [SerializeField] private MeshCollider[] renderStatePlanes = new MeshCollider[0];

    public bool HidByOccluder
    {
        get { return hidByOccluder; }
        set { hidByOccluder = value; }
    }

    private void OnBecameVisible()
    {
        visibleInFrustrum = true;
    }

    //Also triggered when renderer is manually hidden
    private void OnBecameInvisible()
    {
        //check if we were hid by occluders or not
        if (!hidByOccluder)
        {
            visibleInFrustrum = false;
        }
        else
        {
            visibleInFrustrum = true;
        }
    }

    public MeshCollider[] RenderStatePlanes
    {
        get { return renderStatePlanes; }
    }

    public void AddStatePlate(MeshCollider col)
    {
        bool add = true;
        for(int r = 0; r< renderStatePlanes.Length; r++)
        {
            if (renderStatePlanes[r] == col)
            {
                add = false;
                break;
            }
        }
        if (add)
        {
            Debug.Log("Add " + col);
            MeshCollider[] expansion = new MeshCollider[renderStatePlanes.Length+1];
            renderStatePlanes.CopyTo(expansion, 0);
            expansion[renderStatePlanes.Length] = col;
            renderStatePlanes = expansion;
        }
    }
}
