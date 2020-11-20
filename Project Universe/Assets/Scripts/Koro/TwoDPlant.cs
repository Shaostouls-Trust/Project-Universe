using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoDPlant : MonoBehaviour
{
    public float waterAmount;
    private float water;
    public Animator animation;
    public GameObject slot;
   
    private void Update()
    {
        water = waterAmount;

    }
    public void OnPressGrow()
    {
        animation.enabled = true;
       
    }
    public void finishedgrowing()
    {


        slot.SetActive(false);
    }
}
