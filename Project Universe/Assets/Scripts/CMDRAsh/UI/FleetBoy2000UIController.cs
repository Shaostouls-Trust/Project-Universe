using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FleetBoy2000UIController : MonoBehaviour
{
    [SerializeField] private Button youBttn;
    [SerializeField] private Button invBttn;
    [SerializeField] private Button remoBttn;
    [SerializeField] private Button mapBttn;
    [SerializeField] private Button cmndBttn;
    [SerializeField] private Button transBttn;
    [SerializeField] private Color32 topDefaultColor;
    [SerializeField] private Color32 topCmndColor;
    
    [SerializeField] private GameObject YOUTAB;
    [SerializeField] private GameObject INVTAB;
    [SerializeField] private GameObject REMOTAB;
    [SerializeField] private GameObject MAPTAB;
    [SerializeField] private GameObject CMNDTAB;
    [SerializeField] private GameObject TRANSTAB;

    //You tab
    public RenderTexture playerRender;
    [SerializeField] private TMP_Text head;
    [SerializeField] private TMP_Text chest;
    [SerializeField] private TMP_Text Larm;
    [SerializeField] private TMP_Text Rarm;
    [SerializeField] private TMP_Text Lhand;
    [SerializeField] private TMP_Text Rhand;
    [SerializeField] private TMP_Text Lleg;
    [SerializeField] private TMP_Text Rleg;
    [SerializeField] private TMP_Text Lfoot;
    [SerializeField] private TMP_Text Rfoot;
    [SerializeField] private GameObject radsBar;//Non-standard health
    [SerializeField] private GameObject hydrationBar;
    [SerializeField] private GameObject foodBar;
    [SerializeField] private Button perksExpandBttn;
    [SerializeField] private Button perksRetrackBttn;
    [SerializeField] private GameObject perksPanel;

    // Start is called before the first frame update
    void Start()
    {
        youBttn.onClick.AddListener(delegate { SetTopBarButton0Color(); ChangeActiveDisplay(0); });
        invBttn.onClick.AddListener(delegate { SetTopBarButton1Color(); ChangeActiveDisplay(1); });
        remoBttn.onClick.AddListener(delegate { SetTopBarButton2Color(); ChangeActiveDisplay(2); });
        mapBttn.onClick.AddListener(delegate { SetTopBarButton3Color(); ChangeActiveDisplay(3); });
        cmndBttn.onClick.AddListener(delegate { SetTopBarButton4Color(); ChangeActiveDisplay(4); });
        transBttn.onClick.AddListener(delegate { SetTopBarButton5Color(); ChangeActiveDisplay(5); });

        perksPanel.SetActive(false);
        perksRetrackBttn.gameObject.SetActive(false);
        perksExpandBttn.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SetTopBarButton0Color()
    {
        ColorBlock youCB = youBttn.colors;
        youCB.normalColor = youCB.selectedColor;
        ColorBlock invCB = invBttn.colors;
        invCB.normalColor = topDefaultColor;
        ColorBlock cmndCB = cmndBttn.colors;
        cmndCB.normalColor = topCmndColor;
        youBttn.colors = youCB;
        invBttn.colors = invCB;
        remoBttn.colors = invCB;
        mapBttn.colors = invCB;
        cmndBttn.colors = cmndCB;
        transBttn.colors = invCB;
    }
    private void SetTopBarButton1Color()
    {
        ColorBlock youCB = youBttn.colors;
        youCB.normalColor = topDefaultColor;
        ColorBlock invCB = invBttn.colors;
        invCB.normalColor = invCB.selectedColor;
        ColorBlock cmndCB = cmndBttn.colors;
        cmndCB.normalColor = topCmndColor;
        youBttn.colors = youCB;
        invBttn.colors = invCB;
        remoBttn.colors = youCB;
        mapBttn.colors = youCB;
        cmndBttn.colors = cmndCB;
        transBttn.colors = youCB;
    }
    private void SetTopBarButton2Color()
    {
        ColorBlock invCB = invBttn.colors;
        invCB.normalColor = topDefaultColor;
        ColorBlock remoCB = remoBttn.colors;
        remoCB.normalColor = remoCB.selectedColor;
        ColorBlock cmndCB = cmndBttn.colors;
        cmndCB.normalColor = topCmndColor;
        youBttn.colors = invCB;
        invBttn.colors = invCB;
        remoBttn.colors = remoCB;
        mapBttn.colors = invCB;
        cmndBttn.colors = cmndCB;
        transBttn.colors = invCB;
    }
    private void SetTopBarButton3Color()
    {
        ColorBlock invCB = invBttn.colors;
        invCB.normalColor = topDefaultColor;
        ColorBlock mapCB = mapBttn.colors;
        mapCB.normalColor = mapCB.selectedColor;
        ColorBlock cmndCB = cmndBttn.colors;
        cmndCB.normalColor = topCmndColor;
        youBttn.colors = invCB;
        invBttn.colors = invCB;
        remoBttn.colors = invCB;
        mapBttn.colors = mapCB;
        cmndBttn.colors = cmndCB;
        transBttn.colors = invCB;
    }
    private void SetTopBarButton4Color()
    {
        ColorBlock invCB = invBttn.colors;
        invCB.normalColor = topDefaultColor;
        ColorBlock cmndCB = cmndBttn.colors;
        cmndCB.normalColor = cmndCB.selectedColor;
        youBttn.colors = invCB;
        invBttn.colors = invCB;
        remoBttn.colors = invCB;
        mapBttn.colors = invCB;
        cmndBttn.colors = cmndCB;
        transBttn.colors = invCB;
    }
    private void SetTopBarButton5Color()
    {
        ColorBlock invCB = invBttn.colors;
        invCB.normalColor = topDefaultColor;
        ColorBlock cmndCB = cmndBttn.colors;
        cmndCB.normalColor = topCmndColor;
        ColorBlock transCB = transBttn.colors;
        transCB.normalColor = transCB.selectedColor;
        youBttn.colors = invCB;
        invBttn.colors = invCB;
        remoBttn.colors = invCB;
        mapBttn.colors = invCB;
        cmndBttn.colors = cmndCB;
        transBttn.colors = transCB;
    }

    private void ChangeActiveDisplay(int i)
    {
        switch (i)
        {
            case 0:
                YOUTAB.SetActive(true);
                INVTAB.SetActive(false);
                REMOTAB.SetActive(false);
                CMNDTAB.SetActive(false);
                MAPTAB.SetActive(false);
                TRANSTAB.SetActive(false);
                break;
            case 1:
                YOUTAB.SetActive(false);
                INVTAB.SetActive(true);
                REMOTAB.SetActive(false);
                CMNDTAB.SetActive(false);
                MAPTAB.SetActive(false);
                TRANSTAB.SetActive(false);
                break;
            case 2:
                YOUTAB.SetActive(false);
                INVTAB.SetActive(false);
                REMOTAB.SetActive(true);
                CMNDTAB.SetActive(false);
                MAPTAB.SetActive(false);
                TRANSTAB.SetActive(false);
                break;
            case 3:
                YOUTAB.SetActive(false);
                INVTAB.SetActive(false);
                REMOTAB.SetActive(false);
                CMNDTAB.SetActive(true);
                MAPTAB.SetActive(false);
                TRANSTAB.SetActive(false);
                break;
            case 4:
                YOUTAB.SetActive(false);
                INVTAB.SetActive(false);
                REMOTAB.SetActive(false);
                CMNDTAB.SetActive(false);
                MAPTAB.SetActive(true);
                TRANSTAB.SetActive(false);
                break;
            case 5:
                YOUTAB.SetActive(false);
                INVTAB.SetActive(false);
                REMOTAB.SetActive(false);
                CMNDTAB.SetActive(false);
                MAPTAB.SetActive(false);
                TRANSTAB.SetActive(true);
                break;
        }
    }

    private void ExtendPerksPanel()
    {

    }
    private void RetractPerksPanel()
    {

    }
}
