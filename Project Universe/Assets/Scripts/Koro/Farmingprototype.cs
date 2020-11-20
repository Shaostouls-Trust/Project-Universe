using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Farmingprototype : MonoBehaviour
{
    public float TimerAmmount = 10f;
    private float timer;
    public GameObject plant;
    private Vector3 DirtPos;
    private float offsety = 1f;
    private bool CanPlant = true;
    private bool cantakeplant = false;
    public GameObject PlantUI;
    public GameObject plantchild;

    

    void Start()
    {
        timer = TimerAmmount;
        DirtPos = transform.position;
        DirtPos.y += offsety;
    }


    void Update()
    {
        //if (Input.GetKey(KeyCode.F))
        //{
        //    timer -= Time.deltaTime;
        //}
        if (timer <= 0 && CanPlant)
        {

            FinishFarming();
        }
        if (Input.GetKeyUp(KeyCode.E))
        {
            if (cantakeplant)
            {
                if (plantchild.active == false)
                {
                    plantchild.SetActive(true);
                    plantchild.SendMessage("Plusoneplantinchild");
                    plant.SetActive(true);
                }
                else
                {
                    
                    plantchild.SendMessage("Plusoneplantinchild");
                    plant.SetActive(true);

                }
            }
        } 
    }

    private void FinishFarming()
    {
        GameObject PlantSpawned = Instantiate(plant, DirtPos, transform.rotation);
        
        //PlantUI.SetActive(true);
        cantakeplant = true;
        CanPlant = false;
    }
}
