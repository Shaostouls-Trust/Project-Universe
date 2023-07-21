using Unity.Netcode;
using ProjectUniverse.Player.PlayerController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectUniverse.Transfer
{
    /// <summary>
    /// Move a player up and down when they enter the trigger based on their camera angle (up or down).
    /// Motion will be done by moving forward or backwards, where forwards is towards the camera and backwards is away from the camera.
    /// </summary>
    public class Ladder : MonoBehaviour
    {
        [SerializeField] private float ladderheight;
        private float heightstore = 0f;
        private SupplementalController kis;
        private float startY;
        GameObject player;
        private bool reverseCalcDir = false;

        public float LadderHeight
        {
            get { return ladderheight; }
        }

        public Vector3 LadderForward()
        {
            return gameObject.transform.forward;
        }

        //Put the player on the ladder if they are NOT at the top. IE if they're going down.
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                if (kis == null)
                {
                    if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
                    {
                        kis = networkedClient.PlayerObject.gameObject.GetComponent<SupplementalController>();
                        player = networkedClient.PlayerObject.gameObject;
                    }
                }
                if (!kis.OnLadder)
                {
                    /*if (kis == null)
                    {
                        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
                        {
                            //kis = networkedClient.PlayerObject.gameObject.GetComponent<SupplementalController>();
                            startY = networkedClient.PlayerObject.transform.position.y;
                            //player = networkedClient.PlayerObject.gameObject;
                        }
                    }
                    else
                    {*/
                        startY = player.transform.position.y;
                        //add height according to the offset between the player and the ladder
                        if (heightstore <= 0f)
                        {
                            //heightstore = Mathf.Abs((gameObject.transform.position.y - player.transform.position.y));
                            //ladderheight += heightstore;
                            //Debug.Log(ladderheight);
                        }
                    //}
                    reverseCalcDir = true;

                    Debug.Log("Get on ladder");
                    kis.OnLadder = true;
                    StartCoroutine(OnLadderTrackPosition());
                }
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
            {
                kis = networkedClient.PlayerObject.gameObject.GetComponent<SupplementalController>();
                player = networkedClient.PlayerObject.gameObject;
                //add height according to the offset between the player and the ladder
                //heightstore = Mathf.Abs((gameObject.transform.position.y - player.transform.position.y));
                //ladderheight += heightstore;
                //Debug.Log(ladderheight);
            }
            else
            {
                Debug.Log("Not Connected");
            }
            //cmf offset
            ladderheight += 0.5f;
            
        }

        /// <summary>
        /// Check how far up the ladder the player is. If we're far enough up call the functions to let the player
        /// move forward onto the landing rather than up into the air.
        /// </summary>
        public IEnumerator OnLadderTrackPosition()
        {
            while (kis.OnLadder)
            {
                if (!reverseCalcDir)
                {
                    //Debug.Log(player.transform.position.y + startY);
                    if (Mathf.Abs(player.transform.position.y + startY) > LadderHeight)
                    {
                        kis.EndOfLadder(LadderForward());
                        //heightstore = 0f;
                    }
                }
                else
                {
                    //Debug.Log(startY - player.transform.position.y);
                    if (Mathf.Abs(startY - player.transform.position.y) >= (LadderHeight - 1.0f))
                    {
                        kis.EndOfLadder(LadderForward());
                        //heightstore = 0f;
                    }
                }
                
                yield return null;
            }

        }

        //get on or off the bottom of the ladder
        public void ExternalInteractFunc()
        {
            
            if (kis == null)
            {
                if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
                {
                    kis = networkedClient.PlayerObject.gameObject.GetComponent<SupplementalController>();
                    startY = networkedClient.PlayerObject.transform.position.y;
                    player = networkedClient.PlayerObject.gameObject;
                    //add height according to the offset between the player and the ladder
                    //heightstore = Mathf.Abs((gameObject.transform.position.y - player.transform.position.y));
                    //ladderheight += heightstore;
                    //Debug.Log(ladderheight);
                }
            }
            else
            {
                startY = player.transform.position.y;
                //add height according to the offset between the player and the ladder
                if (heightstore <= 0f)
                {
                    //heightstore = Mathf.Abs((gameObject.transform.position.y - player.transform.position.y));
                    //ladderheight += heightstore;
                    //Debug.Log(ladderheight);
                }
                
            }

            reverseCalcDir = false;
            kis.OnLadder = !kis.OnLadder;

            if (kis.OnLadder)
            {
                StartCoroutine(OnLadderTrackPosition());
            }
        }
    }
}