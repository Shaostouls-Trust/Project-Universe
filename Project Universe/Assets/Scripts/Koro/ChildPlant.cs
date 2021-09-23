using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildPlant : MonoBehaviour
{
    private float Plants;
    public GameObject PlantPrefab;
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
        if (Input.GetMouseButton(1) && Plants != 0)
        {
            RaycastHit hit;
            if(Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range ) && hit.transform.gameObject.tag == "soil")
            {
                GameObject PlantSpawned = Instantiate(PlantPrefab,hit.point,rotation);
                pos = transform.position;
                pos.y += 1;
            }
            Plants -= 1;
        }
    }
}
