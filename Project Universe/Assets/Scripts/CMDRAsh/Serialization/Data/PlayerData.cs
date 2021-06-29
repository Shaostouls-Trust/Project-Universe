using System.Collections;
using System;
using UnityEditor;
using UnityEngine;
using ProjectUniverse.Base;
using System.Collections.Generic;
using ProjectUniverse.Environment.Volumes;
using ProjectUniverse.Player;
using ProjectUniverse.Player.PlayerController;

namespace ProjectUniverse.Serialization
{
    [Serializable]
    public sealed class PlayerData
    {
        //player Transform
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;
        //player physical

        //private Matrix4x4 TransformLTW;
        //ID
        private readonly GUID _PlayerID;
        //Inventory
        public float InventoryWeight;
        /// <summary>
        /// Rigidbodies, BoxColliders, and other Components require a GameObject. IE all must be serialized in a GO Surrogate
        /// </summary>
        //public Rigidbody _Rigidbody;
        public List<ItemStack> PlayerInventory;
        private PlayerVolumeController PVC;
        private SupplementalController StatsSupplement;
        //State of all Equipment/Items (saved seperately?)
        //All Items in hands (later this will be determined by/part of the player inventory)
        //etc

        public PlayerData(GUID guid, Transform transform, Vector3 lookDirection, IPlayer_Inventory inventory, PlayerVolumeController pvc,
            SupplementalController statsController)
        {
            _PlayerID = guid;
            Position = transform.position;
            Rotation = Quaternion.Euler(new Vector3(lookDirection.x, transform.rotation.y, lookDirection.z));
            Scale = transform.localScale;
            //TransformLTW = transform.localToWorldMatrix;
            InventoryWeight = inventory.GetInventoryWeight();
            //_Rigidbody = inventory.GetRigidbody();
            PlayerInventory = inventory.GetPlayerInventory();
            PVC = pvc;
            StatsSupplement = statsController;
        }

        public GUID GetGUID()
        {
            //Debug.Log(_PlayerID);
            return _PlayerID;
        }
        public PlayerVolumeController GetPlayerVolumeController()
        {
            return PVC;
        }
        public SupplementalController LoadStatsSupplement
        {
            get { return StatsSupplement; }
        }
    }
}