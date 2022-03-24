using ProjectUniverse.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace ProjectUniverse.UI
{
    public class GunAmmoUI : MonoBehaviour
    {
        private IEquipable target; //The item to be monitored (Only a gun will pass values.)
        [SerializeField] private TMP_Text gunId;
        [SerializeField] private TMP_Text ammoCurrent;
        //[SerializeField] private TMP_Text ammoTotal;
        [SerializeField] private TMP_Text ammoStored;
        [SerializeField] private TMP_Text firemode;

        public void UpdateGunID(string name)
        {
            gunId.text = name;
        }
        public void UpdateAmmoCurrent(int newAmount)
        {
            ammoCurrent.text = "" + newAmount;
        }
        //public void UpdateAmmoTotal(int total)
        //{
        //    ammoTotal.text = "" + total;
        //}
        public void UpdateAmmoStored(int stored)
        {
            ammoStored.text = "" + stored;
        }
        public void UpdateMode(string mode)
        {
            firemode.text = mode;
        }
    }
}