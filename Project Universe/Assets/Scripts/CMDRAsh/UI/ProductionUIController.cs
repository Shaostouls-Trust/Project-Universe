using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectUniverse.Player.PlayerController;
using ProjectUniverse.Base;
using ProjectUniverse.Production.Machines;
using ProjectUniverse.Data.Libraries.Definitions;
using ProjectUniverse.Data.Libraries;
using MLAPI;

namespace ProjectUniverse.UI
{
    public class ProductionUIController : MonoBehaviour
    {
        private GameObject player;
        [SerializeField] private GameObject Factory;
        public TMP_Text recipeList;
        public TMP_Dropdown dropDownMenu;
        public TMP_InputField productionCount;
        public TMP_Text prodInventory;
        public Button startButton;
        public Button stopButton;
        public GameObject productionBar;
        private List<ItemStack> inputmats;
        private int productionAmount;
        private IComponentDefinition component;
        private bool canProduce;
        private bool startLock;

        // Start is called before the first frame update
        void Start()
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
            {
                player = networkedClient.PlayerObject.gameObject;
            }
            //temp
            productionAmount = 1;
            canProduce = true;
            startLock = false;
            //clear options
            dropDownMenu.options.Clear();
            //for every component in the ComponentsLibrary, add an option
            foreach (IComponentDefinition compDef in IComponentLibrary.ComponentDictionary.Values)
            {
                TMP_Dropdown.OptionData tmpOpDat = new TMP_Dropdown.OptionData(compDef.GetComponentType().Split('_')[1]);//split off "Component_"
                dropDownMenu.options.Add(tmpOpDat);
            }
            //display the recipe of the first selected item
            productionCount.text = "1";
            /// DropDownListener must be called first.
            DropDownListener();//dropDownMenu
            UpdateProductionCount();
            UpdateProductionInventory();
            //now register an event caller for the dropdown
            dropDownMenu.onValueChanged.AddListener(delegate { DropDownListener(); });//dropDownMenu
            productionCount.onValueChanged.AddListener(delegate { UpdateProductionCount(); });
            startButton.onClick.AddListener(delegate { StartFactory(); });
            stopButton.onClick.AddListener(delegate { CloseUI(); });
        }

        public void DropDownListener()//TMP_Dropdown dropMenu
        {
            //get the component's requirements

            int index = dropDownMenu.value;
            string str = "Component_"+dropDownMenu.options[index].text;
            Factory.GetComponent<Mach_DevFactory>().ProduceComponent = str;
            IComponentLibrary.ComponentDictionary.TryGetValue(str, out component);
            Debug.Log(str);
            UpdateUI();
        }

        public void UpdateUI()
        {
            List<(IComponentDefinition, int)> compRecipe = component.GetComponentRecipeList();
            List<(IngotDefinition, float)> ingotRecipe = component.GetIngotRecipeList();
            List<(MaterialDefinition, float)> matRecipe = component.GetMaterialRecipeList();
            canProduce = true;

            ResetProgressBar();
            UpdateProductionInventory();
            //DropDownListener();

            List<(string, int)> compsIn = new List<(string, int)>();

            for (int a = 0; a < inputmats.Count; a++)
            {
                //if (inputmats[a].GetOriginalType() == typeof(IComponentDefinition))
                //{
                compsIn.Add((inputmats[a].GetStackType(), (int)inputmats[a].Size()));
                //}

            }

            int count = 0;
            string compile = "";
            for (int i = 0; i < compRecipe.Count; i++)
            {
                //Debug.Log(compRecipe[i].Item1.GetComponentType());
                for (int r = 0; r < compsIn.Count; r++)
                {
                    //Debug.Log(compRecipe[i].Item1.GetComponentType() + " == " + compsIn[r].Item1);
                    if (compRecipe[i].Item1.GetComponentType() == compsIn[r].Item1)
                    {
                        count += compsIn[r].Item2;
                    }
                }
                //Debug.Log("CompRecipe count: "+count);
                compile += compRecipe[i].Item1.GetComponentType() + ": " + count + "/" + (compRecipe[i].Item2 * productionAmount) + "\n";
                //if we lack the required materials
                if (count < compRecipe[i].Item2 * productionAmount)
                {
                    canProduce = false;
                }
                //if any of the canProduce runs come out as false, it will remain false for all cases.
            }
            count = 0;
            for (int j = 0; j < ingotRecipe.Count; j++)
            {
                //Debug.Log(ingotRecipe[j].Item1.GetIngotType());
                for (int r = 0; r < compsIn.Count; r++)
                {
                    //Debug.Log(ingotRecipe[j].Item1.GetIngotType() + " == " + compsIn[r].Item1);
                    if (ingotRecipe[j].Item1.GetIngotType() == compsIn[r].Item1)
                    {
                        count += compsIn[r].Item2;
                    }
                }
                // Debug.Log("IngotRecipe count: " + count);
                compile += ingotRecipe[j].Item1.GetIngotType() + ": " + count + "/" + (ingotRecipe[j].Item2 * productionAmount) + "\n";
                if (count < ingotRecipe[j].Item2 * productionAmount)
                {
                    canProduce = false;
                }
            }
            count = 0;
            for (int k = 0; k < matRecipe.Count; k++)
            {
                //Debug.Log(matRecipe[k].Item1.GetMaterialType());
                for (int r = 0; r < compsIn.Count; r++)
                {
                    //Debug.Log(compRecipe[i].Item1.GetComponentType() + " == " + compsIn[r].Item1);
                    if (matRecipe[k].Item1.GetMaterialType() == compsIn[r].Item1)
                    {
                        count += compsIn[r].Item2;
                    }
                }
                //Debug.Log("MatRecipe count: " + count);
                compile += matRecipe[k].Item1.GetMaterialType() + ": " + count + "/" + (matRecipe[k].Item2 * productionAmount) + "\n";
                if (count < matRecipe[k].Item2 * productionAmount)
                {
                    canProduce = false;
                }
            }
            //set the compile string
            recipeList.text = compile;
            //unlock production button
            startLock = !startLock;
            startButton.interactable = true;
        }

        public void UpdateProductionInventory()//List<ItemStack> inputMats
        {
            string compile = "";
            for (int k = 0; k < inputmats.Count; k++)
            {
                compile += "" + inputmats[k].GetStackType() + ": " + inputmats[k].Size() + "\n";
            }
            //Debug.Log("inventory compile: "+compile);
            prodInventory.text = compile;
        }

        public void UpdateInputMaterials(List<ItemStack> input)
        {
            inputmats = input;
            //unlock production button
            startLock = !startLock;
            startButton.interactable = true;
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
            player.GetComponent<PlayerController>().LockAndFreeCursor();
        }

        public void CloseUI()
        {
            this.gameObject.SetActive(false);
            player.GetComponent<PlayerController>().UnlockCursor();
        }

        public void StartFactory()
        {
            //A bug with UIToolkit or Unity is calling button functions twice, so we will lock the StartFactory Function
            //until the UI updates
            if (!startLock)
            {
                if (Factory.TryGetComponent<Mach_DevFactory>(out var outParam))
                {
                    startButton.interactable = false;
                   
                    outParam.StartFactory();
                }
            }
            else
            {
                startLock = !startLock;
                startButton.interactable = true;
            }
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
