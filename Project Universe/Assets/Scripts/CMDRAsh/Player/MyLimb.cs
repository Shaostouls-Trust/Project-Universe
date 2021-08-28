using MLAPI;
using MLAPI.NetworkVariable;
using ProjectUniverse.Items.Weapons;
using ProjectUniverse.Player.PlayerController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.Player
{
    public class MyLimb : NetworkBehaviour
    {
        [SerializeField] private int limbID;
        [SerializeField] private float limbHealth;
        [SerializeField] private float limbHealthMax;
        private NetworkVariableFloat netLimbHealth = new NetworkVariableFloat(100f);
        private GameObject player;
        
        public float NetLimbHealth 
        {
            get { return netLimbHealth.Value; }
            set { netLimbHealth.Value = value; }
        }

        public void TakeDamageFromBullet(IBullet bullet)
        {
            Debug.Log("Ouch");
            NetLimbHealth -= bullet.GetDamageAmount();
            switch (limbID)
            {
                case 0:
                    player.GetComponent<SupplementalController>().HeadHealth = NetLimbHealth;
                    break;
                case 1:
                    player.GetComponent<SupplementalController>().ChestHealth = NetLimbHealth;
                    break;
                case 2:
                    player.GetComponent<SupplementalController>().LArmHealth = NetLimbHealth;
                    break;
                case 3:
                    player.GetComponent<SupplementalController>().RArmHealth = NetLimbHealth;
                    break;
                case 4:
                    player.GetComponent<SupplementalController>().LHandHealth = NetLimbHealth;
                    break;
                case 5:
                    player.GetComponent<SupplementalController>().RHandHealth = NetLimbHealth;
                    break;
                case 6:
                    player.GetComponent<SupplementalController>().LLegHealth = NetLimbHealth;
                    break;
                case 7:
                    player.GetComponent<SupplementalController>().RLegHealth = NetLimbHealth;
                    break;
                case 8:
                    player.GetComponent<SupplementalController>().LFootHealth = NetLimbHealth;
                    break;
                case 9:
                    player.GetComponent<SupplementalController>().RFootHealth = NetLimbHealth;
                    break;
            }
        }

        private void Start()
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
            {
                player = networkedClient.PlayerObject.gameObject;
            }

        }

        private void Update()
        {
            limbHealth = netLimbHealth.Value;
        }
    }
}
