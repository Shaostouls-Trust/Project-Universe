using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectUniverse.Base;
using ProjectUniverse.Player;
using ProjectUniverse.PowerSystem;
using ProjectUniverse.Data.Libraries.Definitions;
using ProjectUniverse.Data.Libraries;
using ProjectUniverse.Production.Resources;
using ProjectUniverse.UI;

namespace ProjectUniverse.Production.Machines
{
    public class Mach_DevFactory : IConstructible //MonoBehaviour,
    {
        [SerializeField] private string FactoryID;
        [SerializeField] private GameObject ProductionUI;//From player
        private string produceComponentX;
        private List<ItemStack> inputMaterials = new List<ItemStack>();
        private List<ItemStack> outputMaterials = new List<ItemStack>();
        private List<ItemStack> requiredMaterials = new List<ItemStack>();
        private List<ItemStack> availableMaterials = new List<ItemStack>();//from player or connected inventory
        private float timer = 0.0f;
        [SerializeField] bool Stop = false;
        private bool coroutineIsRunning = false;

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

        public string ProduceComponent{
            get { return produceComponentX; }
            set { produceComponentX = value; }
        }

        public void StartFactory()
        {
            //Debug.Log(inputMaterials[0] != null);//false for some reason
            ///FullyBuilt is a temp control variable!
            if (inputMaterials.Count > 0 && IConstructible_MachineFullyBuilt && ProduceComponent != "")//
            {
                if (GetComponent<IMachine>().RunMachine)
                {
                    isRunning = true;
                }
                else
                {
                    isRunning = false;
                    Debug.Log("Machine not running");
                }

                if (isRunning && isPowered)
                {
                    if (!coroutineIsRunning)
                    {
                        Debug.Log("STARTING");
                        StartCoroutine("FactoryProxyUpdate");
                    }
                }
            }
        }


