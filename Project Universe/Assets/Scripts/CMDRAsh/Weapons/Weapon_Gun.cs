using ProjectUniverse.Items;
using ProjectUniverse.Items.Weapons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.Items.Weapons
{
    /// <summary>
    /// Weapon class. Creates it's own physical game object model with instanced variables on creation.
    /// </summary>
    public class Weapon_Gun : IEquipable
    {
        
        [SerializeField] private IGun_Customizable gun_customBase;
        [SerializeField] private GameObject receiver;
        [SerializeField] private GameObject receiverBone;
        [SerializeField] private GameObject casingBone;
        [SerializeField] private GameObject magazineType;
        [SerializeField] private GameObject magazineBone;
        [SerializeField] private GameObject stock;
        [SerializeField] private GameObject stockBone;
        [SerializeField] private GameObject barrel;
        [SerializeField] private GameObject barrelBone;
        [SerializeField] private GameObject muzzle;
        [SerializeField] private GameObject muzzleBone;
        [SerializeField] private GameObject handguard;
        [SerializeField] private GameObject handguardBone;
        [SerializeField] private GameObject underbarrel;
        [SerializeField] private GameObject underbarrelBone;
        [SerializeField] private GameObject overbarrel;
        [SerializeField] private GameObject overbarrelBone;
        [SerializeField] private GameObject leftbarrel;
        [SerializeField] private GameObject leftbarrelBone;
        [SerializeField] private GameObject rightbarrel;
        [SerializeField] private GameObject rightbarrelBone;

        

        public IGun_Customizable Base
        {
            get { return gun_customBase; }
        }

        /// <summary>
        /// Fire the gun.
        /// </summary>
        override protected void Use()
        {
            //Debug.Log("Fire!");
            gun_customBase.FireMainCall();
        }

        override protected void Stop()
        {
            gun_customBase.StopFiring();
        }

        protected override void Reload()
        {
            gun_customBase.ReloadGun();
        }

        override protected void ToogleMode()
        {
            gun_customBase.ModeSwitch();
        }
    }
}