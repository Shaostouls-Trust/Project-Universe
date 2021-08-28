using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildPlant3 : MonoBehaviour
{
    private float Plants;
    public GameObject PlantPrefabHostile;
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

            if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range) && hit.transform.gameObject.tag == "Hostile environments")
            {
                GameObject PlantSpawned = Instantiate(PlantPrefabHostile, hit.point, rotation);
                pos = transform.position;
                pos.y += 1;
                Plants -= 1;
            }

        }
    }
}
