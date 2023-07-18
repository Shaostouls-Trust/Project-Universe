using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.Items.Weapons
{
    public class IBullet : MonoBehaviour
    {
        private float damage;
        private float penData; //NYI
        [SerializeField] private AudioSource audSrc;
        
        private void OnCollisionEnter(Collision collision)
        {
            //for now, only deal damage
            if (GetComponent<Rigidbody>().velocity.magnitude > 1f)
            {
                //audSrc.Play(0);
            }
            //Bullets not making it through to the limbs b/c of the char controller (even with collision layers)
            collision.gameObject.SendMessage("TakeDamageFromBullet", this, SendMessageOptions.DontRequireReceiver);
            collision.gameObject.SendMessageUpwards("TakeDamageFromBullet", this, SendMessageOptions.DontRequireReceiver);
            //Debug.Log(collision.gameObject.name);
            ///Eventually play an impact sound and do all the ballistics stuff
            ///here
        }

        public void SetDamageAmount(float num)
        {
            damage = num;
        }
        public float GetDamageAmount()
        {
            return damage;
        }
        public float GetPenData()
        {
            return penData;
        }
    }
}