using MLAPI;
using ProjectUniverse.Base;
using ProjectUniverse.Environment.World;
using ProjectUniverse.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.Items.Tools
{
    public class MiningDrill : MonoBehaviour
    {
        [SerializeField] private int mineAmount;
        private GameObject player;
        public GameObject drillRaycastPoint;
        private bool isMining = false;
        private RaycastHit lastHit;
        private RaycastHit current;

        // Start is called before the first frame update
        void Start()
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
            {
                player = networkedClient.PlayerObject.gameObject;
            }
        }

        // Update is called once per frame
        void Update()
        {
            //Vector3 forward = Camera.main.transform.TransformDirection(0f, 0f, 1f) * 1f; //no main camera
            Vector3 forward2 = drillRaycastPoint.transform.TransformDirection(0f, 1f, 0f) * 1f;//0,0,1 is down
            if (Physics.Raycast(
                    new Vector3(drillRaycastPoint.transform.position.x,
                    drillRaycastPoint.transform.position.y,
                    drillRaycastPoint.transform.position.z),
                    forward2, out RaycastHit hit, 1.0f))
            {
                object[] prams;
                BlockOreSingle block;
                if (!hit.transform.CompareTag("Player"))
                {
                    current = hit;
                    if (Input.GetMouseButton(0))
                    {
                        //Debug.Log("Hitting "+hit.transform.gameObject);
                        ItemStack stack = null;
                        if (hit.transform.gameObject.TryGetComponent<BlockOreSingle>(out block))
                        {
                            stack = block.MiningCallback(mineAmount);
                            if (stack != null)
                            {
                                //Debug.Log("Adding ore");
                                //add the ore to the player inventory
                                player.GetComponent<IPlayer_Inventory>().AddStackToPlayerInventory(stack);
                            }
                        }
                    }
                }
            }
            /*
            else
            {
                if (isMining)
                {
                    //player stops welding
                    if (lastHit.transform != null)
                    {
                        object[] prams = { 0 };
                        BlockOreSingle block;
                        if (lastHit.transform.gameObject.TryGetComponent<BlockOreSingle>(out block))
                        {
                            block.MiningCallback();
                        }
                        else
                        {
                            //Debug.Log("Component Not Found: Defaulting to Message");
                            //lastHit.transform.gameObject.SendMessage("MiningCallback", null, SendMessageOptions.DontRequireReceiver);
                        }
                        isMining = false;
                    }
                }
            }*/
        }
    }
}