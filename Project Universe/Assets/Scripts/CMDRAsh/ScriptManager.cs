using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptManager : MonoBehaviour
{
    private GameObject[] doors;
    private GameObject[] lights;

    // Start is called before the first frame update
    void Start()
    {
        //get all 'door' objects in scene
        //doors = GameObject.FindGameObjectsWithTag("Door");
        //lights = GameObject.FindGameObjectsWithTag("LightObject");
    }
    //Apparently Unity automatically does this bit. 
    /*
   // Update is called once per frame
   void Update()
   {

       foreach(GameObject door in doors)
       {
           if (door.GetComponent<Renderer>().enabled)
           {
               door.GetComponent<IMachine>().enabled = false;
               door.GetComponent<DoorAnimator>().enabled = false;
           }
           else
           {
               door.GetComponent<IMachine>().enabled = true;
               door.GetComponent<DoorAnimator>().enabled = true;
           }
       }
       
    }
*/
}
