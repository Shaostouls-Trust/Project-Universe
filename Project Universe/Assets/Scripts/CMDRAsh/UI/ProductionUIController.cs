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
using System.Text.RegularExpressions;

namespace ProjectUniverse.UI
{
    public class ProductionUIController : MonoBehaviour
    {
        private GameObject player;
        [SerializeField] private GameObject Factory;
        public TMP_Text recipeList;
        public TMP_InputField productionCount;
        public Button startButton;
        public Button stopButton;
        public GameObject productionBar;
        public GameObject buttonPrefab;
        public GameObject buttonParent;
        private List<ItemStack> inputmats;
        private List<IComponentDefinition> compDefs = new List<IComponentDefinition>();
        private int productionAmount = 1;
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
            for (int i = buttonParent.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(buttonParent.transform.GetChild(i).gameObject);
            }

            int c = 0;
            //for every component in the ComponentsLibrary, add an option
            foreach (IComponentDefinition compDef in IComponentLibrary.ComponentDictionary.Values)
            {
                compDefs.Add(compDef);
                string nameJoined = compDef.GetComponentType().Split('_')[1];//split off "Component_"
                //put a space between all caps
                var reg = new Regex(@"
                (?<=[A-Z])(?=[A-Z][a-z]) |
                (?<=[^A-Z])(?=[A-Z]) |
                (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);
                string nameSpaced = reg.Replace(nameJoined, " ");
                //instanciate a button
                GameObject bttn = Instantiate(buttonPrefab, buttonParent.transform);
                bttn.transform.GetChild(0).GetComponent<TMP_Text>().text = nameSpaced;
                //OnClick pass int to button. Use int to get component def
                int a = c;
                c++;
                bttn.GetComponent<Button>().onClick.AddListener(delegate { OnComponentSelect(a); });
            }
            //display the recipe of the first selected item
            productionCount.text = "1";
            productionCount.onValueChanged.AddListener(delegate { UpdateProductionCount(); });
            startButton.onClick.AddListener(delegate { StartFactory(); });
            stopButton.onClick.AddListener(delegate { CloseUI(); });
        }

        public void OnComponentSelect(int idx)
        {
            //Debug.Log(idx);
            string str = compDefs[idx].GetComponentType();
            Factory.GetComponent<Mach_DevFactory>().ProduceComponent = str;
            component = compDefs[idx];
            //Debug.Log("Selected: " + str);
            UpdateUI();
        }

        public void DropDownListener()//TMP_Dropdown dropMenu
        {
            //get the component's requirements

            int index = 0;//dropDownMenu.value;
            string str = "Component_";// +dropDownMenu.options[index].text;
            Factory.GetComponent<Mach_DevFactory>().ProduceComponent = str;
            IComponentLibrary.ComponentDictionary.TryGetValue(str, out component);
            Debug.Log("Selected: "+str);
            UpdateUI();
        }

        public void UpdateUI()
        {
            List<(IComponentDefinition, int)> compRecipe = component.GetComponentRecipeList();
            List<(IngotDefinition, float)> ingotRecipe = component.GetIngotRecipeList();
            List<(MaterialDefinition, float)> matRecipe = component.GetMaterialRecipeList();
            canProduce = true;

            ResetProgressBar();
            //UpdateProductionInventory();
            //DropDownListener();

            List<(string, int)> compsIn = new List<(string, int)>();

            //fill the compsIn temp tuple list with the contents of the player production inventory
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
                for (int r = 0; r < compsIn.Count; r++)
                {
                    if (compRecipe[i].Item1.GetComponentType() == compsIn[r].Item1)
                    {
                        count += compsIn[r].Item2;
                    }
                }
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
                for (int r = 0; r < compsIn.Count; r++)
                {
                    if (ingotRecipe[j].Item1.GetIngotType() == compsIn[r].Item1)
                    {
                        count += compsIn[r].Item2;
                    }
                }
                compile += ingotRecipe[j].Item1.GetIngotType() + ": " + count + "/" + (ingotRecipe[j].Item2 * productionAmount) + "\n";
                if (count < ingotRecipe[j].Item2 * productionAmount)
                {
                    canProduce = false;
                }
            }
            count = 0;
            for (int k = 0; k < matRecipe.Count; k++)
            {
                for (int r = 0; r < compsIn.Count; r++)
                {
                    if (matRecipe[k].Item1.GetMaterialType() == compsIn[r].Item1)
                    {
                        count += compsIn[r].Item2;
                    }
                }
                compile += matRecipe[k].Item1.GetMaterialType() + ": " + count + "/" + (matRecipe[k].Item2 * productionAmount) + "\n";
                if (count < matRecipe[k].Item2 * productionAmount)
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
