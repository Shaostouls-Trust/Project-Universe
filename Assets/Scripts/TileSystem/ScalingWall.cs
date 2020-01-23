using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScalingWall : MonoBehaviour
{

 
    public GameObject Wall;
    public Slider slid;
    public Vector3 Scale;
    public Vector3 Offset;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        Scale = new Vector3(1, 3, (1 * slid.value) / 5);
        Wall.transform.localScale = Scale;
        Wall.transform.localPosition = new Vector3(0, 1, (slid.value * 0.1f)-0.5f);

   


    }
}
