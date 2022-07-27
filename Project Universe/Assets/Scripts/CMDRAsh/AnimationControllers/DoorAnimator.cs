using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectUniverse.PowerSystem;
using ProjectUniverse.Data.Libraries;
using MLAPI.NetworkVariable;
using MLAPI;
using MLAPI.Messaging;
using ProjectUniverse.Production.Machines;

namespace ProjectUniverse.Animation.Controllers
{
    public class DoorAnimator : NetworkBehaviour
    {
        [SerializeField]
        private BoxCollider trigger;
        [SerializeField]
        private Animator[] anim;
        [SerializeField]
        private GameObject emissiveMesh;
        [SerializeField]
        private GameObject[] controlPanelScreens;
        private Mach_AirtightDoor M_door;
        [SerializeField] private AudioSource audsrc;
        //IMPORTANT
        //elem 0 is door L
        //elem 1 is door R
        public float leftBoundOpen;//1.8f
        public float leftBoundClosing;//1.0f
        public float rightBoundOpen;//-1.8f
        public float rightBoundClosing;//-1.0f

        [Space]
        [SerializeField] GameObject modeLeverPart;//z to +90 for manual, z to 0 for auto
        [SerializeField] GameObject modeLightGreen;
        [SerializeField] GameObject modeLightRed;

        private bool isLOpen;//door open
        private bool isLClosed;//door closed
        //private bool isLOpening;//door in process of opening
        //private bool isLClosing;//door in process of closing
        private bool isROpen;
        private bool isRClosed;
        //private bool isROpening;
        //private bool isRClosing;
        private bool hasEmissive;

        private bool locked;//whether or not the door can be opened
        private bool weldedClosed;//whether or not door has been welded shut
        private bool weldedOpen;//Allow welded open?

        private bool isPowered;
        private bool isRunning;

        private bool manualmode;
        //speed at which to run animation.
        //private float animSpeed;

        private Transform doorL_TF;
        private Transform doorR_TF;
        //door emissive
        private Renderer emissRenderer;

