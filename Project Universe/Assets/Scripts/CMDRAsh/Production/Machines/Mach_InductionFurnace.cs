using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectUniverse.Base;
using ProjectUniverse.Player;
using ProjectUniverse.PowerSystem;
using ProjectUniverse.Production.Resources;
using ProjectUniverse.Data.Libraries.Definitions;
using ProjectUniverse.Data.Libraries;
using ProjectUniverse.UI;
using MLAPI;
using MLAPI.Messaging;

namespace ProjectUniverse.Production.Machines
{
    public class Mach_InductionFurnace : IConstructible//MonoBehaviour
    {
        [SerializeField] private List<ItemStack> inputMaterials;
        [SerializeField] private List<ItemStack> outputMaterials;
        [SerializeField] private GameObject SmelterUI;//probably just have this UI tied to this object, rather than the player

        private List<ItemStack> byproducts;
        //Refinery level determines quality (and speed for now). Higher tier will take longer (more steps).
        [SerializeField] private int refineryLevel;
        [SerializeField] private float processAmount;
        private float timer = 0.0f;
        private float totaltime = 0.0f;
        //Machine will only 'bool run' when it has input materials. This way it will only consume power when processing.
        private bool isRunning;
        private bool isPowered;
        private bool Stop = false;
        //Also means we don't need an update method.
        //[SerializeField] private IMachine thisIMachineScript;

        override
        public string ToString()
        {
            string str = "Input: " + inputMaterials.ToString();
            str += "\n Output: " + outputMaterials.ToString();
            return str;
        }

        public int ProxyUpdateHelper(out Dictionary<OreDefinition, float> oreInclusions,
                                    out Dictionary<MaterialDefinition, float> matInclusions,
                                    out IngotDefinition ingotDef,
                                    out int qualityLevel,
                                    out float availableToProcess)
        {
            //Get ore from the inputMaterials
            ItemStack processStack = inputMaterials[0].RemoveItemData(processAmount);
            Debug.Log(processStack.ToString());
            availableToProcess = processStack.GetRealLength();
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
            //Debug.Log(inputMaterials.Count);
            //Debug.Log(Stop);
            while (inputMaterials.Count > 0 && inputMaterials[0].GetRealLength() != 0 && !Stop)
            {
                if (isRunning && isPowered)
                {
                    if (timer <= 0.0f)
                    {
                        totaltime = ProxyUpdateHelper(out oreInclusions, out matInclusions, out ingotDef, out oreQuality, out availableToProcess);
                        timer = totaltime;
                        newIngot = new Consumable_Ingot(ingotDef.GetIngotType(), oreQuality, ingotDef,
                            oreInclusions, matInclusions, availableToProcess);
                    }
                    else
                    {
                        //smelting timer. When it reaches zero, the ore has been turned into an ingot.
                        timer -= Time.deltaTime;
                        SmelterUI.GetComponent<InductionSmelterUIController>().UpdateProgressBar(timer / totaltime);
                        //Debug.Log(timer);
                    }

                    ///
                    /// RIGHT NOW, 1 UNIT ORE = 1 INGOT WITH NO REDUCTION IN IMPURITIES OR BYPRODUCT PRODUCTION!!
                    ///
                    if (timer <= 0.0f)
                    {
                       
                        //Ore was removed to process it. Only add the process amount to the ingot itemstack
                        ItemStack ingotStack = new ItemStack(ingotDef.GetIngotType(), 999, typeof(Consumable_Ingot));
                        //Add the itemstack or whatever
                        ingotStack.AddItem(newIngot);
                        ///
                        /// THERE IS A HIGH LIKELYHOOD THAT NO INGOTS WILL BE STRICTLY EQUIVALENT IN METADATA DUE TO IMPURITIES!!
                        ///
                        
                        //Debug.Log("Outputting");
                        outputMaterials.Add(ingotStack);
                        SmelterUI.GetComponent<InductionSmelterUIController>().UpdateProductionPanel();
                        SmelterUI.GetComponent<InductionSmelterUIController>().UpdateAvailableOres();
                        SmelterUI.GetComponent<InductionSmelterUIController>().OnIngotsProduced(outputMaterials);

                        //Debug.Log("Materials left: "+inputMaterials[0]);
                    }
                    if (inputMaterials.Count == 0 && availableToProcess <= 0)
                    {
                        //Debug.Log("STOPPING");
                        Stop = true;
                        //When all ore is processed, stop the update coroutine.
                        StopCoroutine(ProxyUpdate());
                    }
                }
                else
                {
                    Stop = true;
                }
                yield return null;
            }
            //Debug.Log("Termino");
            //Attempt to push the ingots to Next or dump in output stack for player
            if (SmelterUI.GetComponent<InductionSmelterUIController>().OutputMode)
            {
                //true means to player

                //false means to Next (NYI)
            }
        }

        //Try to load in a whole stack of ore for processing
        //ClientRpc?
        public void DisplaySmelterUI(GameObject player)
        {
            SmelterUI.GetComponent<InductionSmelterUIController>().LockScreenAndFreeCursor();
            SmelterUI.SetActive(true);
            SmelterUI.GetComponent<InductionSmelterUIController>().SetInputInventory(player.GetComponent<IPlayer_Inventory>().GetPlayerInventory());
            //SmelterUI.GetComponent<InductionSmelterUIController>().UpdateProductionInventory();
        }

        public void SetInputMaterials(List<ItemStack> ores)
        {
            inputMaterials = ores;
        }

        public void StartFactory()
        {
            if (inputMaterials.Count > 0)
            {
                if (isRunning && isPowered)
                {
                    //Debug.Log(inputMaterials.Count);
                    Debug.Log("Coroutine");
                    StartCoroutine("ProxyUpdate");
                }

            }
        }

        //remove the processed metal from the furnace
        public void OutputToPlayer(GameObject player)
        {
            Debug.Log("gimme!");
            IPlayer_Inventory playerInventory = player.GetComponent<IPlayer_Inventory>();
            for (int i = 0; i < outputMaterials.Count; i++)
            {
                playerInventory.AddStackToPlayerInventory(outputMaterials[i]);
                //playerInventory.AddToPlayerInventory<Consumable_Ingot>(outputMaterials[i].GetItemArray().);
            }
        }

        public void BouncedMessageReceiver(params object[] data)
        {
            MachineMessageReceiver(data);
        }

        private void SetPoweredState(bool state)
        {
            isPowered = state;
        }

        private void SetRunningState(bool state)
        {
            isRunning = state;
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

        protected override void ProcessDamageToComponents() 
        { 

        }
    }
}