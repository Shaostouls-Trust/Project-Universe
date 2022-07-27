using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VideoTransButtonController : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private GameObject remoteControl;
    [SerializeField] private RemoteControlSelectorUI ui;
    [SerializeField] private TMP_Text nameText;
    
    public GameObject RemoteControlDrone
    {
        get { return remoteControl; }
        set { remoteControl = value; }
    }

    public RemoteControlSelectorUI Ui
    {
        get { return ui; }
        set { ui = value; }
    }

    public string NameText
    {
        get { return nameText.text; }
        set { nameText.text = value; }
    }
    
    public void SelectThisDrone()
    {
        if (Ui != null)
        {
            Ui.SelectDrone(RemoteControlDrone);
        }
    }

}
