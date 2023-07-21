using Unity.Netcode;
using ProjectUniverse.Base;
using ProjectUniverse.Data.Libraries.Definitions;
using ProjectUniverse.Player;
using ProjectUniverse.PowerSystem;
using ProjectUniverse.Production.Resources;
using ProjectUniverse.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.Production.Machines
{
    public class Mach_AlloyForge : IConstructible
    {
        [SerializeField] private string FactoryID;
        [SerializeField] private GameObject ForgeUI;//From player
        private IngotDefinition produceIngot;
        List<IngotDefinition> ingotRecipe = new List<IngotDefinition>();
        List<MaterialDefinition> materialRecipe = new List<MaterialDefinition>();
        private List<ItemStack> inputMaterials = new List<ItemStack>();
        private List<ItemStack> outputMaterials = new List<ItemStack>();
        private List<ItemStack> availableMaterials = new List<ItemStack>();//from player or connected inventory
        private float timer = 0.0f;
        [SerializeField] bool Stop = false;
        private bool coroutineIsRunning = false;

        private bool isRunning;
        private bool isPowered;

        public IngotDefinition ProduceIngot
        {
            get { return produceIngot; }
            set { produceIngot = value; }
        }

        public List<IngotDefinition> IngotRecipe
        {
            get { return ingotRecipe; }
            set { ingotRecipe = value; }
        }
        public List<MaterialDefinition> MaterialRecipe
        {
            get { return materialRecipe; }
            set { materialRecipe = value; }
        }

        public void StartFactory()
        {
            //Debug.Log(inputMaterials[0] != null);//false for some reason
            ///FullyBuilt is a temp control variable!
            if (inputMaterials.Count > 0 && IConstructible_MachineFullyBuilt && ProduceIngot != null)
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
            Stop = false;
            int availableToProcess;
            timer = 0f;
            float maxtime = 0f;
            //processtime is the quantity of resources and components used to make it * 1
            float processtime = 0f;
            Consumable_Ingot newComp = null;
            AlloyForgeUIController puic = ForgeUI.GetComponent<AlloyForgeUIController>();
            //temp vars that will be replaced with real num nums based on either: 
            //available materials or number of units to produce, whichever is lower

            // Disable the UI button until the job is finished
            ForgeUI.GetComponent<AlloyForgeUIController>().SetStartButton(false);

            // Update the input materials
            ForgeUI.GetComponent<AlloyForgeUIController>().UpdateInputMaterials(inputMaterials);

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
                        newComp = new Consumable_Ingot(ProduceIngot.GetIngotType(), 4, 10);
                        processtime = 5;
                        //remove the required materials
                        for (int k = inputMaterials.Count-1; k >= 0; k--)
                        {
                            for (int j = 0; j < IngotRecipe.Count; j++)
                            {
                                if (inputMaterials[k].GetStackType() == IngotRecipe[j].GetIngotType())
                                {
                                    inputMaterials[k].RemoveItemData(1);
                                    if (inputMaterials[k].GetRealLength() == 0)
                                    {
                                        inputMaterials.Remove(inputMaterials[k]);
                                        k--;
                                    }
                                }
                            }
                            for (int j = 0; j < MaterialRecipe.Count; j++)
                            {
                                if (inputMaterials[k].GetStackType() == MaterialRecipe[j].GetMaterialType())
                                {
                                    inputMaterials[k].RemoveItemData(1);
                                    if (inputMaterials[k].GetRealLength() == 0)
                                    {
                                        inputMaterials.Remove(inputMaterials[k]);
                                        k--;
                                    }
                                }
                            }
                            timer = 1.0f * processtime;//availableToProcess * processtime
                            maxtime = timer;
                            //Update the production UI
                            puic.ResetProgressBar();
                            puic.UpdateInputMaterials(inputMaterials);
                        }
                    }
                    else
                    {
                        //smelting timer. When it reaches zero, the ore has been turned into an ingot.
                        timer -= Time.deltaTime;
                        //update progress bar
                        puic.UpdateProgressBar(timer / maxtime);
                    }
                    if (timer <= 0.0f)
                    {
                        //Ore was removed to process it. Only add the process amount to the ingot itemstack
                        availableToProcess -= 1;
                        //Debug.Log("Process: " + availableToProcess);
                        Debug.Log(newComp.ToString());
                        ItemStack compStack = new ItemStack(newComp.GetIngotType(), 9000, typeof(Consumable_Ingot));
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
            ForgeUI.GetComponent<AlloyForgeUIController>().SetStartButton(true);
        }

        public void InputFromPlayer(GameObject player)
        {
            //eventually we'll need to come down here to check if we can produce what was selected in the UI
            //and not just load in the entire inventory. Things will/should also be loadable by transport pipes
            //for now just say that available is the entire player inventory, and all comps can be made here
            IPlayer_Inventory playerInventory = player.GetComponent<IPlayer_Inventory>();
            availableMaterials = playerInventory.GetPlayerInventory();
            inputMaterials = availableMaterials;
        }

        public void OutputToPlayer()
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
            {
                IPlayer_Inventory playerInventory = networkedClient.PlayerObject.gameObject.GetComponent<IPlayer_Inventory>();
                Debug.Log("gimme " + outputMaterials.Count);
                for (int i = 0; i < outputMaterials.Count; i++)
                {
                    playerInventory.AddStackToPlayerInventory(outputMaterials[i]);
                }
                outputMaterials.Clear();
            }
        }

        public void ExternalInteractFunc()
        {
            ForgeUI.GetComponent<AlloyForgeUIController>().LockScreenAndFreeCursor();
            ForgeUI.SetActive(true);
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
            {
                InputFromPlayer(networkedClient.PlayerObject.gameObject);
            }
            ForgeUI.GetComponent<AlloyForgeUIController>().UpdateInputMaterials(inputMaterials);
        }

        public void DisplayProductionUI()
        {
            ForgeUI.GetComponent<AlloyForgeUIController>().LockScreenAndFreeCursor();
            ForgeUI.SetActive(true);
            ForgeUI.GetComponent<AlloyForgeUIController>().UpdateInputMaterials(inputMaterials);
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
                    SetPoweredState(true);
                    //SetRunningState(false);
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