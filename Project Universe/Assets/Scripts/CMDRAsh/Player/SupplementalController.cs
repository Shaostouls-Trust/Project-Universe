using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectUniverse.Player;
using ProjectUniverse.Base;
using ProjectUniverse.Serialization.Handler;
using ProjectUniverse.Serialization;
//using UnityEditor;
using ProjectUniverse.Environment.Volumes;
using System;
using UnityEngine.SceneManagement;
using MLAPI;
using MLAPI.NetworkVariable;
using ProjectUniverse.Items.Weapons;
using MLAPI.Messaging;

namespace ProjectUniverse.Player.PlayerController
{
    [Serializable]
    public class SupplementalController : NetworkBehaviour
    {
        private Guid guid;
        public string crouchKey;
        public string proneKey;
        public bool crouchToggle;
        public bool crouching;
        public bool prone;
        [SerializeField] GameObject playerRoot;
        [SerializeField] GameObject cameraRoot;
        [SerializeField] GameObject fleetBoy;
        [SerializeField] private float crouchHeight;
        [SerializeField] private float proneHeight;
        [SerializeField] private float shrinkerSize;
        [SerializeField] private float defaultHeight;
        //Player stats2
        private NetworkVariableFloat playerNetHealth = new NetworkVariableFloat(100f);
        [SerializeField] private float playerHealth = 100f;//Non-standard. Radiation, suffocation, etc.
        //[SerializeField] private MyLimb head;//0
        //[SerializeField] private MyLimb chest;//1
        [SerializeField] private float headHealth = 45f;
        [SerializeField] private float chestHealth = 225f;
        [SerializeField] private float lArmHealth = 110f;
        [SerializeField] private float rArmHealth = 110f;
        [SerializeField] private float lHandHealth = 25f;
        [SerializeField] private float rHandHealth = 25f;
        [SerializeField] private float lLegHealth = 125f;
        [SerializeField] private float rLegHealth = 125f;
        [SerializeField] private float lFootHealth = 25f;
        [SerializeField] private float rFootHealth = 25f;
        [SerializeField] private float playerHydration = 100f;
        [SerializeField] private float playerHappyStomach = 100f;
        private bool fleetBoyOut = false;

        public float HeadHealth
        {
            get { return headHealth; }
            set { headHealth = value; }
        }
        public float ChestHealth
        {
            get { return chestHealth; }
            set { chestHealth = value; }
        }
        public float LArmHealth
        {
            get { return lArmHealth; }
            set { lArmHealth = value; }
        }
        public float RArmHealth
        {
            get { return rArmHealth; }
            set { rArmHealth = value; }
        }
        public float LHandHealth
        {
            get { return lHandHealth; }
            set { lHandHealth = value; }
        }
        public float RHandHealth
        {
            get { return rHandHealth; }
            set { rHandHealth = value; }
        }
        public float LLegHealth
        {
            get { return lLegHealth; }
            set { lLegHealth = value; }
        }
        public float RLegHealth
        {
            get { return rLegHealth; }
            set { rLegHealth = value; }
        }
        public float LFootHealth
        {
            get { return lFootHealth; }
            set { lFootHealth = value; }
        }
        public float RFootHealth
        {
            get { return rFootHealth; }
            set { rFootHealth = value; }
        }
        public float PlayerHydration
        {
            get { return playerHydration; }
            set { playerHydration = value; }
        }
        public float PlayerHappyStomach
        {
            get { return playerHappyStomach; }
            set { playerHappyStomach = value; }
        }

        private void Awake()
        {
            //if(guid == null)
            //{
            //    guid = Guid.NewGuid();
            //}
        }

