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
using Unity.Netcode;
using ProjectUniverse.Items;
using ProjectUniverse.Util;

namespace ProjectUniverse.Production.Machines
{
    /// <summary>
    /// Smelt ores into ingots with reduced inclusions and variable qualities
    /// </summary>
    public class Mach_InductionFurnace : IConstructible
    {
        [SerializeField] private List<ItemStack> inputMaterials;
        [SerializeField] private List<ItemStack> outputMaterials;
        [SerializeField] private GameObject SmelterUI;
        [SerializeField] private GameObject transferDuct;
        [SerializeField] private Inventory inventory;
        [SerializeField] private bool ArcFurnace;

        //Impurities can be separated into dust or ingots
        private List<ItemStack> byproducts;
        //Refinery level determines quality (and speed for now). Higher tier will take longer (more steps).
        //0 - tier 0, 1 - tier 1, etc
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

        public GameObject TransferDuct
        {
            get { return transferDuct; }
            set { transferDuct = value; }
        }

        public Inventory Inventory
        {
            get { return inventory; }
        }

        public List<ItemStack> OutputMaterials
        {
            get { return outputMaterials; }
        }

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
                //Debug.Log(smeltTime);
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
        /// Add up the mass of the ore and precalculate how many ingots it will create based on quality and refinery level
        ///
        public int CalculateIngotOutput(List<ItemStack> materials, out Consumable_Ingot ingotOutput)
            //out IngotDefinition ingotDef, out List<(OreDefinition, float)> oreInclusions,
            //out List<(MaterialDefinition, float)> matInclusions)
        {
            float massOre = 0f;
            float redux = 0f;
            float quality = 0f;
            List<(OreDefinition, float)> oreInclusions = new List<(OreDefinition, float)>();
            List<(MaterialDefinition, float)> matInclusions = new List<(MaterialDefinition, float)>();
            IngotDefinition ingotDef;

            if (materials.Count > 0)
            {
                // get ore definition
                if (OreLibrary.OreDictionary.TryGetValue(materials[0].GetStackType(), out OreDefinition oreDef))
                {
                    //get the ingot definition from the ingot library
                    IngotLibrary.IngotDictionary.TryGetValue(oreDef.GetProductionID(), out ingotDef);
                }
                else
                {
                    ingotDef = null;
                }
                for (int a = 0; a < materials.Count; a++)
                {
                    if (materials[a].GetRealLength() >= 0)
                    {
                        for (int i = 0; i < materials[a].GetItemArray().Length; i++)
                        {
                            //Debug.Log((materials[a].GetItemArray().GetValue(i) as Consumable_Ore));
                            ///
                            /// REDUX if ores only
                            ///
                            Consumable_Ore ore = (materials[a].GetItemArray().GetValue(i) as Consumable_Ore);
                            //if (!ArcFurnace)
                            //{
                            //    refineryLevel = 0;
                            //}
                            redux = Utils.RefinementMassLoss(refineryLevel, ore.GetOreQuality());
                            //Debug.Log(redux);
                            massOre += ore.GetOreMass() - (ore.GetOreMass() * redux) - (ore.GetOreMass() * Utils.OreToIngotBaseLoss(ore.GetOreQuality()));
                            quality += ore.GetOreQuality();

                            // total up all inclusions
                            // for every ore inclusion in the ore
                            //Debug.Log(ore.GetOreInclusions().Count);
                            foreach (var k in ore.GetOreInclusions().Keys)
                            {
                                bool oreAdded = false;
                                ore.GetOreInclusions().TryGetValue(k, out float amt);
                                //Debug.Log(k.GetOreType() + ":" + amt);
                                // if it's already in the final inclusion thing
                                for (int oii = 0; oii < oreInclusions.Count; oii++)
                                {
                                    if (oreInclusions[oii].Item1 == k)
                                    {
                                        oreInclusions[oii] = (oreInclusions[oii].Item1, oreInclusions[oii].Item2 + amt);
                                        oreAdded = true;
                                    }
                                }
                                if (!oreAdded)
                                {
                                    //Debug.Log("Adding inclusion");
                                    oreInclusions.Add((k, amt));
                                }
                            }

                            // for every material inclusion
                            foreach (var k in ore.GetMaterialInclusions().Keys)
                            {
                                bool matAdded = false;
                                ore.GetMaterialInclusions().TryGetValue(k, out float amt);
                                //Debug.Log(k.GetMaterialType() + ":" + amt);
                                // if it's already in the final inclusion thing
                                for (int mii = 0; mii < matInclusions.Count; mii++)
                                {
                                    
                                    if (matInclusions[mii].Item1 == k)
                                    {
                                        matInclusions[mii] = (matInclusions[mii].Item1, matInclusions[mii].Item2 + amt);
                                        matAdded = true;
                                    }
                                }
                                if (!matAdded)
                                {
                                    //Debug.Log("Adding inclusion");
                                    matInclusions.Add((k, amt));
                                }
                            }
                        }
                    }
                }

                float incFactor = (1 - redux) / massOre;
                // inclusion redux (per mass by redux)
                for (int a = 0; a < oreInclusions.Count; a++)
                {
                    oreInclusions[a] = (oreInclusions[a].Item1, oreInclusions[a].Item2 * incFactor);
                }
                for (int b = 0; b < matInclusions.Count; b++)
                {
                    matInclusions[b] = (matInclusions[b].Item1, matInclusions[b].Item2 * incFactor);
                }

                // turn the final ore mass into a number ingots
                float rawIngots = massOre / 10f;
                float dustIngots = rawIngots % 1;
                float numIngots = rawIngots - dustIngots;

                // the quality of the ingots is the average quality of all ingots rounded up
                quality = Mathf.RoundToInt(quality / materials[0].GetItemArray().Length);

                Dictionary<OreDefinition, float> oreInclusionsDict = new Dictionary<OreDefinition, float>();
                Dictionary<MaterialDefinition, float> matInclusionsDict = new Dictionary<MaterialDefinition, float>();

                //create dictionaries for the inclusions
                foreach (var ore in oreInclusions)
                {
                    oreInclusionsDict.Add(ore.Item1, ore.Item2);
                }
                foreach (var mat in matInclusions)
                {
                    matInclusionsDict.Add(mat.Item1, mat.Item2);
                }

                // create the template ingot
                ingotOutput = new Consumable_Ingot(ingotDef.GetIngotType(), (int)quality, ingotDef, oreInclusionsDict, matInclusionsDict, 10f);

                return (int)numIngots;
            }

            ingotOutput = null;
            return 0;
        }

