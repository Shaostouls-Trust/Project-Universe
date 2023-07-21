using Unity.Netcode;
using ProjectUniverse.Base;
using ProjectUniverse.Data.Libraries;
using ProjectUniverse.Data.Libraries.Definitions;
using ProjectUniverse.Player.PlayerController;
using ProjectUniverse.Production.Machines;
using ProjectUniverse.Util;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectUniverse.UI
{
    public class AlloyForgeUIController : MonoBehaviour
    {
        private GameObject player;
        [SerializeField] private GameObject Forge;
        public TMP_Text recipeList;
        public TMP_InputField productionCount;
        public Button startButton;
        public Button stopButton;
        public GameObject productionBar;
        private IngotDefinition ingotDef;
        private List<ItemStack> inputmats;
        private int productionAmount = 1;
        private string[] rcp;
        private bool canProduce;
        private bool startLock;

        void Start()
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
            {
                player = networkedClient.PlayerObject.gameObject;
            }
            productionAmount = 1;
            canProduce = true;
            //startLock = false;

            //display the recipe of the first selected item
            productionCount.text = "1";
            productionCount.onValueChanged.AddListener(delegate { UpdateProductionCount(); });
            startButton.onClick.AddListener(delegate { StartFactory(); });
            stopButton.onClick.AddListener(delegate { CloseUI(); });
        }

        public void SelectSteel()
        {
            rcp = Utils.AlloyRecipeMatrix(0);
            IngotLibrary.IngotDictionary.TryGetValue("Ingot_Steel", out ingotDef);
            Forge.GetComponent<Mach_AlloyForge>().ProduceIngot = ingotDef;
            UpdateUI();
        }
        public void SelectChromeSteel()
        {
            rcp = Utils.AlloyRecipeMatrix(1);
            IngotLibrary.IngotDictionary.TryGetValue("Ingot_ChromeSteel", out ingotDef);
            Forge.GetComponent<Mach_AlloyForge>().ProduceIngot = ingotDef;
            UpdateUI();
        }
        public void SelectBrass()
        {
            rcp = Utils.AlloyRecipeMatrix(2);
            IngotLibrary.IngotDictionary.TryGetValue("Ingot_Brass", out ingotDef);
            Forge.GetComponent<Mach_AlloyForge>().ProduceIngot = ingotDef;
            UpdateUI();
        }
        public void SelectInvar()
        {
            rcp = Utils.AlloyRecipeMatrix(3);
            IngotLibrary.IngotDictionary.TryGetValue("Ingot_Invar", out ingotDef);
            Forge.GetComponent<Mach_AlloyForge>().ProduceIngot = ingotDef;
            UpdateUI();
        }

        public void UpdateUI()
        {
            //get the ingots and mats that go into the selected alloy
            List<IngotDefinition> ingotRecipe = new List<IngotDefinition>();
            List<MaterialDefinition> matRecipe = new List<MaterialDefinition>();
            IngotDefinition tempIng;
            MaterialDefinition tempMat;
            if (IngotLibrary.IngotDictionary.TryGetValue(rcp[0], out tempIng))
            {
                ingotRecipe.Add(tempIng);
            }
            else if (OreLibrary.MaterialDictionary.TryGetValue(rcp[0], out tempMat))
            {
                matRecipe.Add(tempMat);
            }
            if (IngotLibrary.IngotDictionary.TryGetValue(rcp[1], out tempIng))
            {
                ingotRecipe.Add(tempIng);
            }
            else if (OreLibrary.MaterialDictionary.TryGetValue(rcp[1], out tempMat))
            {
                matRecipe.Add(tempMat);
            }
            Forge.GetComponent<Mach_AlloyForge>().IngotRecipe = ingotRecipe;
            Forge.GetComponent<Mach_AlloyForge>().MaterialRecipe = matRecipe;

            canProduce = true;
            ResetProgressBar();
            List<(string, int)> compsIn = new List<(string, int)>();

            //fill the compsIn temp tuple list with the contents of the player production inventory
            for (int a = 0; a < inputmats.Count; a++)
            {
                compsIn.Add((inputmats[a].GetStackType(), (int)inputmats[a].Size()));
            }

            int count = 0;
            string compile = "";
            for (int j = 0; j < ingotRecipe.Count; j++)
            {
                for (int r = 0; r < compsIn.Count; r++)
                {
                    if (ingotRecipe[j].GetIngotType() == compsIn[r].Item1)
                    {
                        count += compsIn[r].Item2;
                    }
                }
                compile += ingotRecipe[j].GetIngotType() + ": " + count + "/" + (productionAmount) + "\n";
                if (count < productionAmount)//ingotRecipe[j].Item2 *
                {
                    canProduce = false;
                }
            }
            count = 0;
            for (int k = 0; k < matRecipe.Count; k++)
            {
                for (int r = 0; r < compsIn.Count; r++)
                {
                    if (matRecipe[k].GetMaterialType() == compsIn[r].Item1)
                    {
                        count += compsIn[r].Item2;
                    }
                }
                compile += matRecipe[k].GetMaterialType() + ": " + count + "/" + (productionAmount) + "\n";
                if (count < productionAmount)
                {
                    canProduce = false;
                }
            }
            //set the compile string
            recipeList.text = compile;
        }

        public void UpdateInputMaterials(List<ItemStack> input)
        {
            inputmats = input;
            //unlock production button
            //startLock = !startLock;
            //startButton.interactable = true;
        }

        public void UpdateProductionCount()
        {
            try
            {
                productionAmount = int.Parse(productionCount.text);
                UpdateUI();
            }
            catch (System.FormatException)
            {
                productionAmount = 0;
            }
        }

        public void LockScreenAndFreeCursor()
        {
            if (player == null)
            {
                if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
                {
                    player = networkedClient.PlayerObject.gameObject;
                }
            }
            player.GetComponent<SupplementalController>().LockScreenAndFreeCursor();
        }

        public void CloseUI()
        {
            this.gameObject.SetActive(false);
            player.GetComponent<SupplementalController>().FreeScreenAndLockCursor();
        }

        public void StartFactory()
        {
            //A bug with UIToolkit or Unity is calling button functions twice, so we will lock the StartFactory Function
            //until the UI updates
            //if (!startLock)
            //{
                if (Forge.TryGetComponent<Mach_AlloyForge>(out var outParam))
                {
                    //startButton.interactable = false;
                    outParam.StartFactory();
                }
            //}
            //else
            //{
            //    startLock = !startLock;
            //    startButton.interactable = true;
            //}
        }

        public void SetStartButton(bool state)
        {
            startButton.interactable = state;
        }

        public int GetProductionAmount()
        {
            return productionAmount;
        }

        public void ResetProgressBar()
        {
            productionBar.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        }

        public void UpdateProgressBar(float timer)
        {
            productionBar.transform.localScale = new Vector3(timer, 1.0f, 1.0f);
        }

        public bool CanProduceRequested()
        {
            return canProduce;
        }
    }
}