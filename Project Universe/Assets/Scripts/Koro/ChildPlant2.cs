using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildPlant2 : MonoBehaviour
{
    private float Plants;
    public GameObject PlantPrefabPropitious;
    public Camera fpsCam;
    public float range = 5f;
    public Quaternion rotation;
    private Vector3 pos;
    public void Plusoneplantinchild()
    {

        Plants += 1;


    }
    private void Update()
    {
        if (Input.GetMouseButtonUp(1) && Plants != 0)
        {
            RaycastHit hit;

            if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range) && hit.transform.gameObject.tag == "Propitious environments")
            {
                GameObject PlantSpawned = Instantiate(PlantPrefabPropitious, hit.point, rotation);
                pos = transform.position;
                pos.y += 1;
                Plants -= 1;
            }

        }
    }
}
