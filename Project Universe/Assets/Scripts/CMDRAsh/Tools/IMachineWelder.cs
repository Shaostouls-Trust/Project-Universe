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
    public class IMachineWelder : IEquipable
    {
        //player will provide their inventory
        //will need reset if dropped and picked up
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
        private GameObject lastHitGO;
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
            //Vector3 forward = Camera.main.transform.TransformDirection(0f, 0f, 1f) * 1f;
            Vector3 forward2 = welderRaycastPoint.transform.TransformDirection(0f, 1f, 0f) * 1f;//0,0,1 is down?
            //try to raycast to a machine
            if (Physics.Raycast(
                    new Vector3(welderRaycastPoint.transform.position.x,
                    welderRaycastPoint.transform.position.y,
                    welderRaycastPoint.transform.position.z),
                    forward2, out RaycastHit hit, 1.0f))//Vector.Back
            {
                object[] prams;
                IConstructible cons;
                if (!hit.transform.CompareTag("Player"))
                {
                    GameObject target = null;
                    // Check if hit is IConstructible. If not, go up to 3 levels higher until a ICons is found
                    // the target GO becomes that ICons
                    if (hit.transform.TryGetComponent<IConstructible>(out IConstructible z))
                    {
                        target = hit.transform.gameObject;
                    }
                    else
                    {
                        if(hit.transform.parent != null)
                        {
                            GameObject one = hit.transform.parent.gameObject;
                            //check one level up
                            if (one.TryGetComponent(out IConstructible a))
                            {
                                target = one;
                            }
                            else
                            {
                                if(one.transform.parent != null)
                                {
                                    GameObject two = one.transform.parent.gameObject;
                                    //check 2 levels up
                                    if (two.TryGetComponent(out IConstructible b))
                                    {
                                        target = two;
                                    }
                                    else
                                    {
                                        if(two.transform.parent != null)
                                        {
                                            GameObject three = two.transform.parent.gameObject;
                                            //check 3 levels up
                                            if (three.TryGetComponent(out IConstructible c))
                                            {
                                                target = three;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        
                    }

                    current = hit;
                    if (Input.GetMouseButton(0))
                    {
                        if (isWelding)
                        {
                            if (lastHitGO != null && target != null)//lastHit.transform
                            {
                                //if the last raycast and this new raycast are different, assume that we've moved on to another machine
                                if (lastHitGO.transform != target.transform)//lastHit.transform != hit.transform
                                {
                                    //stop the welding on the lastHit
                                    //Debug.Log("Stop: new");
                                    prams = new object[] { 0 };
                                    if (lastHitGO.TryGetComponent<IConstructible>(out cons))//lastHit.transform.gameObject
                                    {
                                        cons.MachineMessageReceiver(prams);
                                    }
                                    else
                                    {
                                        Debug.Log("Component Not Found: Defaulting to Message");
                                        lastHitGO.SendMessage("MachineMessageReceiver", prams, SendMessageOptions.DontRequireReceiver);
                                        //lastHit.transform.gameObject
                                    }
                                    isWelding = false;
                                }
                            }
                        }
                        //start welding the new machine
                        if (!isWelding)
                        {
                            if (target != null)
                            {
                                //send to message to the hit gameobject to start a new welding coroutine.
                                prams = new object[] { 3, this, player.GetComponent<IPlayer_Inventory>() };//object[] 
                                if (target.TryGetComponent<IConstructible>(out cons))//hit.transform.gameObject
                                {
                                    cons.MachineMessageReceiver(prams);
                                }
                                else
                                {
                                    Debug.Log("Component Not Found: Defaulting to Message");
                                    //hit.transform.gameObject
                                    target.SendMessage("MachineMessageReceiver", prams, SendMessageOptions.DontRequireReceiver);
                                }
                            }
                        }
                        else//We're already welding, so just update the display
                        {
                            if (target != null)
                            {
                                prams = new object[] { 2, this, player.GetComponent<IPlayer_Inventory>() };
                                if (target.TryGetComponent<IConstructible>(out cons))//hit.transform.gameObject
                                {
                                    cons.MachineMessageReceiver(prams);
                                }
                                else
                                {
                                    Debug.Log("Component Not Found: Defaulting to Message");
                                    //hit.transform.gameObject
                                    target.SendMessage("MachineMessageReceiver", prams, SendMessageOptions.DontRequireReceiver);
                                }
                            }
                        }
                        lastHit = hit;
                        lastHitGO = target;
                    }
                    else
                    {
                        if (isWelding)
                        {
                            if (lastHitGO != null)
                            {
                                //stop building
                                prams = new object[] { 0 };
                                //lastHit.transform.gameObject
                                if (lastHitGO.transform.gameObject.TryGetComponent<IConstructible>(out cons))
                                {
                                    cons.MachineMessageReceiver(prams);
                                }
                                else
                                {
                                    Debug.Log("Component Not Found: Defaulting to Message");
                                    lastHitGO.SendMessage("MachineMessageReceiver", prams, SendMessageOptions.DontRequireReceiver);
                                    //lastHit.transform.gameObject
                                }
                            }
                        }
                        //and peak
                        if (target != null)
                        {
                            prams = new object[] { 1, this, player.GetComponent<IPlayer_Inventory>() };
                            if (target.TryGetComponent<IConstructible>(out cons))//hit.transform.gameObject
                            {
                                cons.MachineMessageReceiver(prams);
                            }
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
                    if (lastHitGO != null)//lastHit.transform 
                    {
                        //Debug.Log("Stop: raycast");
                        object[] prams = { 0 };
                        IConstructible cons;
                        if (lastHitGO.transform.gameObject.TryGetComponent<IConstructible>(out cons))//lastHit.transform
                        {
                            cons.MachineMessageReceiver(prams);
                        }
                        else
                        {
                            Debug.Log("Component Not Found: Defaulting to Message");
                            lastHitGO.SendMessage("MachineMessageReceiver", prams, SendMessageOptions.DontRequireReceiver);
                            //lastHit.transform.gameObject
                        }
                        isWelding = false;
                    }
                }
                else // clear display
                {
                    if(buildComponentPanel.transform.childCount > 1)
                    {
                        ClearUILabels();
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

        private void ClearUILabels()
        {
            for (int i = buildComponentPanel.transform.childCount; i > buildComponentPanel.transform.childCount; i--)
            {
                GameObject.Destroy(buildComponentPanel.transform.GetChild(0).gameObject);
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