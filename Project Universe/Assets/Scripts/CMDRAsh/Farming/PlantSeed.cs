using MLAPI;
using ProjectUniverse.Base;
using ProjectUniverse.Items.Consumable;
using ProjectUniverse.Player;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantSeed : MonoBehaviour
{
    [SerializeField] private PlantType plantType;
    [SerializeField] private GameObject PlantPrefab;
    private IPlayer_Inventory playerInventory;
    public float range = 5f;
    public Quaternion rotation;
    private int seeds = 1;
    private ItemStack seedStack;

    public enum PlantType
    {
        Corn = 0,
        Carrot = 1,
        Tomato = 2
    }

    private void Start()
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
        {
            playerInventory = networkedClient.PlayerObject.GetComponent<IPlayer_Inventory>();

            Consumable_Produce produceSeeds = new Consumable_Produce(plantType.ToString() + " Seeds", 1);
            ItemStack seedStack = new ItemStack(plantType.ToString() + " Seeds", 999, typeof(Consumable_Produce));
            seedStack.AddItem(produceSeeds);
            Debug.Log("Add 1 seed at start");
            playerInventory.AddStackToPlayerInventory(seedStack);
            //seedStack = seedstack;
        }
    }

    public void RemoveSeedFromPlayerInventory()
    {
        Consumable_Produce produceSeeds = new Consumable_Produce(plantType.ToString() + " Seeds", 1);
        ItemStack seedStack = new ItemStack(plantType.ToString() + " Seeds", 999, typeof(Consumable_Produce));
        seedStack.AddItem(produceSeeds);
        playerInventory.RemoveFromPlayerInventory(seedStack);
        Debug.Log("Remove seed");
        //seedStack.RemoveItemData(1); <-- produces a NULL :(
    }

    // Update is called once per frame
    void Update()
    {
        //if(seedStack == null)
        //{
            List<ItemStack> inv = playerInventory.GetPlayerInventory();
            for (int i = 0; i < inv.Count; i++)
            {
                if (inv[i].GetStackType() == plantType.ToString() + " Seeds")
                {
                    seedStack = inv[i];
                    seeds = inv[i].GetRealLength();
                }
            }
        //}
        //else
        //{
            //Debug.Log(seedStack.GetRealLength());
        //    seeds = seedStack.GetRealLength();
        //}
        if (Input.GetKeyDown(KeyCode.E) && seeds != 0)
        {
            RaycastHit hit;
            Vector3 forward = transform.TransformDirection(0f, 0f, 1f) * 5f;//5m reach
            if (Physics.Raycast(transform.position, forward, out hit, 5f))
            {
                string tag = hit.transform.gameObject.tag;
                if (tag == "Fertile environments" || tag == "Propitious environments" 
                    || tag == "Hostile environments" || tag == "Difficult environments")
                {
                    GameObject PlantSpawned = Instantiate(PlantPrefab, hit.point, rotation);
                    RemoveSeedFromPlayerInventory();
                }
            }
        }
    }
}
