using ProjectUniverse.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Control the planting of plants on soil
/// </summary>
public class FarmPlotController : MonoBehaviour
{
    [SerializeField] private SoilType soiltype;
    private float offsety = 1f;
    public bool isinrange = false;
    [SerializeField] private Transform childPlant;

    public enum SoilType
    {
        Fertile = 0,
        Propitious = 1,
        Difficult = 2,
        Hostile = 3,
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            isinrange = true;
        }
        else 
        {
            isinrange = false; 
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void LookInfoMsg(LookInfo linf)
    {
        string[] data = { soiltype.ToString() };
        //return type of soil
        linf.GetType().GetMethod("LookInfoCallback").Invoke(linf, new[] { data });
    }
}