        ///
        /// Process a stack of input ore into a batch of x ingots of y average quality and z average inclusions.
        ///
        public IEnumerator ProxyUpdate2()
        {
            //float totalStock = inputMaterials[0].GetRealLength();
            int ingotsAvailableToProcess;
            Consumable_Ingot newIngot;
            InductionSmelterUIController smelter = SmelterUI.GetComponent<InductionSmelterUIController>();
            float smeltTimeSingle = 1f;
            Stop = false;

            // Use the available ores to determine the final product
            ingotsAvailableToProcess = CalculateIngotOutput(inputMaterials, out newIngot);

            if (OreLibrary.OreDictionary.TryGetValue(inputMaterials[0].GetStackType(), out OreDefinition oreDef))
            {
                //get the smelt time
                smeltTimeSingle = oreDef.GetProcessingTimePerUnit();
            }
            
            while (ingotsAvailableToProcess > 0 && inputMaterials[0].GetRealLength() >= 0 && !Stop)//the last ingot turns this into input == 0 
            {
                if (isRunning && isPowered)
                {
                    if (inputMaterials.Count > 0f)
                    {
                        if (timer <= 0.0f)
                        {
                            totaltime = smeltTimeSingle * (float)ingotsAvailableToProcess;
                            timer = totaltime;
                        }
                        else
                        {
                            //smelting timer. When it reaches zero, the ore has been turned into an ingot.
                            timer -= Time.deltaTime;
                            smelter.UpdateProgressBar(timer / totaltime);
                            //Debug.Log(timer);
                        }
                    }
                    if (timer <= 0.0f)
                    {
                        // All ore has been processed and the batch of ingots is ready
                        ItemStack ingotStack = new ItemStack(newIngot.GetIngotType(), 9000, typeof(Consumable_Ingot));
                        for(int n = 0; n < ingotsAvailableToProcess; n++)
                        {
                            //Debug.Log("Adding "+newIngot);
                            ingotStack.AddItem(newIngot);
                        }
                        //Debug.Log("Outputting");
                        //Debug.Log(ingotStack);
                        outputMaterials.Add(ingotStack);
                        ingotsAvailableToProcess = 0;

                        //TEMP ONLY!! purge input materials (they've all been converted) THIS WILL WASTE LEFT-OVERS!! TEMP ONLY!!
                        inputMaterials.Clear();

                        //Attempt to push the ingots to Next or dump in output stack for player
                        if (smelter.OutputMode)
                        {
                            TransferToInternalInventory();
                            //true means to internal inventory

                            //false means to Next (NYI)
                        }
                        UpdateAll();
                        //Debug.Log("Materials left: "+inputMaterials[0]);
                    }
                    if (inputMaterials.Count == 0 && ingotsAvailableToProcess <= 0)
                    {
                        UpdateAll();
                        Debug.Log("Done STOPPING");
                        Stop = true;
                        //When all ore is processed, stop the update coroutine.
                        StopCoroutine(ProxyUpdate2());
                    }
                }
                else
                {
                    smelter.UpdateProductionPanel();
                    smelter.OnIngotsProduced(outputMaterials);
                    Stop = true;
                    Debug.Log("Pwr STOPPING");
                }

                yield return null;
            }
        }


