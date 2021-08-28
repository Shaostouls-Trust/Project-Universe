using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using ProjectUniverse.Player;
using ProjectUniverse.Base;
using ProjectUniverse.Data.Libraries.Definitions;
using ProjectUniverse.Production.Resources;
using MLAPI;

namespace ProjectUniverse.Items.Tools
{
    public class IMachineWelder : MonoBehaviour
    {
        //player will provide their inventory
        private GameObject player;
        [SerializeField] private GameObject buildComponentPanel;
        [SerializeField] private TMP_Text machineName;
        //machine's recipe - UI use only
        private (IComponentDefinition, int)[] RequiredComponents;//IComponentDefinition
                                                                 //what it currently in the machine - UI use only
        private ItemStack[] RealComponents;
        //private float timer = 0.0f;
        //all machines will have an IConstructible Base, a power system script, and their specific
        //control script. We'll use SendMessage, and just ensure that these classes have the backend.
        //public GameObject machine;
        public GameObject welderUICompPrefab;
        public GameObject welderRaycastPoint;
        private bool isWelding = false;
        private RaycastHit lastHit;
        private RaycastHit current;
        private Color32 LIGHTGOLD = new Color32(238, 232, 70, 255);

        private void Start()
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
            {
                player = networkedClient.PlayerObject.gameObject;
            }
        }

        // Update is called once per frame
        void Update()
        {
            //try to raycast to a machine
            if (Physics.Raycast(
                    new Vector3(welderRaycastPoint.transform.position.x,
                    welderRaycastPoint.transform.position.y,
                    welderRaycastPoint.transform.position.z),
                    Vector3.back, out RaycastHit hit, 1.0f))
            {
                object[] prams;
                IConstructible cons;
                if (!hit.transform.CompareTag("Player"))
                {
                    current = hit;
                    if (Input.GetMouseButton(0))
                    {
                        if (isWelding)
                        {
                            if (lastHit.transform != null)
                            {
                                //if the last raycast and this new raycast are different, assume that we've moved on to another machine
                                if (lastHit.transform != hit.transform)
                                {
                                    //stop the welding on the lastHit
                                    Debug.Log("Stop: new");
                                    prams = new object[] { 0 };
                                    if (lastHit.transform.gameObject.TryGetComponent<IConstructible>(out cons))
                                    {
                                        cons.MachineMessageReceiver(prams);
                                    }
                                    else
                                    {
                                        Debug.Log("Component Not Found: Defaulting to Message");
                                        lastHit.transform.gameObject.SendMessage("MachineMessageReceiver", prams, SendMessageOptions.DontRequireReceiver);
                                    }
                                    isWelding = false;
                                }
                            }
                        }
                        //start welding the new machine
                        if (!isWelding)
                        {
                            //send to message to the hit gameobject to start a new welding coroutine.
                            prams = new object[] { 3, this, player.GetComponent<IPlayer_Inventory>() };//object[] 
                            if (hit.transform.gameObject.TryGetComponent<IConstructible>(out cons))
                            {
                                cons.MachineMessageReceiver(prams);
                            }
                            else
                            {
                                Debug.Log("Component Not Found: Defaulting to Message");
                                hit.transform.gameObject.SendMessage("MachineMessageReceiver", prams, SendMessageOptions.DontRequireReceiver);
                            }
                        }
                        else//We're already welding, so just update the display
                        {
                            prams = new object[] { 2, this, player.GetComponent<IPlayer_Inventory>() };
                            if (hit.transform.gameObject.TryGetComponent<IConstructible>(out cons))
                            {
                                cons.MachineMessageReceiver(prams);
                            }
                            else
                            {
                                Debug.Log("Component Not Found: Defaulting to Message");
                                hit.transform.gameObject.SendMessage("MachineMessageReceiver", prams, SendMessageOptions.DontRequireReceiver);
                            }
                        }
                        lastHit = hit;
                    }
                    else
                    {
                        if (isWelding)
                        {
                            //stop building
                            prams = new object[] { 0 };

                            if (lastHit.transform.gameObject.TryGetComponent<IConstructible>(out cons))
                            {
                                cons.MachineMessageReceiver(prams);
                            }
                            else
                            {
                                Debug.Log("Component Not Found: Defaulting to Message");
                                lastHit.transform.gameObject.SendMessage("MachineMessageReceiver", prams, SendMessageOptions.DontRequireReceiver);
                            }
                        }
                        //and peak
                        prams = new object[] { 1, this, player.GetComponent<IPlayer_Inventory>() };
                        if (hit.transform.gameObject.TryGetComponent<IConstructible>(out cons))
                        {
                            cons.MachineMessageReceiver(prams);
                        }
                        isWelding = false;
                    }
                }
            }
            else
            {
                if (isWelding)
                {
                    //player stops welding
                    if (lastHit.transform != null)
                    {
                        Debug.Log("Stop: raycast");
                        object[] prams = { 0 };
                        IConstructible cons;
                        if (lastHit.transform.gameObject.TryGetComponent<IConstructible>(out cons))
                        {
                            cons.MachineMessageReceiver(prams);
                        }
                        else
                        {
                            Debug.Log("Component Not Found: Defaulting to Message");
                            lastHit.transform.gameObject.SendMessage("MachineMessageReceiver", prams, SendMessageOptions.DontRequireReceiver);
                        }
                        isWelding = false;
                    }
                }
            }
        }

