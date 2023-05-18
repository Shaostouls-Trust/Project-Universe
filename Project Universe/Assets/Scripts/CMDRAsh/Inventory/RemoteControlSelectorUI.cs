using ProjectUniverse.Player.PlayerController;
using ProjectUniverse.Ship;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RemoteControlSelectorUI : MonoBehaviour
{
    [SerializeField] private GameObject content;
    private bool connected;
    [SerializeField] private GameObject VTOButtonPrefab;
    private List<GameObject> buttons = new List<GameObject>();
    [SerializeField] private TMP_Text connectionText;
    [SerializeField] private GameObject connectButton;
    private GameObject targetDrone;
    [SerializeField] private SphereCollider col;

    private void Start()
    {
        col.enabled = true;
    }

    private void OnEnable()
    {
        col.enabled = true;
    }

    private void OnDisable()
    {
        // for every gameobject in buttons, if other.gameobject is in buttons, remove it
        for(int a = buttons.Count-1; a >= 0; a--)
        {
            Destroy(buttons[a]);
            buttons.Remove(buttons[a]);
        }
        connectionText.text = "<No Connection>";
        connectionText.color = Color.white;
        connectButton.SetActive(false);
        targetDrone = null;
        col.enabled = false;
    }

    public void ProxyTriggerEnter(Collider other)
    {
        //Debug.Log("PTE "+other.gameObject);
        GameObject newButton = Instantiate(VTOButtonPrefab, content.transform);
        buttons.Add(newButton);
        VideoTransButtonController vtbc = newButton.GetComponent<VideoTransButtonController>();
        vtbc.Ui = this;
        vtbc.RemoteControlDrone = other.gameObject;
        vtbc.NameText = other.gameObject.GetComponent<ShipController>().ShipName;
    }
    
    public void ProxyTriggerExit(Collider other)
    {
        foreach (GameObject button in buttons)
        {
            if (button.gameObject == other.gameObject)
            {
                Debug.Log("Destroy Button");
                Destroy(button);
                buttons.Remove(button);
                break;
            }
        }
    }

    public void ProxyTriggerStay(Collider other)
    {
        bool add = true;
        // for every gameobject in buttons, if other.gameobject is in buttons, remove it
        foreach (GameObject button in buttons)
        {
            if (button.GetComponent<VideoTransButtonController>().RemoteControlDrone == other.gameObject)
            {
                add = false;
                break;
            }
        }
        if (add)
        {
            GameObject newButton = Instantiate(VTOButtonPrefab, content.transform);
            buttons.Add(newButton);
            VideoTransButtonController vtbc = newButton.GetComponent<VideoTransButtonController>();
            vtbc.Ui = this;
            vtbc.RemoteControlDrone = other.gameObject;
            vtbc.NameText = other.gameObject.GetComponent<ShipController>().ShipName;
        }

    }

    public void SelectDrone(GameObject drone)
    {
        if(drone != null)
        {
            DroneVolumeController dvc;
            if (drone.TryGetComponent(out dvc))
            {
                if (dvc.CanConnect)
                {
                    connectionText.text = "<Ready to Connect...>";
                    connectionText.color = Color.green;
                    connectButton.SetActive(true);
                }
                else
                {
                    connectionText.text = "<Failed to Connect>";
                    connectionText.color = Color.red;
                    connectButton.SetActive(false);
                }
            }
        }
        targetDrone = drone;
    }

    public void ConnectToTargetDrone()
    {
        Debug.Log("CONNECT");
        if (targetDrone != null)
        {
            connected = true;
            targetDrone.GetComponent<DroneVolumeController>().Connected = true;
            connectionText.text = "<Connected>";
            connectionText.color = Color.green;
            connectButton.SetActive(false);
            
            targetDrone.GetComponent<ShipControlConsole>().UIExternalInteract();
        }
    }
}
