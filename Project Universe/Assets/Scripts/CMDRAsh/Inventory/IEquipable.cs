using ProjectUniverse.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ProjectUniverse.Base.ItemStack;

namespace ProjectUniverse.Items
{
    public class IEquipable : MonoBehaviour
    {
        //BaseID will be used to create the actual physical gun
        [SerializeField] private string baseID;
        [SerializeField] private bool isArmor;
        [SerializeField] private Category category;
        [SerializeField] private Slot slot;
        [SerializeField] private float defense;
        [SerializeField] private float toughness;
        [SerializeField] private float weight;
        [SerializeField] private float durability;
        [SerializeField] private int quality;
        [SerializeField] private Vector3 offset;
        [SerializeField] private Vector3 rotation;

        public enum Slot
        {
            Undersuit,
            Head,
            Torso,
            RightShoulder,
            LeftShoulder,
            RightUpperArm,
            LeftUpperArm,
            RightForearm,
            LeftForearm,
            RightHand,
            LeftHand,
            LowerBack,
            Hips,
            RightUpperLeg,
            LeftUpperLeg,
            RightLeg,
            LeftLeg,
            RightFoot,
            LeftFoot
        }

        override public string ToString()
        {
            return baseID;
        }

        public string ID
        {
            get { return baseID; }
            set { baseID = value; }
        }

        public Slot EquipmentSlot
        {
            get { return slot; }
            //set { slot = value; }
        }

        public Vector3 ModelOffset
        {
            get { return offset; }
            set { offset = value; }
        }
        public Vector3 ModelRotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        public Category EquipmentCategory
        {
            get { return category; }
            set { category = value; }
        }

        /// <summary>
        /// Use the equipable item. This is either Fire, Throw, or a gadget function.
        /// </summary>
        protected virtual void Use()
        {

        }

        /// <summary>
        /// Stop using the item
        /// </summary>
        protected virtual void Stop()
        {

        }

        /// <summary>
        /// Reload the equipable item.
        /// </summary>
        protected virtual void Reload()
        {

        }

        protected virtual void ToogleMode()
        {

        }

        public void ExternalInteractFunc(int i)
        {
            if(i == 0)
            {
                if (MLAPI.NetworkManager.Singleton.ConnectedClients.TryGetValue(MLAPI.NetworkManager.Singleton.LocalClientId, out var networkedClient))
                {
                    gameObject.GetComponent<InteractionElement>().Parameter = 1;
                    networkedClient.PlayerObject.gameObject.GetComponent<IPlayer_Inventory>().AddToPlayerInventory<Weapons.Weapon_Gun>(this);
                    //Destroy(gameObject);
                    //disable rb and hide
                    gameObject.GetComponent<Rigidbody>().detectCollisions = false;
                    gameObject.SetActive(false);
                }
            }
        }
    }
}