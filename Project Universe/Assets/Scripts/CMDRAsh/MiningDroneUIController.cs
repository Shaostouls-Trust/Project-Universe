using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MiningDroneUIController : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler
{
    public TMP_Text Timer;
    public TMP_Text OreList;
    //public TMP_Text AvailableAsteroids;
    public Button startButton;
    public GameObject drone;
    public GameObject asteroidButtonPref;
    public GameObject asteroidButtonFrame;
    public DevOreAsteroid TargetAsteroid;
    private float timeReal;
    private bool running;

    // Start is called before the first frame update
    void Start()
    {
        UpdateAsteroidRegistry();
        if(TargetAsteroid != null)
        {
            CompileOreList();
            timeReal = int.Parse(Timer.text);
        }
        else
        {
            timeReal = 0f;
            Timer.text = "0";
            OreList.text = "<Select Asteroid>";
        }
        startButton.onClick.AddListener(delegate { LaunchMiner(); });
    }

    // Update is called once per frame
    void Update()
    {
        if (running)
        {
            if(timeReal <= 0)
            {
                running = false;
                drone.SetActive(true);
                //create ores of X type, 100Kg for each ore type present
                int[] OreQualities = TargetAsteroid.GetOreQualities();
                int[] OreZones = TargetAsteroid.GetOreZones();
                string[] OreTypes = TargetAsteroid.GetOreTypes();
                int[] OreMasses = TargetAsteroid.GetOreMasses();

                int amount = 100;
                
                ItemStack[] oreList = new ItemStack[OreTypes.Length];
                for (int o = 0;o < OreTypes.Length; o++)
                {
                    if((OreMasses[0] - 100) < 0)
                    {
                        amount = OreMasses[0];
                    }
                    else
                    {
                        amount = 100;
                    }
                    Consumable_Ore ore = new Consumable_Ore(OreTypes[o], OreQualities[o], OreZones[o], amount);
                    ItemStack orestack = new ItemStack(OreTypes[o],100,typeof(Consumable_Ore));
                    orestack.AddItem(ore);
                    TargetAsteroid.SetOreMass(o,OreMasses[o] - 100);
                    oreList[o] = orestack;
                }
                CompileOreList();
                timeReal = 10f;
                Timer.text = "10";
                drone.GetComponent<IMiningDrone>().AddOreInventory(oreList);
            }
            else
            {
                timeReal -= Time.deltaTime;
                Timer.text = "" + Mathf.Round(timeReal);
            }
        }
    }

    public void SelectAsteroid(string asteroidName)
    {
        foreach(DevOreAsteroid ast in FindObjectsOfType<DevOreAsteroid>())
        {
            if(ast.GetAsteroidName() == asteroidName)
            {
                TargetAsteroid = ast;
                CompileOreList();
                break;
            }
        }
    }


    public void UpdateAsteroidRegistry()
    {
        string compile = "";
        //eventually this will need to be "all asteroids within range of drone"
        DevOreAsteroid[] asters = FindObjectsOfType<DevOreAsteroid>();
        Debug.Log(asters.Length);
        float newy = 0;
        foreach (DevOreAsteroid ast in asters)
        {
            GameObject instanceButton = (GameObject)Instantiate(asteroidButtonPref);
            instanceButton.transform.SetParent(asteroidButtonFrame.transform);
            instanceButton.transform.localPosition = new Vector3(0, newy, 0);
            instanceButton.transform.localRotation = Quaternion.Euler(0, 0, 0);
            newy -= 0.0325f;
            instanceButton.GetComponent<Button>().onClick.AddListener(delegate { SelectAsteroid(ast.GetAsteroidName()); });
            instanceButton.transform.GetChild(0).GetComponent<TMP_Text>().text = ast.GetAsteroidName();
            //compile += ast.GetAsteroidName() + "\n";
        }
        //AvailableAsteroids.text = compile;
    }

    private void CompileOreList()
    {
        OreList.text = "";
        int[] OreQualities = TargetAsteroid.GetOreQualities();
        //int[] OreZones = TargetAsteroid.GetOreZones();
        string[] OreTypes = TargetAsteroid.GetOreTypes();
        int[] OreMasses = TargetAsteroid.GetOreMasses();

        string partial = "";
        for (int i = 0; i < OreTypes.Length; i++)
        {
            partial += (OreTypes[i].Split('_')[1]);
            partial += " > \n";

            partial += ("Quality: "+OreQualities[i]+"\n");

            partial += ("Mass: ~"+OreMasses[i]+"Kg \n");
            partial += "\n";

            OreList.text += partial;
            partial = "";
        }

    }

    public void LaunchMiner()
    {
        drone.SetActive(false);
        timeReal -= Time.deltaTime;
        Timer.text = ""+Mathf.Round(timeReal);
        running = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //Debug.Log("ENTER");
        //show the mouse to let the player click the button
        Cursor.visible = true;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        Cursor.visible = false;
    }
}
