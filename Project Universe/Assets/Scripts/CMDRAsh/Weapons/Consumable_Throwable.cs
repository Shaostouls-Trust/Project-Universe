using ProjectUniverse.Items;
using ProjectUniverse.Player;
using ProjectUniverse.Player.PlayerController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.Items.Consumable
{
    public class Consumable_Throwable : IEquipable
    {
        [SerializeField] private ThrowableType tType;
        [SerializeField] private bool onContactEffect;
        [SerializeField] private float delay = 3f;
        [SerializeField] private float radius = 5f;
        [SerializeField] private float force = 700f;
        private bool burntimer = false;
        private bool deployed = false;
        private SupplementalController player;

        public SupplementalController SC
        {
            get { return player; }
            set { player = value; }
        }

        public enum ThrowableType
        {
            Probe,
            Flare,
            Noisemaker,
            SmokeGrenade,
            EMPGrenade,
            Grenade,
            IncediaryGrenade
        }

        public ThrowableType ThrowType
        {
            get { return tType; }
        }

        // Update is called once per frame
        void Update()
        {
            if (!deployed && burntimer)
            {
                delay -= Time.deltaTime;
                if (delay <= 0f)
                {
                    Explode();
                }
            }
        }

        public void StartTimer()
        {
            burntimer = true;
        }

        public void Throw()
        {
            Debug.Log("Throw");
            GameObject cons = this.gameObject;
            cons.transform.SetParent(null);
            Rigidbody rb = cons.GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.detectCollisions = true;
            rb.AddForce(transform.forward * 25f, ForceMode.VelocityChange);
        }

        public void Explode()
        {
            Debug.Log("Boom");
            deployed = true;
            //Instantiate(explosioneffect, transform.position, transform.rotation);
            Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
            foreach (Collider nearbyObject in colliders)
            {
                Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddExplosionForce(force, transform.position, radius);
                }
            }
            Destroy(gameObject);
        }

        // Start timer and throw grenade along player look axis
        override protected void Use()
        {
            StartTimer();
            Throw();

            //remove one from the grenade stack
            player.ConsumableCallback(ThrowType);
        }

    }
}