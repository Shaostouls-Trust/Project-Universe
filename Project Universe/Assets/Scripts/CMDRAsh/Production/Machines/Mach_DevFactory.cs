using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mach_DevFactory : MonoBehaviour
{
    [SerializeField] private string FactoryID;
    [SerializeField] private GameObject ProductionUI;
    public string produceComponentX;
    private List<ItemStack> inputMaterials = new List<ItemStack>();
    private List<ItemStack> outputMaterials = new List<ItemStack>();
    private List<ItemStack> requiredMaterials = new List<ItemStack>();
    private List<ItemStack> availableMaterials = new List<ItemStack>();//from player or connected inventory
    private float timer = 0.0f;
    [SerializeField] bool Stop = false;

    private bool isRunning;
    private bool isPowered;

    //Machine will only 'bool run' when it has input materials. This way it will only consume power when processing.
    //Also means we don't need an update method.
    //[SerializeField] private IMachine thisIMachineScript;

    override
    public string ToString()
    {
        string str = "Available: " + availableMaterials.ToString();
        str += "Input: " + inputMaterials.ToString();
        str += "\n Output: " + outputMaterials.ToString();
        return str;
    }

    public void StartFactory()
    {
        //Debug.Log(inputMaterials[0] != null);//false for some reason
        if (inputMaterials.Count > 0 && produceComponentX != "")
        {
            if (GetComponent<IMachine>().RunMachine)
            {
                isRunning = true;
            }
            else
            {
                isRunning = false;
            }

            if (isRunning && isPowered)
            {
                Debug.Log("STARTING " + inputMaterials.Count);
                StartCoroutine("FactoryProxyUpdate");
            }
        }
    }
    

    ///
    /// Use the input materials to create a component
    ///
    private IEnumerator FactoryProxyUpdate()
    {
        int availableToProcess = 0;
        timer = 0f;
        float maxtime = 0f;
        float processtime = 0f;
        Consumable_Component newComp = null;
        IComponentDefinition def;
        ProductionUIController puic = ProductionUI.GetComponent<ProductionUIController>();
        //temp vars that will be replaced with real num nums based on either: 
        //available materials or number of units to produce, whichever is lower
        if (puic.CanProduceRequested())
        {
            //Debug.Log("ODALADDLE ODALADDEL");
            availableToProcess = puic.GetProductionAmount();
        }
        else
        {
            availableToProcess = 0;
        }

        //availableToProcess = 1;
        processtime = 2.5f;
        //Debug.Log("aTP: "+availableToProcess);
        //Debug.Log(!Stop);
        while (availableToProcess > 0 && !Stop)//inputMaterials.Count
        {
            if(isPowered && isRunning)
            {
                if (timer <= 0.0f)
                {
                    //Debug.Log("Produce Component:"+produceComponentX);
                    IComponentLibrary.ComponentDictionary.TryGetValue(produceComponentX, out def);
                    newComp = new Consumable_Component(produceComponentX, availableToProcess, def);
                    float counter = 0;
                    float targetNumber = 0.0f;
                    //remove the required materials
                    for (int k = 0; k < inputMaterials.Count; k++)
                    {

                        for (int j = 0; j < def.GetComponentRecipeList().Count; j++)
                        {
                            if (inputMaterials[k].GetStackType() == def.GetComponentRecipeList()[j].Item1.GetComponentType())
                            {
                                //if we haven't yet removed the number of needed components
                                targetNumber = def.GetComponentRecipeList()[j].Item2;
                                if (counter < targetNumber)
                                {
                                    //remove def.GetComponentRecipeList()[j].Item2
                                    inputMaterials[k].RemoveItemData(targetNumber);
                                    inputMaterials.RemoveAt(k);
                                    //Debug.Log("removing "+ targetNumber);
                                    counter += targetNumber;
                                }

                            }
                        }
                        counter = 0;
                        for (int j = 0; j < def.GetIngotRecipeList().Count; j++)
                        {
                            if (inputMaterials[k].GetStackType() == def.GetIngotRecipeList()[j].Item1.GetIngotType())
                            {
                                targetNumber = def.GetIngotRecipeList()[j].Item2;
                                if (counter < targetNumber)
                                {
                                    //remove def.GetIngotRecipeList()[j].Item2
                                    inputMaterials[k].RemoveItemData(targetNumber);
                                    inputMaterials.RemoveAt(k);
                                    //Debug.Log("removing "+ targetNumber);
                                    counter += targetNumber;
                                    //Debug.Log("counter:" + counter);
                                }

                            }
                        }
                        counter = 0;
                        for (int j = 0; j < def.GetMaterialRecipeList().Count; j++)
                        {
                            if (inputMaterials[k].GetStackType() == def.GetMaterialRecipeList()[j].Item1.GetMaterialType())
                            {
                                targetNumber = def.GetMaterialRecipeList()[j].Item2;
                                if (counter < targetNumber)
                                {
                                    //remove def.GetIngotRecipeList()[j].Item2
                                    inputMaterials[k].RemoveItemData(targetNumber);
                                    inputMaterials.RemoveAt(k);
                                    //Debug.Log("removing "+ targetNumber);
                                    counter += targetNumber;
                                }
                            }
                        }
                        //if we have all the required materials
                        //NYI
                        //if so, assign to time for processing to begin
                        timer = 1.0f * processtime;//availableToProcess * processtime
                        maxtime = timer;
                        Debug.Log(maxtime);
                        //Update the production UI
                        puic.ResetProgressBar();
                        puic.UpdateInputMaterials(inputMaterials);
                        puic.UpdateProductionInventory();
                    }
                }
                else
                {
                    //This runs twice (once more after it's supposed to finish)
                    //smelting timer. When it reaches zero, the ore has been turned into an ingot.
                    timer -= Time.deltaTime;
                    //update progress bar
                    puic.UpdateProgressBar(timer / maxtime);
                    //Debug.Log(timer);
                }
                if (timer <= 0.0f)
                {
                    //Ore was removed to process it. Only add the process amount to the ingot itemstack
                    availableToProcess -= 1;
                    ItemStack ingotStack = new ItemStack(newComp.GetComponentID(), 100, typeof(Consumable_Component));
                    //Add the itemstack or whatever
                    ingotStack.AddItem(newComp);
                    //Debug.Log("Outputting");
                    outputMaterials.Add(ingotStack);

                    //Debug.Log("InputMatCount: " + inputMaterials.Count);
                    if (availableToProcess <= 0)//inputMaterials.Count <= 0 || 
                    {
                        Debug.Log("STOPPING");
                        //Stop = true;
                        //When all components are made or we're out of mats, stop the update coroutine.
                        puic.UpdateUI();
                        StopCoroutine(FactoryProxyUpdate());
                    }
                }
            }
            yield return null;
        }
    }


    public void InputFromPlayer(GameObject player)
    {
        //eventually we'll need to see if the component is producable by the factory
        //and not just load in the entire inventory
        //for now just say that available is the entire player inventory, and all comps can be made here
        IPlayer_Inventory playerInventory = player.GetComponent<IPlayer_Inventory>();
        availableMaterials = playerInventory.GetPlayerInventory();

        IComponentDefinition IcompDef;
        IComponentLibrary.ComponentDictionary.TryGetValue(produceComponentX, out IcompDef);
        List<ItemStack> playerInv = playerInventory.GetPlayerInventory();
        //Debug.Log("Inventory.Count: "+playerInv.Count);
        if (playerInv.Count > 0)
        {
            //Debug.Log("checking inventory");
            //get the required mats from player inventory
            for (int j = 0; j < playerInv.Count; j++)
            {
                string stackType = playerInv[j].GetStackType();
                //search ingots
                for (int i = 0; i < IcompDef.GetIngotRecipeList().Count; i++)
                {
                    //check the player inventory for the needed material
                    if (stackType == IcompDef.GetIngotRecipeList()[i].Item1.GetIngotType())
                    {
                        //remove the needed amount
                        inputMaterials.Add(playerInventory.RemoveFromPlayerInventory(playerInv[j], j, IcompDef.GetIngotRecipeList()[i].Item2));
                        //Debug.Log("Found Ingot");
                    }
                }
                //search materials
                for (int i = 0; i < IcompDef.GetMaterialRecipeList().Count; i++)
                {
                    //check the player inventory for the needed material
                    if (stackType == IcompDef.GetMaterialRecipeList()[i].Item1.GetMaterialType())
                    {
                        //remove the needed amount
                        inputMaterials.Add(playerInventory.RemoveFromPlayerInventory(playerInv[j], 
                            j, IcompDef.GetMaterialRecipeList()[i].Item2));
                        //Debug.Log("Found Material");
                    }
                }
                //search Components
                for (int i = 0; i < IcompDef.GetComponentRecipeList().Count; i++)
                {
                    //check the player inventory for the needed material
                    if (stackType == IcompDef.GetComponentRecipeList()[i].Item1.GetComponentType())
                    {
                        //remove the needed amount
                        inputMaterials.Add(playerInventory.RemoveFromPlayerInventory(playerInv[j], 
                            j, IcompDef.GetComponentRecipeList()[i].Item2));
                        //Debug.Log("Found Component");
                    }
                }
            }
        }
        //Debug.Log("Available:"+availableMaterials.Count);
        //combine all materials in the inputmaterials list
    }

    //remove the component from the factory
    public void OutputToPlayer(GameObject player)
    {
        Debug.Log("gimme!");
        IPlayer_Inventory playerInventory = player.GetComponent<IPlayer_Inventory>();
        for (int i = 0; i < outputMaterials.Count; i++)
        {
            playerInventory.AddStackToPlayerInventory(outputMaterials[i]);
        }
        outputMaterials.Clear();
    }

    public void DisplayProductionUI()
    {
        ProductionUI.GetComponent<ProductionUIController>().LockScreenAndFreeCursor();
        ProductionUI.SetActive(true);
        ProductionUI.GetComponent<ProductionUIController>().UpdateInputMaterials(inputMaterials);
        ProductionUI.GetComponent<ProductionUIController>().UpdateProductionInventory();//inputMaterials
    }

    public List<ItemStack> GetInputMaterials()
    {
        return inputMaterials;
    }

    public void SetPoweredState(bool state)
    {
        isPowered = state;
    }
    public void SetRunningState(bool state)
    {
        isRunning = state;
        GetComponent<IMachine>().RunMachine = state;
    }

    public void RunMachine(int powerLevel)
    {
        switch (powerLevel)
        {
            case 0:
                SetPoweredState(true);
                SetRunningState(true);
                break;
            case 1:
                SetPoweredState(true);
                SetRunningState(true);
                break;
            case 2:
                SetPoweredState(true);
                SetRunningState(true);
                break;
            case 3:
                SetPoweredState(true);
                SetRunningState(true);
                break;
            case 4:
                SetPoweredState(false);
                SetRunningState(false);
                break;
            case 5:
                SetPoweredState(false);
                SetRunningState(false);
                break;
        }
    }

    /*
    public void SetComponent(string component)
    {
        produceComponentX = component;
    }
    */
}
