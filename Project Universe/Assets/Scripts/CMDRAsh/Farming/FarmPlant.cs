using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using ProjectUniverse.Networking;
using ProjectUniverse.Player;
using ProjectUniverse.Base;
using ProjectUniverse.Items.Consumable;
using ProjectUniverse.UI;

public class FarmPlant : MonoBehaviour
{
    [SerializeField] private PlantSeed.PlantType plantType;
    [SerializeField] private float growTime = 9f;
    private float growTimeRemaining;
    [SerializeField] private float ripeTime;
    private float ripeTimeRemaining;
    [SerializeField] private float yields;//how many times can this produce fruit before it dies
    [SerializeField] private int yield;//how many times can this produce fruit before it dies
    private float yieldsRemaining;
    [SerializeField] private GameObject fruit;
    //[SerializeField] private Vector3 FruitPos;
    private bool ripe = false;
    private bool grown = false;
    private int growthStage = 9;
    [SerializeField] private GameObject[] stages;

    // Start is called before the first frame update
    void Start()
    {
        growTimeRemaining = growTime;
        ripeTimeRemaining = ripeTime;
        yieldsRemaining = yields;
    }

    // Update is called once per frame
    void Update()
    {
        if (yieldsRemaining > 0)
        {
            if (growTimeRemaining > 0f)
            {
                growTimeRemaining -= Time.deltaTime;
                //handle growth stages
                HandleGrowthStages();
            }
            else
            {
                grown = true;
                if(ripeTimeRemaining > 0f)
                {
                    ripeTimeRemaining -= Time.deltaTime;
                }
                else
                {
                    ripe = true;
                    FinishFarming();
                }
            }
        }
        else
        {
            //kill this plant
            //Debug.Log("Kill!");
            GameObject.Destroy(this.gameObject);
        }
    }
    private void FinishFarming()
    {
        //Debug.Log("Ripe");
        fruit.SetActive(true);
        //GameObject PlantSpawned = Instantiate(fruit, FruitPos, transform.rotation);
        
    }

    public void ExternalInteractFunc()
    {
        PickupPlant();
    }

    /// <summary>
    /// get fruit and seeds added to player inventory
    /// </summary>
    public void PickupPlant()
    {
        if (ripe)
        {
            Debug.Log("Pickup plant");
            fruit.SetActive(false);
            yieldsRemaining--;
            ripe = false;
            ripeTimeRemaining = ripeTime;
            //get player inventory (do we have to get this every single time?)
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId,
                    out var networkedClient))
            {
                IPlayer_Inventory playerInventory = networkedClient.PlayerObject.GetComponent<IPlayer_Inventory>();
                Consumable_Produce produce = new Consumable_Produce(plantType.ToString(), yield);
                Consumable_Produce produceSeeds = new Consumable_Produce(plantType.ToString() + "_Seeds", 2);
                ItemStack produceStack = new ItemStack(plantType.ToString(), 9000, typeof(Consumable_Produce));
                produceStack.AddItem(produce);
                ItemStack seedStack = new ItemStack(plantType.ToString()+"_Seeds", 9000, typeof(Consumable_Produce));
                seedStack.AddItem(produceSeeds);
                Debug.Log("Add produce");
                playerInventory.AddStackToPlayerInventory(produceStack);
                Debug.Log("Add 2 seeds");
                playerInventory.AddStackToPlayerInventory(seedStack);
            }
        }

    }

    public void LookInfoMsg(LookInfo linf)
    {
        string str = plantType.ToString();
        
        if (ripe)
        {
            str = "Ripe "+str;
        }
        else
        {
            str = "Unripe " + str;
        }
        if (grown)
        {
            str = "Mature " + str;
        }
        else
        {
            str = "Immature " + str;
        }
        string[] data = { str };
        //return type of soil
        linf.GetType().GetMethod("LookInfoCallback").Invoke(linf, new[] { data });
    }

    /// <summary>
    /// If growtimeremaining is <= a growth stage, the plant is at that growth stage.
    /// Stage 0 = growtime/3 * 1 (3)
    /// Stage 1 = growtime/3 * 2 (6)
    /// Stage 2 = growtime/3 * 3 (9)
    /// </summary>
    private void HandleGrowthStages()
    {
        float mod = growTime / stages.Length;
        for(int i = 0; i < stages.Length; i++)
        {
            if(growTimeRemaining <= mod * (i+1))//0+1, 1+1, etc
            {
                //Debug.Log(growTimeRemaining + " <= " + mod * (i + 1));
                if (growthStage != i && growthStage > i)//3(0) -> 6(1) -> 9(2), 9 growth, 6 growth|9 growth, 3|6|9 growth
                {
                    //select that growth stage
                    //Debug.Log("Grow to next stage");
                    growthStage = i;
                    GetComponent<MeshFilter>().sharedMesh = stages[growthStage].gameObject.GetComponent<MeshFilter>().sharedMesh;
                    //set scale to growthstage too
                    transform.localScale = stages[growthStage].gameObject.transform.localScale;
                    //break;
                }

            }
        }
    }
}