        ///
        /// Use the input materials to create a component
        ///
        private IEnumerator FactoryProxyUpdate()
        {
            coroutineIsRunning = true;
            int availableToProcess;
            timer = 0f;
            float maxtime = 0f;
            //processtime is the quantity of resources and components used to make it * 1
            float processtime = 0f;
            Consumable_Component newComp = null;
            IComponentDefinition def;
            ProductionUIController puic = ProductionUI.GetComponent<ProductionUIController>();
            float counterMat = 0;
            float counterIng = 0;
            float counterComp = 0;
            //temp vars that will be replaced with real num nums based on either: 
            //available materials or number of units to produce, whichever is lower
            if (puic.CanProduceRequested())
            {
                availableToProcess = puic.GetProductionAmount();
                //Debug.Log("Process: "+availableToProcess);
            }
            else
            {
                availableToProcess = 0;
            }
            while (availableToProcess > 0 && !Stop)//inputMaterials.Count
            {
                if (isPowered && isRunning)
                {
                    if (timer <= 0.0f)
                    {
                        counterMat = 0;
                        counterIng = 0;
                        counterComp = 0;
                        IComponentLibrary.ComponentDictionary.TryGetValue(ProduceComponent, out def);
                        newComp = new Consumable_Component(ProduceComponent, availableToProcess, def);
                        processtime = def.GetBuildTime();
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
                                    if (counterComp < targetNumber)
                                    {
                                        //remove def.GetComponentRecipeList()[j].Item2
                                        inputMaterials[k].RemoveItemData(targetNumber);
                                        //inputMaterials.RemoveAt(k);
                                        //Debug.Log("removing "+ targetNumber);
                                        counterComp += targetNumber;
                                    }

                                }
                            }
                            for (int j = 0; j < def.GetIngotRecipeList().Count; j++)
                            {
                                if (inputMaterials[k].GetStackType() == def.GetIngotRecipeList()[j].Item1.GetIngotType())
                                {
                                    targetNumber = def.GetIngotRecipeList()[j].Item2;
                                    if (counterIng < targetNumber)
                                    {
                                        //remove def.GetIngotRecipeList()[j].Item2
                                        inputMaterials[k].RemoveItemData(targetNumber);
                                        //inputMaterials.RemoveAt(k);
                                        //Debug.Log("removing "+ targetNumber);
                                        counterIng += targetNumber;
                                    }

                                }
                            }
                            for (int j = 0; j < def.GetMaterialRecipeList().Count; j++)
                            {
                                if (inputMaterials[k].GetStackType() == def.GetMaterialRecipeList()[j].Item1.GetMaterialType())
                                {
                                    targetNumber = def.GetMaterialRecipeList()[j].Item2;
                                    if (counterMat < targetNumber)
                                    {
                                        //remove def.GetIngotRecipeList()[j].Item2
                                        inputMaterials[k].RemoveItemData(targetNumber);
                                        //inputMaterials.RemoveAt(k);
                                        //Debug.Log("removing "+ targetNumber);
                                        counterMat += targetNumber;
                                    }
                                }
                            }
                            //if we have all the required materials
                            //NYI
                            //if so, assign to time for processing to begin
                            timer = 1.0f * processtime;//availableToProcess * processtime
                            maxtime = timer;
                            //Debug.Log(maxtime);
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
                        //Debug.Log("Process: " + availableToProcess);
                        ItemStack compStack = new ItemStack(newComp.ComponentID, 999, typeof(Consumable_Component));
                        //Add the itemstack or whatever
                        compStack.AddItem(newComp);
                        //Debug.Log("Outputting");
                        outputMaterials.Add(compStack);

                        //Debug.Log("InputMatCount: " + inputMaterials.Count);
                        if (availableToProcess <= 0)//inputMaterials.Count <= 0 || 
                        {
                            Debug.Log("STOPPING");
                            Stop = true;
                            //When all components are made or we're out of mats, stop the update coroutine.
                            puic.UpdateUI();
                            StopCoroutine(FactoryProxyUpdate());
                        }
                    }
                }
                //puic.updatedisplay
                yield return null;
            }
            coroutineIsRunning = false;
        }

        public void InputFromPlayer(GameObject player)
        {
            //eventually we'll need to come down here to check if we can produce what was selected in the UI
            //and not just load in the entire inventory. Things will/should also be loadable by transport pipes
            //for now just say that available is the entire player inventory, and all comps can be made here
            IPlayer_Inventory playerInventory = player.GetComponent<IPlayer_Inventory>();
            availableMaterials = playerInventory.GetPlayerInventory();
            inputMaterials = availableMaterials;
            /*
            IComponentDefinition IcompDef;
            IComponentLibrary.ComponentDictionary.TryGetValue(ProduceComponent, out IcompDef);
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
                            inputMaterials.Add(playerInventory.RemoveFromPlayerInventory(j, IcompDef.GetIngotRecipeList()[i].Item2));
                            //playerInv[j], 
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
                            inputMaterials.Add(playerInventory.RemoveFromPlayerInventory(
                                j, IcompDef.GetMaterialRecipeList()[i].Item2));//playerInv[j],
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
                            inputMaterials.Add(playerInventory.RemoveFromPlayerInventory(
                                j, IcompDef.GetComponentRecipeList()[i].Item2));//playerInv[j], 
                                                                                //Debug.Log("Found Component");
                        }
                    }
                }
            }
            //Debug.Log("Available:"+availableMaterials.Count);
            //combine all materials in the inputmaterials list
            */
        }

        //remove the component from the factory
        public void OutputToPlayer(GameObject player)
        {
            Debug.Log("gimme "+ outputMaterials.Count);
            IPlayer_Inventory playerInventory = player.GetComponent<IPlayer_Inventory>();
            for (int i = 0; i < outputMaterials.Count; i++)
            {
                playerInventory.AddStackToPlayerInventory(outputMaterials[i]);
            }
            outputMaterials.Clear();
        }

        public void ExternalInteractFunc()
        {
            ProductionUI.GetComponent<ProductionUIController>().LockScreenAndFreeCursor();
            ProductionUI.SetActive(true);
            ProductionUI.GetComponent<ProductionUIController>().UpdateInputMaterials(inputMaterials);//inputMaterials
            ProductionUI.GetComponent<ProductionUIController>().UpdateProductionInventory();//inputMaterials
        }

        public void DisplayProductionUI()
        {
            ProductionUI.GetComponent<ProductionUIController>().LockScreenAndFreeCursor();
            ProductionUI.SetActive(true);
            ProductionUI.GetComponent<ProductionUIController>().UpdateInputMaterials(inputMaterials);//inputMaterials
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

        public void MachineMessageReceiver(int mode)
        {
            switch (mode)
            {
                case 1://welder
                    break;
            }
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
                    SetPoweredState(true);
                    SetRunningState(false);
                    break;
            }
        }

        public void BouncedMessageReceiver(params object[] data)
        {
            MachineMessageReceiver(data);
        }

        protected override void ProcessDamageToComponents()
        {

        }
    }
}