        //panel emissive
        private Renderer[] panelRenderer;
        public bool doorAnimIsInX;
        //private ISubMachine doorMachine;
        private bool thisRunMachine = false;
        [Space]
        private NetworkVariableBool netLocked = new NetworkVariableBool(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone });
        private NetworkVariableBool netWeldedClosed = new NetworkVariableBool(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone });
        private NetworkVariableBool netWeldedOpen = new NetworkVariableBool(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone });
        private NetworkVariableBool netIsPowered = new NetworkVariableBool(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone });
        private NetworkVariableBool netIsRunning = new NetworkVariableBool(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone });
        private NetworkVariableBool netThisRunMachine = new NetworkVariableBool(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone });
        //private NetworkVariableFloat netAnimSpeed = new NetworkVariableFloat(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone });

        public bool Open
        {
            get { return (isLOpen || isROpen); }
        }
        
        void Start()
        {
            NetworkListeners();
            M_door = GetComponent<Mach_AirtightDoor>();
            panelRenderer = new Renderer[controlPanelScreens.Length];
            //doorMachine = GetComponent<ISubMachine>();
            //thisRunMachine = doorMachine.RunMachine;
            //Debug.Log(thisRunMachine);
            doorL_TF = anim[0].gameObject.transform;
            doorR_TF = anim[1].gameObject.transform;
            if (emissiveMesh == null)
            {
                hasEmissive = false;
            }
            else
            {
                emissRenderer = emissiveMesh.GetComponent<Renderer>();
                hasEmissive = true;
            }
            //grab all screen renderers
            for (int i = 0; i < controlPanelScreens.Length; i++)
            {
                panelRenderer[i] = controlPanelScreens[i].GetComponent<Renderer>();
            }

            isRunning = true;
            netThisRunMachine.Value = GetComponent<ISubMachine>().GetRunMachine();
        }

        private void NetworkListeners()
        {
            //set starting values
            netLocked.Value = locked;
            netWeldedClosed.Value = weldedClosed;
            netWeldedOpen.Value = weldedOpen;
            netIsPowered.Value = isPowered;
            netIsRunning.Value = isPowered;
            netThisRunMachine.Value = thisRunMachine;
            //set up events
            netLocked.OnValueChanged += delegate { locked = netLocked.Value; };
            netWeldedClosed.OnValueChanged += delegate { weldedClosed = netWeldedClosed.Value; };
            netWeldedOpen.OnValueChanged += delegate { weldedOpen = netWeldedOpen.Value; };
            netIsPowered.OnValueChanged += delegate { isPowered = netIsPowered.Value; };
            netIsRunning.OnValueChanged += delegate { isRunning = netIsRunning.Value; };
            netThisRunMachine.OnValueChanged += delegate { thisRunMachine = netThisRunMachine.Value; };
            //netAnimSpeed.OnValueChanged += delegate { animSpeed = netAnimSpeed.Value; };
        }

        void Update()
        {
            if (thisRunMachine)
            {
                //isRunning = true;
                netIsRunning.Value = true;
                if (!locked)
                {
                    emissRenderer.material = MaterialLibrary.GetDoorStateMaterials(0);
                    for (int i = 0; i < panelRenderer.Length; i++)
                    {
                        panelRenderer[i].material = MaterialLibrary.GetDoorDisplayMaterials(0);
                    }
                }
                else
                {
                    emissRenderer.material = MaterialLibrary.GetDoorStateMaterials(2);
                    for (int i = 0; i < panelRenderer.Length; i++)
                    {
                        panelRenderer[i].material = MaterialLibrary.GetDoorDisplayMaterials(2);
                    }
                }
            }
            else
            {
                //isRunning = false;
                netIsRunning.Value = false;
                emissRenderer.material = MaterialLibrary.GetDoorStateMaterials(2);
                for (int i = 0; i < panelRenderer.Length; i++)
                {
                    panelRenderer[i].material = MaterialLibrary.GetDoorDisplayMaterials(2);
                }
            }
            //if door is powered, run the below checks
            if (isPowered && isRunning)
            {
                //check door positions and animation cycles
                //when the door is at open position, but the animation is not done opening.
                if (doorAnimIsInX)
                {
                    if (doorL_TF.localPosition.x > leftBoundOpen && (isLOpen))//isLOpening || 
                    {
                        anim[0].enabled = false;
                        anim[1].enabled = false;
                        //doorL_TF.localPosition = new Vector3(doorL_TF.localPosition.x, doorL_TF.localPosition.y, leftBoundOpen);
                        doorL_TF.localPosition = new Vector3(leftBoundOpen, doorL_TF.localPosition.y, doorL_TF.localPosition.z);

                    }
                    else
                    {
                        anim[0].enabled = true;
                        anim[1].enabled = true;
                    }
                }
                else
                {
                    if (doorL_TF.localPosition.z > leftBoundOpen && (isLOpen))//isLOpening || 
                    {
                        //stop the left door animation.
                        anim[0].enabled = false;
                        anim[1].enabled = false;
                        //return to final position
                        //doorL_TF.localPosition = new Vector3(leftBoundOpen, doorL_TF.localPosition.y, doorL_TF.localPosition.z);
                        doorL_TF.localPosition = new Vector3(doorL_TF.localPosition.x, doorL_TF.localPosition.y, leftBoundOpen);
                    }
                    else
                    {
                        anim[0].enabled = true;
                        anim[1].enabled = true;
                    }
                }

                //if closed but not done with closing animation
                if (doorAnimIsInX)
                {
                    if (doorL_TF.localPosition.x < leftBoundClosing && (isLClosed))//isLClosing || 
                    {
                        anim[0].enabled = false;
                        anim[1].enabled = false;
                        //doorL_TF.localPosition = new Vector3(doorL_TF.localPosition.x, doorL_TF.localPosition.y, leftBoundClosing);
                        doorL_TF.localPosition = new Vector3(leftBoundClosing, doorL_TF.localPosition.y, doorL_TF.localPosition.z);
                    }
                    else
                    {
                        anim[0].enabled = true;
                        anim[1].enabled = true;
                    }
                }
                else
                {
                    if (doorL_TF.localPosition.z < leftBoundClosing && (isLClosed))// isLClosing ||
                    {
                        //stop
                        anim[0].enabled = false;
                        anim[1].enabled = false;
                        //doorL_TF.localPosition = new Vector3(leftBoundClosing, doorL_TF.localPosition.y, doorL_TF.localPosition.z);
                        doorL_TF.localPosition = new Vector3(doorL_TF.localPosition.z, doorL_TF.localPosition.y, leftBoundClosing);
                    }
                    else
                    {
                        anim[0].enabled = true;
                        anim[1].enabled = true;
                    }
                }

                //R Door open, but not done with animation
                if (doorAnimIsInX)
                {

                    if (doorR_TF.localPosition.x < rightBoundOpen && (isROpen)) //open past -1.8Z isROpening || 
                    {
                        anim[0].enabled = false;
                        anim[1].enabled = false;
                        doorR_TF.localPosition = new Vector3(rightBoundOpen, doorR_TF.localPosition.y, doorR_TF.localPosition.z);
                    }
                    else
                    {
                        anim[0].enabled = true;
                        anim[1].enabled = true;
                    }
                }
                else
                {
                    if (doorR_TF.localPosition.z < rightBoundOpen && ( isROpen)) //open past -1.8Z isROpening ||
                    {
                        anim[0].enabled = false;
                        anim[1].enabled = false;
                        doorR_TF.localPosition = new Vector3(doorR_TF.localPosition.x, doorR_TF.localPosition.y, rightBoundOpen);
                    }
                    else
                    {
                        anim[0].enabled = true;
                        anim[1].enabled = true;
                    }
                }

                //if closed but not done with anim
                if (doorAnimIsInX)
                {
                    if (doorR_TF.localPosition.x > rightBoundClosing && (isRClosed)) //closed past -1.0f isRClosing || 
                    {
                        anim[0].enabled = false;
                        anim[1].enabled = false;
                        doorR_TF.localPosition = new Vector3(rightBoundClosing, doorR_TF.localPosition.y, doorR_TF.localPosition.z);
                    }
                    else
                    {
                        anim[0].enabled = true;
                        anim[1].enabled = true;
                    }
                }
                else
                {
                    if (doorR_TF.localPosition.z > rightBoundClosing && (isRClosed)) //closed past -1.0f isRClosing || 
                    {
                        anim[0].enabled = false;
                        anim[1].enabled = false;
                        doorR_TF.localPosition = new Vector3(doorR_TF.localPosition.x, doorR_TF.localPosition.y, rightBoundClosing);
                    }
                    else
                    {
                        anim[0].enabled = true;
                        anim[1].enabled = true;
                    }
                }
                //netDoorL_Position.Value = doorL_TF.position;
                //netDoorR_Position.Value = doorR_TF.position;
            }
            else
            {
                //if the door is not powered or running, turn emissives and such red.
                emissRenderer.material = MaterialLibrary.GetDoorStateMaterials(2);
                for (int i = 0; i < panelRenderer.Length; i++)
                {
                    panelRenderer[i].material = MaterialLibrary.GetDoorDisplayMaterials(2);
                }
            }

        }

        public bool Locked
        {
            get
            {
                return locked;
            }
            set
            {
                locked = value;
            }
        }

        public void OpenDoor()
        {
            if (isPowered && (!locked && isRunning))
            {
                if (!isLOpen && !isROpen)//should these be network vars? //opening
                {
                    OpenDoorServerRpc();
                }
            }
        }
        
        public void CloseDoor()
        {
            if (isPowered && (!locked && isRunning))
            {
                if (isLOpen && isROpen)
                {
                    CloseDoorServerRpc();
                }
            }
        }

        /// <summary>
        /// open this door
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        private void OpenDoorServerRpc()
        {
            OpenDoorClientRpc();
        }

        /// <summary>
        /// Show other clients that this door is being opened
        /// </summary>
        [ClientRpc]
        private void OpenDoorClientRpc()
        {
            //Debug.Log("ClientDoorOpen");
            //yellow blinking
            doorRIsOpening();
            doorLIsOpening();
            anim[0].Play("DoorLeftOpen");
            anim[1].Play("DoorRightOpen");
            doorLIsOpen();
            doorRIsOpen();
            //green
            audsrc.Play();
        }

        [ServerRpc(RequireOwnership = false)]
        private void CloseDoorServerRpc()
        {
            CloseDoorClientRpc();
        }
        [ClientRpc]
        private void CloseDoorClientRpc()
        {
            //yellow blinking
            doorRIsClosing();
            doorLIsClosing();
            anim[0].Play("DoorLeftClose");
            anim[1].Play("DoorRightClose");
            doorLIsClosed();
            doorRIsClosed();
            //green
            audsrc.Play();
        }

        void OnTriggerEnter(Collider other)
        {
            if (!manualmode)
            {
                if (other.CompareTag("Player"))//replace with a collision layer that only detects actors
                {
                    if (isPowered && (!locked && isRunning))
                    {
                        if (!isLOpen && !isROpen)//should these be network vars? //opening
                        {
                            //Debug.Log(isPowered + "," + isRunning + "," + !locked + "," + !isLOpening + "," + !isROpening);
                            OpenDoorServerRpc();
                        }
                    }
                }
            }
        }

        void OnTriggerStay(Collider other)
        {
            if (!manualmode)
            {
                if (other.CompareTag("Player"))
                {
                    if (isPowered && (!locked && isRunning))
                    {
                        //both doors are open (eval to false) or opening already.
                        if ((!isLOpen && !isROpen))// || (isROpening && isLOpening))
                        {
                            OpenDoorServerRpc();
                        }
                    }
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (!manualmode)
            {
                if (other.CompareTag("Player"))
                {
                    if (isPowered && (!locked && isRunning))
                    {
                        if (!isLClosed && !isRClosed)//!isRClosing etc
                        {
                            CloseDoorServerRpc();
                        }
                    }
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void RunSubmachineServerRpc(int powerLevel)
        {
            runSubMachine(powerLevel);
        }

        public void runSubMachine(int powerLevel)
        {
            //Debug.Log("Running Door");
            switch (powerLevel)
            {
                case 0:
                    setPoweredState(true);
                    setRunningState(true);
                    setAnimSpeedServerRpc(1.0f);
                    //Debug.Log("Case 0");
                    break;
                case 1:
                    setPoweredState(true);
                    setRunningState(true);
                    setAnimSpeedServerRpc(0.75f);
                    //Debug.Log("Case 1");
                    break;
                case 2:
                    setPoweredState(true);
                    setRunningState(true);
                    setAnimSpeedServerRpc(0.5f);
                    //Debug.Log("Case 2");
                    break;
                case 3:
                    setPoweredState(true);
                    setRunningState(true);
                    setAnimSpeedServerRpc(0.15f);
                    //Debug.Log("Case 3");
                    break;
                case 4:
                    setPoweredState(false);
                    setRunningState(false);//was true
                    setAnimSpeedServerRpc(0.0f);
                    //Debug.Log("Case 4");
                    break;
                case 5:
                    setPoweredState(false);//wasn't present
                    setRunningState(false);
                    setAnimSpeedServerRpc(0.0f);
                    //Debug.Log("Case 5");
                    break;
            }
            //temp override for ship construction sake
            //setPoweredState(true);
            //setAnimSpeed(1.0f);
        }

        public bool OpenOrOpening()
        {
            bool state = false;
            if (isLOpen || isROpen)//isLOpening || || isROpening
            {
                state = true;
            }
            return state;
        }

        [ServerRpc(RequireOwnership = false)]
        public void HaltAllAnimationsServerRpc()
        {
            haltAllAnimationsClientRpc();
        }

        [ClientRpc]
        public void haltAllAnimationsClientRpc()
        {
            anim[0].enabled = false;
            anim[1].enabled = false;
        }

        [ServerRpc(RequireOwnership = false)]
        private void lockDoorServerRpc()
        {
            lockDoorClientRpc();
        }

        [ClientRpc]
        private void lockDoorClientRpc()
        {
            if (isRunning && isPowered)
            {
                //close door
                netLocked.Value = true;
                emissRenderer.material = MaterialLibrary.GetDoorStateMaterials(2);
                for (int i = 0; i < panelRenderer.Length; i++)
                {
                    panelRenderer[i].material = MaterialLibrary.GetDoorDisplayMaterials(2);
                }
                if (isLOpen)
                {
                    doorLIsClosing();
                    anim[0].Play("DoorLeftClose");
                    doorLIsClosed();
                }
                if (isROpen)
                {
                    doorRIsClosing();
                    anim[1].Play("DoorRightClose");
                    doorRIsClosed();
                }
                audsrc.Play();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void unlockDoorServerRpc()
        {
            unlockDoorClientRpc();
        }

        [ClientRpc]
        private void unlockDoorClientRpc()
        {
            if (isRunning && isPowered)
            {
                netLocked.Value = false;
                emissRenderer.material = MaterialLibrary.GetDoorStateMaterials(0);
                for (int i = 0; i < panelRenderer.Length; i++)
                {
                    panelRenderer[i].material = MaterialLibrary.GetDoorDisplayMaterials(0);
                }
                //green. Assumes no further issues. Will need logic later on.
                if (hasEmissive)
                {
                    emissRenderer.material = MaterialLibrary.GetDoorStateMaterials(0);
                }
                for (int i = 0; i < panelRenderer.Length; i++)
                {
                    panelRenderer[i].material = MaterialLibrary.GetDoorDisplayMaterials(0);
                }
                audsrc.Play();                
            }
        }

        public void ExternalInteractFunc(int param)
        {
            //lock the door
            if(param == 0)
            {
                if (!locked)
                {
                    lockDoorServerRpc();
                }
                else if (locked)
                {
                    unlockDoorServerRpc();
                }
            }
            //switch modes
            else if(param == 1) 
            {
                manualmode = !manualmode;
                //
                if (manualmode)
                {
                    modeLeverPart.transform.localRotation = Quaternion.Euler(0f,0f,90f);
                    modeLightGreen.GetComponent<Renderer>().material = MaterialLibrary.GetPowerSystemStateMaterials()[3];
                    modeLightRed.GetComponent<Renderer>().material = MaterialLibrary.GetPowerSystemStateMaterials()[1];
                }
                else
                {
                    modeLeverPart.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                    modeLightGreen.GetComponent<Renderer>().material = MaterialLibrary.GetPowerSystemStateMaterials()[0];
                    modeLightRed.GetComponent<Renderer>().material = MaterialLibrary.GetPowerSystemStateMaterials()[4];
                }
                
            }
            //open/close in manual mode
            else if(param == 2)
            {
                if (isPowered && (!locked && isRunning))
                {
                    if (!isLOpen && !isROpen)//should these be network vars?
                    {
                        OpenDoorServerRpc();
                    }
                    else if (!isLClosed && !isRClosed)
                    {
                        CloseDoorServerRpc();
                    }
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void AnimEventOpenServerRpc()
        {
            AnimEventOpenClientRpc();
        }

        //universal
        [ClientRpc]
        public void AnimEventOpenClientRpc()
        {
            //if locked, change to flashing red
            if (locked)
            {
                if (hasEmissive)
                {
                    emissRenderer.material = MaterialLibrary.GetDoorStateMaterials(3);
                }
                for (int i = 0; i < panelRenderer.Length; i++)
                {
                    panelRenderer[i].material = MaterialLibrary.GetDoorDisplayMaterials(2);
                }
            }
            else
            {
                //change to blinking yellow on open
                if (hasEmissive)
                {
                    emissRenderer.material = MaterialLibrary.GetDoorStateMaterials(1);
                }
                for (int i = 0; i < panelRenderer.Length; i++)
                {
                    panelRenderer[i].material = MaterialLibrary.GetDoorDisplayMaterials(1);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void AnimEventDoneServerRpc()
        {
            animEventOpenDoneClientRpc();
        }

        //universal
        [ClientRpc]
        public void animEventOpenDoneClientRpc()
        {
            //if locked, change to solid red
            if (locked)
            {
                if (hasEmissive)
                {
                    emissRenderer.material = MaterialLibrary.GetDoorStateMaterials(2);
                }
                for (int i = 0; i < panelRenderer.Length; i++)
                {
                    panelRenderer[i].material = MaterialLibrary.GetDoorDisplayMaterials(2);
                }
            }
            else
            {
                //else change to green
                if (hasEmissive)
                {
                    emissRenderer.material = MaterialLibrary.GetDoorStateMaterials(0);
                }
                for (int i = 0; i < panelRenderer.Length; i++)
                {
                    panelRenderer[i].material = MaterialLibrary.GetDoorDisplayMaterials(0);
                }
            }
        }

        public void setPoweredState(bool value)
        {
            netIsPowered.Value = value;
            //isPowered = value;
        }
        public void setRunningState(bool value)
        {
            netIsRunning.Value = value;
            //isRunning = value;
        }

        [ServerRpc(RequireOwnership = false)]
        public void setAnimSpeedServerRpc(float speed)
        {
            setAnimSpeedClientRpc(speed);
        }

        [ClientRpc]
        public void setAnimSpeedClientRpc(float speed)
        {
            if(speed == 0f)
            {
                anim[0].enabled = false;
                anim[1].enabled = false;
            }
            else
            {
                anim[0].enabled = true;
                anim[1].enabled = true;
            }
            anim[0].SetFloat("AnimSpeed", speed);
            anim[1].SetFloat("AnimSpeed", speed);
        }

        public void doorLIsOpening()
        {
            //isLOpening = true;
            //isLClosing = false;
            isLOpen = false;
            isLClosed = false;
        }

        public void doorLIsClosing()
        {
            //isLOpening = false;
            //isLClosing = true;
            isLOpen = false;
            isLClosed = false;
        }

        public void doorLIsClosed()
        {
            //isLOpening = false;
            //isLClosing = false;
            isLOpen = false;
            isLClosed = true;
        }

        public void doorLIsOpen()
        {
            //isLOpening = false;
            //isLClosing = false;
            isLOpen = true;
            isLClosed = false;
        }

        public void doorRIsOpening()
        {
            //isROpening = true;
            //isRClosing = false;
            isROpen = false;
            isRClosed = false;
        }

        public void doorRIsClosing()
        {
            //isROpening = false;
            //isRClosing = true;
            isROpen = false;
            isRClosed = false;
        }

        public void doorRIsClosed()
        {
            //isROpening = false;
            //isRClosing = false;
            isROpen = false;
            isRClosed = true;
        }

        public void doorRIsOpen()
        {
            //isROpening = false;
            //isRClosing = false;
            isROpen = true;
            isRClosed = false;
        }
    }
}