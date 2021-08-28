using MLAPI;
using ProjectUniverse.Base;
using ProjectUniverse.Data.Libraries.Definitions;
using ProjectUniverse.Player;
using ProjectUniverse.Player.PlayerController;
using ProjectUniverse.Production.Machines;
using ProjectUniverse.Production.Resources;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectUniverse.UI
{
    public class InductionSmelterUIController : MonoBehaviour
    {
        private GameObject player;
        [SerializeField] private GameObject Factory;
        private List<ItemStack> suppliedOres = new List<ItemStack>();//from smelter inventory
        private List<ItemStack> transfererinventory;//player inventory or transport pipe
        [SerializeField] private List<ItemStack> nextInv = new List<ItemStack>();//inventory of ToNext
        /// <summary>
        /// ButtonEmpty
        ///     Image
        ///     Button
        ///         Text (TMP)
        ///     Count (TMP)
        /// </summary>
        public GameObject[] OreInsertionButtons;
        public GameObject OreRemoveButton;
        public Button OreToPlayerButton;
        public Button OreToNextButton;
        [SerializeField] private Color32 normalColor;
        [SerializeField] private bool OreToPlayer = true;//true is to player, false is to next (if next exists)

        public TMP_Text InsertedOreName;
        public TMP_Text InsertedOreCount;
        public TMP_Text ProducedIngotName;
        public TMP_Text ProducedIngotCount;

        public GameObject productionBar;
        //private int productionAmount;
        private bool startLock;
        public Button startButton;
        public Button stopButton;

        void Start()
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
            {
                player = networkedClient.PlayerObject.gameObject;
            }
            //UpdateProductionPanel();
            //for (int i = 0; i < OreInsertionButtons.Length; i++)
            //{
            //     only passed i = 20 once done, so had to manually declare:
            //}
            OreInsertionButtons[0].transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate { SortOreInsertByIndex(0); });
            OreInsertionButtons[1].transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate { SortOreInsertByIndex(1); });
            OreInsertionButtons[2].transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate { SortOreInsertByIndex(2); });
            OreInsertionButtons[3].transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate { SortOreInsertByIndex(3); });
            OreInsertionButtons[4].transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate { SortOreInsertByIndex(4); });
            OreInsertionButtons[5].transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate { SortOreInsertByIndex(5); });
            OreInsertionButtons[6].transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate { SortOreInsertByIndex(6); });
            OreInsertionButtons[7].transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate { SortOreInsertByIndex(7); });
            OreInsertionButtons[8].transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate { SortOreInsertByIndex(8); });
            OreInsertionButtons[9].transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate { SortOreInsertByIndex(9); });
            OreInsertionButtons[10].transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate { SortOreInsertByIndex(10); });
            OreInsertionButtons[11].transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate { SortOreInsertByIndex(11); });
            OreInsertionButtons[12].transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate { SortOreInsertByIndex(12); });
            OreInsertionButtons[13].transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate { SortOreInsertByIndex(13); });
            OreInsertionButtons[14].transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate { SortOreInsertByIndex(14); });
            OreInsertionButtons[15].transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate { SortOreInsertByIndex(15); });
            OreInsertionButtons[16].transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate { SortOreInsertByIndex(16); });
            OreInsertionButtons[17].transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate { SortOreInsertByIndex(17); });
            OreInsertionButtons[18].transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate { SortOreInsertByIndex(18); });
            OreInsertionButtons[19].transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate { SortOreInsertByIndex(19); });
            OreRemoveButton.transform.GetComponent<Button>().onClick.AddListener(delegate { RemoveOneFromSupply(); });
            OreToPlayerButton.onClick.AddListener(delegate { NextLocationIsPlayer(true); });
            OreToNextButton.onClick.AddListener(delegate { NextLocationIsPlayer(false); });
            startButton.onClick.AddListener(delegate { StartFactory(); });
            stopButton.onClick.AddListener(delegate { CloseUI(); });
            //transfererinventory = player.GetComponent<IPlayer_Inventory>().GetPlayerInventory();
            //UpdateAvailableOres();
        }


        public void SortOreInsertByIndex(int i)
        {
            string searchfor = "";
            switch (i)
            {
                case 0://try to put in Iron
                    searchfor = "Ore_Iron";
                    break;
                case 1://try to put in Copper
                    searchfor = "Ore_Copper";
                    break;
                case 2://try to put in Tin
                    searchfor = "Ore_Tin";
                    break;
                case 3://try to put in Zinc
                    searchfor = "Ore_Zinc";
                    break;
                case 4://try to put in Osmium
                    searchfor = "Ore_Osmium";
                    break;
                case 5://try to put in Iridium
                    searchfor = "Ore_Iridium";
                    break;
                case 6://try to put in Aluminum
                    searchfor = "Ore_Aluminum";
                    break;
                case 7://try to put in Lithium
                    searchfor = "Ore_Lithium";
                    break;
                case 8://try to put in Uranium
                    searchfor = "Ore_Uranium";
                    break;
                case 9://try to put in Gold
                    searchfor = "Ore_Gold";
                    break;
                case 10://try to put in Platinum
                    searchfor = "Ore_Platinum";
                    break;
                case 11://try to put in Plutonium
                    searchfor = "Ore_Plutonium";
                    break;
                case 12://try to put in Titanium
                    searchfor = "Ore_Titanium";
                    break;
                case 13://try to put in Palladium
                    searchfor = "Ore_Palladium";
                    break;
                case 14://try to put in Tungsten
                    searchfor = "Ore_Tungsten";
                    break;
                case 15://try to put in Nickel
                    searchfor = "Ore_Nickel";
                    break;
                case 16://try to put in Chromium
                    searchfor = "Ore_Chromium";
                    break;
                case 17://try to put in Molybdenum
                    searchfor = "Ore_Molybdenum";
                    break;
                case 18://try to put in Vanadium
                    searchfor = "Ore_Vanadium";
                    break;
                case 19://try to put in Manganese
                    searchfor = "Ore_Managanese";
                    break;
            }
            for (int j = 0; j < transfererinventory.Count; j++)
            {
                if (transfererinventory[j].GetStackType() == searchfor)
                {
                    ItemStack stack = transfererinventory[j].RemoveItemData(1f);
                    if (stack != null)
                    {
                        if (suppliedOres.Count > 0)
                        {
                            for (int k = 0; k < suppliedOres.Count; k++)
                            {
                                if (suppliedOres[k].GetStackType() == searchfor)
                                {
                                    suppliedOres[k].AddItemStack(stack);
                                    break;//Add only one, if that won't already be that case
                                }
                            }
                        }
                        else
                        {
                            suppliedOres.Add(stack);
                        }
                        startLock = true;
                    }
                }
            }
            if (startLock == true)
            {
                startButton.interactable = true;
                for (int k = 0; k < OreInsertionButtons.Length; k++)
                {
                    if (k != i)
                    {
                        OreInsertionButtons[k].transform.GetChild(1).GetComponent<Button>().interactable = false;
                    }
                }
            }
            UpdateAvailableOres();
            UpdateProductionPanel();
        }

        public void RemoveOneFromSupply()
        {
            if (suppliedOres.Count > 0)
            {
                for (int j = 0; j < transfererinventory.Count; j++)
                {
                    if (transfererinventory[j].GetStackType() == suppliedOres[0].GetStackType())
                    {
                        ItemStack stack = suppliedOres[0].RemoveItemData(1f);
                        //Debug.Log("supplied: " + suppliedOres[0]);
                        //Debug.Log("stack: " + stack);
                        if (stack != null)
                        {
                            transfererinventory[j].AddItemStack(stack);
                        }
                        //Debug.Log("remains: " + suppliedOres[0].GetRealLength());
                    }
                }
            }
            if (suppliedOres[0].GetRealLength() == 0)
            {
                suppliedOres.Clear();
            }
            //Debug.Log(suppliedOres.Count);
            if (suppliedOres.Count == 0)
            {
                startLock = false;
                startButton.interactable = false;
                for (int k = 0; k < OreInsertionButtons.Length; k++)
                {
                    OreInsertionButtons[k].transform.GetChild(1).GetComponent<Button>().interactable = true;
                }
            }
            UpdateAvailableOres();
            UpdateProductionPanel();
        }

        public bool OutputMode
        {
            get { return OreToPlayer; }
        }
        public void NextLocationIsPlayer(bool yn)
        {
            OreToPlayer = yn;
            //change color of normal
            if (OreToPlayer)
            {
                ColorBlock colorBlockPlayer = OreToPlayerButton.colors;
                colorBlockPlayer.normalColor = OreToPlayerButton.colors.selectedColor;
                ColorBlock colorBlockNext = OreToNextButton.colors;
                colorBlockNext.normalColor = normalColor;
                OreToPlayerButton.colors = colorBlockPlayer;
                OreToNextButton.colors = colorBlockNext;
            }
            else
            {
                ColorBlock colorBlockPlayer = OreToPlayerButton.colors;
                colorBlockPlayer.normalColor = normalColor;
                ColorBlock colorBlockNext = OreToNextButton.colors;
                colorBlockNext.normalColor = OreToNextButton.colors.selectedColor;
                OreToPlayerButton.colors = colorBlockPlayer;
                OreToNextButton.colors = colorBlockNext;
            }
        }

        public void UpdateProductionPanel()
        {
            if (suppliedOres.Count != 0 && suppliedOres[0] != null)
            {
                InsertedOreName.text = suppliedOres[0].GetStackType().Split('_')[1] + " Ore";
                InsertedOreCount.text = "" + suppliedOres[0].GetRealLength();
                //
            }
            else
            {
                InsertedOreName.text = "<Add Ore From Right>";
                InsertedOreCount.text = "0";

                if (suppliedOres[0].GetRealLength() == 0)
                {
                    suppliedOres.Clear();
                }

                //Debug.Log(suppliedOres.Count);
                if (suppliedOres.Count == 0)
                {
                    startLock = false;
                    startButton.interactable = false;
                    for (int k = 0; k < OreInsertionButtons.Length; k++)
                    {
                        OreInsertionButtons[k].transform.GetChild(1).GetComponent<Button>().interactable = true;
                    }
                }
            }

        }

        public void UpdateAvailableOres()
        {
            for (int k = 0; k < OreInsertionButtons.Length; k++)
            {
                OreInsertionButtons[k].transform.GetChild(2).GetComponent<TMP_Text>().text = "0";
            }
            for (int k = 0; k < OreInsertionButtons.Length; k++)
            {
                for (int j = 0; j < transfererinventory.Count; j++)
                {
                    //Debug.Log(transfererinventory[j].GetStackType().Split('_')[1] + " V " + OreInsertionButtons[k].transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text.Split(' ')[0]);
                    if (transfererinventory[j].GetStackType().Split('_')[1]
                        == OreInsertionButtons[k].transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text.Split(' ')[0])
                    {
                        OreInsertionButtons[k].transform.GetChild(2).GetComponent<TMP_Text>().text
                            = "" + transfererinventory[j].GetRealLength();
                    }
                }
            }
        }

        public void OnIngotsProduced(List<ItemStack> ingots)
        {
            ProducedIngotName.text = (ingots[0].GetItemArray().GetValue(0) as Consumable_Ingot).GetIngotType().Split('_')[1] + " Ingot";
            ProducedIngotCount.text = ""+ingots.Count;
        }

        public void SetInputInventory(List<ItemStack> inventory)
        {
            transfererinventory = inventory;
            UpdateAvailableOres();
        }

        // Update is called once per frame
        //void FixedUpdate()
        //{
            //transfererinventory = player.GetComponent<IPlayer_Inventory>().GetPlayerInventory();
            //UpdateAvailableOres();
        //}

        public void LockScreenAndFreeCursor()
        {
            if (player == null)
            {
                if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
                {
                    player = networkedClient.PlayerObject.gameObject;
                }
            }
            player.GetComponent<PlayerController>().LockAndFreeCursor();
        }

        public void CloseUI()
        {
            this.gameObject.SetActive(false);
            player.GetComponent<PlayerController>().UnlockCursor();
        }

        public void ResetProgressBar()
        {
            productionBar.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        }
        public void UpdateProgressBar(float timer)
        {
            productionBar.transform.localScale = new Vector3(timer, 1.0f, 1.0f);
        }

        public void StartFactory()
        {
            Mach_InductionFurnace factory = Factory.GetComponent<Mach_InductionFurnace>();
            factory.SetInputMaterials(suppliedOres);
            factory.StartFactory();
        }
    }
}