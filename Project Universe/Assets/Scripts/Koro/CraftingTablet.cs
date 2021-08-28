
using System;
using UnityEngine;
using System;
using System.Collections;
using Unity;

public class CraftingTablet : MonoBehaviour
{
    public const int GridSize = 3;
    public Animator anim;
    public int Hands;
    public bool TabletOn = false;
    public GameObject CraftBtn;
    public GameObject CraftEXT;
    public GameObject CraftingPanel;
    private string[,] recipe;
    public float numberofingots = 2f;
    public float ingots = 0f;
    public float numberofsticks = 1f;
    public float sticks = 0f;
    public GameObject Result;
    public GameObject IronSword;
    [HideInInspector]
    protected WeaponSwitching WS;
    // Start is called before the first frame update
    void Start()
    {

      /*  recipe[1, 1] = ItemScript.ItemType.None.ToString(); recipe[1, 2] = ItemScript.ItemType.iron.ToString(); recipe[1, 3] = ItemScript.ItemType.None.ToString();
        recipe[2, 1] = ItemScript.ItemType.None.ToString(); recipe[2, 2] = ItemScript.ItemType.iron.ToString(); recipe[2, 3] = ItemScript.ItemType.None.ToString();
        recipe[3, 1] = ItemScript.ItemType.None.ToString(); recipe[3, 2] = ItemScript.ItemType.Stick.ToString(); recipe[3, 3] = ItemScript.ItemType.None.ToString();*/
        WS = FindObjectOfType<WeaponSwitching>();
        
    }
    public void checkrecipe()
    {
        for (int k = 0; k < recipe.GetLength(0); k++)
        {
            for (int l = 0; l < recipe.GetLength(1); l++)
            {


            }

        }


    }

    // Update is called once per frame
    void Update()
    {
     
        if (Input.GetKeyUp(KeyCode.E))
        {
            OnPressCraftBtn();
            Debug.Log("deez");
            TabletOn = true;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

        }
        if (Input.GetKeyUp(KeyCode.Q) && TabletOn)
        {
            OnPressExitTabletBtn();
            Debug.Log("deez2");
            TabletOn = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            CraftingPanel.SetActive(false);


        }
    }
    public void OnPressCraftBtn()
    {
        anim.SetBool("y", true);
        WS.selectedWeapon = Hands;
        WS.SelectWeaponServerRpc();
        //WS.SelectWeapon();// ServerRpc();
        TabletOn = true;
        CraftBtn.SetActive(false);
        CraftEXT.SetActive(true);
    }

    public void OnPressExitTabletBtn()
    {
        CraftingPanel.SetActive(false);
        anim.SetBool("y", false);
        CraftBtn.SetActive(true);
        CraftEXT.SetActive(false);

    }
    public void TurnOnCraftingPanel()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        CraftingPanel.SetActive(true);

    }
    public void OnPressBuild()
    {
        

    }
    public void RecieveMaterialInfo(  string ID, string pos)
    {

        Debug.Log(ID);
        Debug.Log(pos);
        if (ID == "Iron Ingot" && pos == "(1195.1, 517.3, 9.1)" || ID == "Iron Ingot" && pos == "(1195.1, 418.5, 9.1)")
        {
            ingots += 1f;
            Debug.Log("First ingot has been placed and your still a dumbass");

        }
        else
        {
            if(ID == "Stick" && pos == "(1195.1, 317.0, 9.1)")
            {
                sticks += 1f;
                Debug.Log("AYYYEEEE");
                if (sticks == numberofsticks && ingots == numberofingots)
                {
                    Debug.Log("LETS FRICKIN GOOO");
                
                    var go = Instantiate(IronSword, Result.transform.position, Quaternion.identity);
                    var component = go.GetComponent<sword>();
                    IronSword = component.gameObject;
                    IronSword.transform.parent = CraftingPanel.transform;

                }
            }
        }
        
    }
}



