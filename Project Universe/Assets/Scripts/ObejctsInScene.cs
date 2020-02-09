using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObejctsInScene : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //count them
        int numOfObjects = GameObject.FindGameObjectsWithTag("MallBasicTile").Length;
        print(numOfObjects);
        //disable colliders
        GameObject[] listOfObjects = GameObject.FindGameObjectsWithTag("MallBasicTile");
        foreach(GameObject obj in listOfObjects)
        {
            if (obj.GetComponent<Collider>() != null)
            {
        //        obj.GetComponent<Collider>().enabled = false;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
