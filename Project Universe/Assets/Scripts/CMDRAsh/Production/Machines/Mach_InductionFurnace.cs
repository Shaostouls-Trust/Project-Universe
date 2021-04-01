using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mach_InductionFurnace : MonoBehaviour
{
    [SerializeField] private List<ItemStack> inputMaterials;
    [SerializeField] private List<ItemStack> outputMaterials;
    private List<ItemStack> byproducts;
    //Refinery level determines quality (and speed for now). Higher tier will take longer (more steps).
    [SerializeField] private int refineryLevel;
    [SerializeField] private float processAmount;
    private float timer = 0.0f;
    //Machine will only 'bool run' when it has input materials. This way it will only consume power when processing.
    private bool isRunning;
    private bool isPowered;
    //Also means we don't need an update method.
    //[SerializeField] private IMachine thisIMachineScript;

    override
    public string ToString()
    {
        string str = "Input: " + inputMaterials.ToString();
        str += "\n Output: " + outputMaterials.ToString();
        return str;
    }

    public int ProxyUpdateHelper(out Dictionary<OreDefinition,float> oreInclusions,
                                out Dictionary<MaterialDefinition, float> matInclusions,
                                out IngotDefinition ingotDef,
                                out int qualityLevel,
                                out float availableToProcess)
    {
        //Get ore from the inputMaterials
        ItemStack processStack = inputMaterials[0].RemoveItemData(processAmount);
        Debug.Log(processStack.ToString());
        availableToProcess = processStack.Length();
        // Get ore definiton
        OreDefinition oreDef;
        if (OreLibrary.OreDictionary.TryGetValue(processStack.GetStackType(), out oreDef))
        {
            //get the ore's speed of smelting, inclusions, and output
            int smeltTime = oreDef.GetProcessingTimePerUnit();
            //get the ore inclusions from the inclusion library
            if (processStack.GetOriginalType() == typeof(Consumable_Ore))
            {
                //Consumable_Ore ore = processStack.gameObject.GetComponent<Consumable_Ore>();
                ///
                /// THIS ONLY GETS THE FIRST ORE!! WILL THIS BE AN ISSUE?
                ///
                Consumable_Ore ore;
                ore = processStack.GetItemArray().GetValue(0) as Consumable_Ore;
                oreInclusions = ore.GetOreInclusions();//MASSIVE IF
                matInclusions = ore.GetMaterialInclusions();
                //inclusions are multiplied by the (50 -> 90)% refinery level.
                qualityLevel = ore.GetOreQuality();
            }
            else
            {
                oreInclusions = null;
                matInclusions = null;
                qualityLevel = 0;
                availableToProcess = 0;
            }
            //get the ingot definition from the ingot library (produces: ingotDef)
            IngotLibrary.IngotDictionary.TryGetValue(oreDef.GetProductionID(), out ingotDef);
            Debug.Log(smeltTime);
            return smeltTime;
        }
        else
        {
            Debug.Log("Bad Ore ID");//eventually we want to bypass any bad IDs
            oreInclusions = null;
            matInclusions = null;
            ingotDef = null;
            qualityLevel = 0;
            availableToProcess = 0;
            return 0;
        }
    }

    ///
    /// Process a certain amount of ore in one update
    ///
    private IEnumerator ProxyUpdate()
    {
        float availableToProcess = 0f;
        IngotDefinition ingotDef = null;
        Dictionary<OreDefinition, float> oreInclusions;
        Dictionary<MaterialDefinition, float> matInclusions;
        int oreQuality;
        Consumable_Ingot newIngot = null;
        while(inputMaterials.Count > 0)
        {
            if(isRunning && isPowered)
            {
                if (timer <= 0.0f)
                {
                    float totalTime = ProxyUpdateHelper(out oreInclusions, out matInclusions, out ingotDef, out oreQuality, out availableToProcess);
                    timer = totalTime;
                    newIngot = new Consumable_Ingot(ingotDef.GetIngotType(), oreQuality, ingotDef,
                        oreInclusions, matInclusions, availableToProcess);
                }
                else
                {
                    //smelting timer. When it reaches zero, the ore has been turned into an ingot.
                    timer -= Time.deltaTime;
                    Debug.Log(timer);
                }

                ///
                /// RIGHT NOW, 1 UNIT ORE = 1 INGOT WITH NO REDUCTION IN IMPURITIES OR BYPRODUCT PRODUCTION!!
                ///
                if (timer <= 0.0f)
                {
                    //Ore was removed to process it. Only add the process amount to the ingot itemstack
                    ItemStack ingotStack = new ItemStack(ingotDef.GetIngotType(), 100, typeof(Consumable_Ingot));
                    //Add the itemstack or whatever
                    ingotStack.AddItem(newIngot);
                    ///
                    /// THERE IS A HIGH LIKELYHOOD THAT NO INGOTS WILL BE STRICTLY EQUIVALENT IN METADATA DUE TO IMPURITIES!!
                    ///
                    Debug.Log("Outputting");
                    outputMaterials.Add(ingotStack);
                }
                if (inputMaterials.Count == 0 && availableToProcess <= 0)
                {
                    Debug.Log("STOPPING");
                    //When all ore is processed, stop the update coroutine.
                    StopCoroutine(ProxyUpdate());
                }
                yield return null;
            }
        }
    }

    //Try to load in a whole stack of ore for processing
    public void InputFromPlayer(GameObject player)
    {
        IPlayer_Inventory playerInventory = player.GetComponent<IPlayer_Inventory>();
        //put in the ore held in the player's hand for now?
        //or just dump all the ore in for now?
        //or dump in last index (THIS ONE FOR NOW)
        List<ItemStack> playerInv = playerInventory.GetPlayerInventory();
        if(playerInv.Count > 0)
        {
            Debug.Log("Removing:");
            inputMaterials.Add(playerInv[0]);
            playerInventory.RemoveFromPlayerInventory(playerInv[0]);
        }
        /*
        for (int i = playerInv.Count-1; i >= 0; i--)
        {
            string[] typeType = playerInv[i].GetStackType().Split('_');
            if (typeType[0] == "Ore")
            {
                Debug.Log("Adding " + playerInv[i].ToString() + " to inventory");
                playerInventory.RemoveFromPlayerInventory(playerInv[i]);
                inputMaterials.Add(playerInv[i]);
            }
        }
        */
        if (inputMaterials.Count > 0)
        {
            if(GetComponent<IMachine>().RunMachine && isPowered)
            {
                //Debug.Log(inputMaterials.Count);
                Debug.Log("CoRtn");
                StartCoroutine("ProxyUpdate");
            }
            
        }
    }

    //remove the processed metal from the furnace
    public void OutputToPlayer(GameObject player)
    {
        Debug.Log("gimme!");
        IPlayer_Inventory playerInventory = player.GetComponent<IPlayer_Inventory>();
        for(int i = 0; i < outputMaterials.Count; i++)
        {
            playerInventory.AddStackToPlayerInventory(outputMaterials[i]);
            //playerInventory.AddToPlayerInventory<Consumable_Ingot>(outputMaterials[i].GetItemArray().);
        }
    }

    public void SetPoweredState(bool state)
    {
        isPowered = state;
    }

    public void RunMachine(int powerLevel)
    {
        switch (powerLevel)
        {
            case 0:
                SetPoweredState(true);
                //SetRunningState(true);
                break;
            case 1:
                SetPoweredState(true);
                //SetRunningState(true);
                break;
            case 2:
                SetPoweredState(true);
                //SetRunningState(true);
                break;
            case 3:
                SetPoweredState(true);
                //SetRunningState(true);
                break;
            case 4:
                SetPoweredState(false);
                //SetRunningState(false);
                break;
            case 5:
                SetPoweredState(false);
                //SetRunningState(false);
                break;
        }
    }
}
