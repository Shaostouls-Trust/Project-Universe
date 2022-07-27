using MLAPI;
using ProjectUniverse.Base;
using ProjectUniverse.Environment.World;
using ProjectUniverse.Items.Containers;
using ProjectUniverse.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.Items.Tools
{
    public class MiningDrill : IEquipable
    {
        [SerializeField] private int mineAmount;
        [SerializeField] private LineRenderer line;
        [SerializeField] private bool mounted;
        [SerializeField] private Inventory mountedInventory;
        [SerializeField] private CargoContainer container;
        [SerializeField] private float distance = 1f;
        public Vector3 rayDir;
        private GameObject player;
        public GameObject drillRaycastPoint;
        private bool isMining = false;
        private RaycastHit lastHit;
        private RaycastHit current;
        private int count = 10;

        public int MineAmount
        {
            get { return mineAmount; }
            set { mineAmount = value; }
        }

        // Start is called before the first frame update
        void Start()
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
            {
                player = networkedClient.PlayerObject.gameObject;
            }
        }

        public GameObject Player
        {
            get { return player; }
            set { player = value; }
        }

        private void Update()
        {
            //if (count <= 0)
            //{
            //    count = 10;
            //}
            //count--;
            if (mountedInventory != null)
            {
                if (mountedInventory.IsFull())
                {
                    isMining = false;
                }
            }
            else if (container != null)
            {
                if (container.IsFull)
                {
                    isMining = false;
                }
            }
            if (isMining)
            {
                Vector3 forward2 = drillRaycastPoint.transform.TransformDirection(rayDir) * 1f;//0,1,0
                if (Physics.Raycast(
                        new Vector3(drillRaycastPoint.transform.position.x,
                        drillRaycastPoint.transform.position.y,
                        drillRaycastPoint.transform.position.z),
                        forward2, out RaycastHit hit, distance))
                {
                    object[] prams;
                    BlockOreSingle block;
                    if (!hit.transform.CompareTag("Player"))
                    {
                        current = hit;
                        //line render to te it point
                        if (!line.enabled)
                        {
                            line.enabled = true;
                            //line.SetPosition(1, (hit.point));
                        }
                        //if (count <= 0) hit.point.x
                        //{
                        line.SetPosition(1, new Vector3(hit.point.x,
                            drillRaycastPoint.transform.localPosition.y, drillRaycastPoint.transform.localPosition.z));
                        //}
                        //Debug.Log("Hitting "+hit.transform.gameObject);
                        ItemStack stack = null;
                        if (hit.transform.gameObject.TryGetComponent<BlockOreSingle>(out block))
                        {
                            stack = block.MiningCallback(mineAmount);
                            if (stack != null)
                            {
                                //Debug.Log("Adding ore");
                                //add the ore to the player inventory
                                if (!mounted)
                                {
                                    player.GetComponent<IPlayer_Inventory>().AddStackToPlayerInventory(stack);
                                }
                                else
                                {
                                    if(mountedInventory != null)
                                    {
                                        mountedInventory.Add(stack);
                                    }
                                    else
                                    {
                                        container.AddToInventory(stack);
                                    }
                                    //Debug.Log("Stack: "+stack);
                                    
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (line.enabled)
                {
                    line.enabled = false;
                }
            }
        }

        override protected void Use()
        {
            isMining = true;
        }

        override protected void Stop()
        {
            isMining = false;
        }

    }
}