        ///
        /// Process a certain amount of ore in one update
        ///
        private IEnumerator ProxyUpdate()
        {
            float remaining = inputMaterials[0].GetRealLength();
            float availableToProcess = 0f;
            IngotDefinition ingotDef = null;
            Dictionary<OreDefinition, float> oreInclusions;
            Dictionary<MaterialDefinition, float> matInclusions;
            int oreQuality;
            Consumable_Ingot newIngot = null;
            InductionSmelterUIController smelter = SmelterUI.GetComponent<InductionSmelterUIController>();

            while (remaining > 0f && inputMaterials[0].GetRealLength() >= 0 && !Stop)//the last ingot turns this into input == 0
            {
                if (isRunning && isPowered)
                {
                    if (inputMaterials.Count > 0f)
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
                            smelter.UpdateProgressBar(timer / totaltime);
                            //Debug.Log(timer);
                        }
                    }

                    ///
                    /// RIGHT NOW, 1 UNIT ORE = 1 INGOT WITH NO REDUCTION IN IMPURITIES OR BYPRODUCT PRODUCTION!!
                    ///
                    if (timer <= 0.0f)
                    {
                        remaining--;
                        //Ore was removed to process it. Only add the process amount to the ingot itemstack
                        ItemStack ingotStack = new ItemStack(ingotDef.GetIngotType(), 9000, typeof(Consumable_Ingot));
                        //Add the itemstack or whatever
                        ingotStack.AddItem(newIngot);
                        Debug.Log("Adding "+newIngot+" to ingotStack");
                        ///
                        /// THERE IS A HIGH LIKELYHOOD THAT NO INGOTS WILL BE STRICTLY EQUIVALENT IN METADATA DUE TO IMPURITIES!!
                        ///
                        
                        //Debug.Log("Outputting");
                        outputMaterials.Add(ingotStack);
                        //Attempt to push the ingots to Next or dump in output stack for player
                        if (smelter.OutputMode)
                        {
                            TransferToInternalInventory();
                            //true means to internal inventory

                            //false means to Next (NYI)
                        }
                        UpdateAll();
                        //Debug.Log("Materials left: "+inputMaterials[0]);
                    }
                    if (inputMaterials.Count == 0 && availableToProcess <= 0)
                    {
                        UpdateAll();
                        //Debug.Log("STOPPING");
                        Stop = true;
                        //When all ore is processed, stop the update coroutine.
                        StopCoroutine(ProxyUpdate());
                    }
                }
                else
                {
                    smelter.UpdateProductionPanel();
                    smelter.OnIngotsProduced(outputMaterials);
                    Stop = true;
                    //Debug.Log("STOPPING");
                }
                yield return null;
            }
            //Debug.Log("Termino");
        }

        public void UpdateAll()
        {
            InductionSmelterUIController smelter = SmelterUI.GetComponent<InductionSmelterUIController>();
            smelter.UpdateProductionPanel();
            smelter.UpdateAvailableOres();
            smelter.OnIngotsProduced(outputMaterials);
            //update producedItemCount
            if(OutputMaterials.Count > 0)
            {
                smelter.ProducedIngotCount.text = ""+OutputMaterials.Count;
            }
            else
            {
                smelter.ProducedIngotCount.text = "0";
                smelter.ProducedIngotName.text = "... Ingot";
            }
        }

        public void ExternalInteractFunc()
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
            {
                SmelterUI.GetComponent<InductionSmelterUIController>().LockScreenAndFreeCursor();
                SmelterUI.GetComponent<InductionSmelterUIController>().SetInputInventory(
                    networkedClient.PlayerObject.gameObject.GetComponent<IPlayer_Inventory>().GetPlayerInventory());
                SmelterUI.SetActive(true);
            }
        }

        //Try to load in a whole stack of ore for processing
        //ClientRpc?
        public void DisplaySmelterUI(GameObject player)
        {
            SmelterUI.GetComponent<InductionSmelterUIController>().LockScreenAndFreeCursor();
            SmelterUI.GetComponent<InductionSmelterUIController>().SetInputInventory(player.GetComponent<IPlayer_Inventory>().GetPlayerInventory());
            SmelterUI.SetActive(true);
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
                    StartCoroutine(ProxyUpdate2());//ProxyUpdate
                }

            }
        }

        public void TransferToInternalInventory()
        {
            for(int i  = OutputMaterials.Count - 1; i >= 0; i--)
            {
                
                if (!Inventory.IsFull())
                {
                    if(OutputMaterials[i] != null)
                    {
                        Debug.Log(OutputMaterials[i]);
                        Debug.Log("Transfer to internal");
                        Inventory.Add(OutputMaterials[i]);
                        OutputMaterials.RemoveAt(i);
                    }
                }
            }
            UpdateAll();
        }

        public void TransferInventoryToPlayer()
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
            {
                //Debug.Log("TTP");
                IPlayer_Inventory inv = networkedClient.PlayerObject.gameObject.GetComponent<IPlayer_Inventory>();
                //Debug.Log(Inventory.GetInventory().Count);
                for (int i = Inventory.GetInventory().Count - 1; i >= 0; i--)
                {
                    if(Inventory.GetInventory()[i] != null)
                    {
                        Debug.Log(Inventory.GetInventory()[i]);
                        inv.AddStackToPlayerInventory(Inventory.Remove(i));
                    }
                }
            }
            UpdateAll();
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