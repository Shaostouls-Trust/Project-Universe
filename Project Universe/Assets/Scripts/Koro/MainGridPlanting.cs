using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainGridPlanting : MonoBehaviour
{

    public GameObject camerathreed;
    public GameObject virtualscreen;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

   public void OnPress3D()
    {

        camerathreed.SetActive(true);
        virtualscreen.SetActive(true);
    }
   public void OnPress2D()
    {
        camerathreed.SetActive(false);
        virtualscreen.SetActive(false);


    }
}