        // Update is called once per frame
        void Update()
        {
            playerHealth = playerNetHealth.Value;
            //crouch (hold c)
            if (!crouchToggle)
            {
                if (Input.GetKeyDown(crouchKey))
                {
                    crouching = true;
                    prone = false;
                    playerRoot.transform.localScale = new Vector3(playerRoot.transform.localScale.x,
                        crouchHeight, playerRoot.transform.localScale.z);//.GetComponent<CapsuleCollider>()
                }
                if (Input.GetKeyUp(crouchKey))
                {
                    crouching = false;
                    prone = false;
                    playerRoot.transform.localScale = new Vector3(playerRoot.transform.localScale.x,
                        defaultHeight, playerRoot.transform.localScale.z);
                }
            }
            //crouch (press c)
            else
            {
                if (Input.GetKeyDown(crouchKey))
                {
                    if (crouching)
                    {
                        crouching = false;
                        prone = false;
                        playerRoot.transform.localScale = new Vector3(1.0f, defaultHeight, 1.0f);
                    }
                    else
                    {
                        crouching = true;
                        prone = false;
                        playerRoot.transform.localScale = new Vector3(1.0f, crouchHeight, 1.0f);
                    }
                }
            }
            //prone (press z)
            if (Input.GetKeyDown(proneKey))
            {
                if (prone)
                {
                    crouching = false;
                    prone = false;
                    playerRoot.transform.localScale = new Vector3(1.0f,
                    defaultHeight, 1.0f);
                    playerRoot.GetComponent<CharacterController>().height = defaultHeight;
                    playerRoot.GetComponent<CharacterController>().radius = 0.31f;
                    //playerRoot.GetComponent<CapsuleCollider>().height = defaultHeight;
                    //playerRoot.GetComponent<CapsuleCollider>().radius = 0.31f;
                }
                else
                {
                    crouching = false;
                    prone = true;
                    playerRoot.transform.localScale = new Vector3(1.0f, proneHeight, 1.0f);
                    playerRoot.GetComponent<CharacterController>().height = proneHeight;
                    playerRoot.GetComponent<CharacterController>().radius = shrinkerSize;
                    //playerRoot.GetComponent<CapsuleCollider>().height = proneHeight;
                    //playerRoot.GetComponent<CapsuleCollider>().radius = shrinkerSize;
                }
            }
            if (Input.GetKeyDown(KeyCode.I))
            {
                if (!fleetBoyOut)
                {
                    //disable other controls like LMB, E, Enter, R, etc.
                    playerRoot.GetComponent<PlayerController>().LockAndFreeCursor();
                    fleetBoy.SetActive(true);
                    fleetBoy.GetComponent<FleetBoy2000UIController>().Refresh();
                    fleetBoyOut = true;
                }
                else
                {
                    playerRoot.GetComponent<PlayerController>().UnlockCursor();
                    fleetBoy.SetActive(false);
                    fleetBoyOut = false;
                }
            }
            if (Input.GetKeyDown(KeyCode.Q))
            {
                IPlayer_Inventory inventory = playerRoot.GetComponent<IPlayer_Inventory>();
                foreach (ItemStack stack in inventory.GetPlayerInventory())
                {
                    Debug.Log(stack);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void InflictPlayerDamageServerRpc(float amount)
        {

            // \/ Naw. Let health be negative. Competative dying.
            //if (playerHealth <= 0)
            //{
            //    playerHealth = 0;
            //}
            playerNetHealth.Value -= amount;
        }
        public float PlayerHealth
        {
            get { return playerHealth; }
            set { playerHealth = value; }
        }

        
        public void SavePlayer()
        {
            SceneDataHelper sdh = new SceneDataHelper(SceneManager.GetActiveScene().name, "Adrian Expanse Sector 1", DateTime.Now.ToString(), "CMDR Ash");
            PlayerData data = new PlayerData(guid, sdh, playerRoot.transform,cameraRoot.transform.rotation.eulerAngles,
                GetComponent<IPlayer_Inventory>(), GetComponent<PlayerVolumeController>(),this);
            SerializationHandler.SavePlayer("Player_current",data);
            Debug.Log("Saved");
        }

        public void LoadPlayer()
        {
            PlayerData data = SerializationHandler.Load("Player_current");
            if(data != null)
            {
                //Debug.Log("...Injecting");
                guid = data.GetGUID();
                //Debug.Log(data.Position);
                Vector3 dataRot = data.Rotation.eulerAngles;
                playerRoot.transform.position = data.Position;
                playerRoot.transform.rotation = Quaternion.Euler(0, dataRot.y, 0);
                cameraRoot.transform.rotation = Quaternion.Euler(dataRot.x, 0, dataRot.z);
                playerRoot.transform.localScale = data.Scale;
                object[] prams = 
                    { data.PlayerInventory, data.InventoryWeight };
                GetComponent<IPlayer_Inventory>().OnLoad(prams);
                prams = new object[]{
                data.GetPlayerVolumeController(),
                    };
                GetComponent<PlayerVolumeController>().OnLoad(prams);

                crouchToggle = data.LoadStatsSupplement.crouchToggle;
                crouching = data.LoadStatsSupplement.crouching;
                prone = data.LoadStatsSupplement.prone;
                PlayerHealth = data.LoadStatsSupplement.PlayerHealth;
                HeadHealth = data.LoadStatsSupplement.HeadHealth;
                ChestHealth = data.LoadStatsSupplement.ChestHealth;
                LArmHealth = data.LoadStatsSupplement.LArmHealth;
                RArmHealth = data.LoadStatsSupplement.RArmHealth;
                LHandHealth = data.LoadStatsSupplement.LHandHealth;
                RHandHealth = data.LoadStatsSupplement.RHandHealth;
                LLegHealth = data.LoadStatsSupplement.LLegHealth;
                RLegHealth = data.LoadStatsSupplement.RLegHealth;
                LFootHealth = data.LoadStatsSupplement.LFootHealth;
                RFootHealth = data.LoadStatsSupplement.RFootHealth;
                PlayerHydration = data.LoadStatsSupplement.PlayerHydration;
                PlayerHappyStomach = data.LoadStatsSupplement.PlayerHappyStomach;
            }
            else
            {
                Debug.LogError("Failed to Load");
            }
           
        }

    }
}