        public void IConstructibleCallback(params object[] prams)
        {
            if (prams.Length == 1)//update current components display
            {
                RealComponents = prams.GetValue(0) as ItemStack[];
                isWelding = true;
                UpdateUIDisplay(false);
            }
            else if (prams.Length == 2)//get needed and current components
            {
                RequiredComponents = prams.GetValue(0) as (IComponentDefinition, int)[];
                RealComponents = prams.GetValue(1) as ItemStack[];
                isWelding = true;
                TrimUILabels();
                UpdateUIDisplay(false);
            }
            else if (prams.Length == 3)//just peaking
            {
                RequiredComponents = prams.GetValue(0) as (IComponentDefinition, int)[];
                RealComponents = prams.GetValue(1) as ItemStack[];
                isWelding = false;

                if (current.transform != lastHit.transform)
                {
                    TrimUILabels();
                }
                UpdateUIDisplay((bool)prams.GetValue(2));
            }
        }

        private void TrimUILabels()
        {
            if (buildComponentPanel.transform.childCount < RequiredComponents.Length)
            {
                for (int i = 0; i < (RequiredComponents.Length - buildComponentPanel.transform.childCount); i++)
                {
                    GameObject compInst = Instantiate(welderUICompPrefab);
                    compInst.transform.SetParent(buildComponentPanel.transform);
                    compInst.transform.localScale = new Vector3(1, 1, 1);
                    compInst.transform.localPosition = new Vector3(0, 0, 0);
                    compInst.transform.localRotation = Quaternion.Euler(0, 0, 0);
                }
            }
            else if (buildComponentPanel.transform.childCount > RequiredComponents.Length)
            {
                for (int i = buildComponentPanel.transform.childCount;
                    i > (buildComponentPanel.transform.childCount - RequiredComponents.Length); i--)
                {
                    GameObject.Destroy(buildComponentPanel.transform.GetChild(0).gameObject);
                }
            }
        }

        private void UpdateUIDisplay(bool highlightPotentials)
        {
            //update the build components' text fields
            for (int i = 0; i < buildComponentPanel.transform.childCount; i++)
            {
                Transform compPanel = buildComponentPanel.transform.GetChild(i);
                compPanel.gameObject.SetActive(true);

                string componentName = RequiredComponents[i].Item1.GetComponentType().Split('_')[1];
                int compAmountNeeded = RequiredComponents[i].Item2;
                int compHas = 0;
                //Because of the sort, these comps are no longer in line, so we need to find the matching component
                for (int j = 0; j < RealComponents.Length; j++)
                { 
                    if (RealComponents[j].GetStackType() == RequiredComponents[i].Item1.GetComponentType())
                    {
                        compHas = RealComponents[j].GetRealLength();
                    }
                }
                
                compPanel.GetChild(0).GetComponent<TMP_Text>().text = componentName + ":";
                compPanel.GetChild(1).GetComponent<TMP_Text>().text = compHas + "/" + compAmountNeeded;
                if (highlightPotentials)
                {
                    float InvAmount = 0f;
                    int lastindex = 0;
                    //find any matching component in the player inventory
                    Consumable_Component[] components = player.GetComponent<IPlayer_Inventory>().SearchInventoryForComponent(
                        RequiredComponents[i].Item1, out lastindex);
                    foreach (Consumable_Component component in components)
                    {
                        InvAmount += component.GetQuantity();
                    }
                    if (compHas >= compAmountNeeded)
                    {
                        compPanel.GetChild(1).GetComponent<TMP_Text>().color = Color.green;
                    }
                    else if (InvAmount >= compAmountNeeded)
                    {
                        compPanel.GetChild(1).GetComponent<TMP_Text>().color = LIGHTGOLD;
                    }
                    else
                    {
                        compPanel.GetChild(1).GetComponent<TMP_Text>().color = Color.red;
                    }
                }
                else
                {
                    if (compHas >= compAmountNeeded)
                    {
                        compPanel.GetChild(1).GetComponent<TMP_Text>().color = Color.green;
                    }
                    else
                    {
                        compPanel.GetChild(1).GetComponent<TMP_Text>().color = Color.red;
                    }
                }
            }
        }
    }
}