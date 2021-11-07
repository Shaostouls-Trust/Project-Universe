using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// raycast
/// when hits WSButton
/// on keygetinput
/// run func
/// </summary>
namespace ProjectUniverse.Environment.Interactable
{
    public class ButtonInteractor : MonoBehaviour
    {
        [SerializeField] private GameObject defaultpointer;
        [SerializeField] private GameObject interactpointer;
        private bool showingInteractPointer = false;
        private GameObject player;
        void Start()
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
            {
                player = networkedClient.PlayerObject.gameObject;
            }
            else player = null;
        }

        // Update is called once per frame
        void Update()
        {
            //1.5m reach at 0deg. At angles greater than 0 (looking down) it extends
            float extensiondistance = Mathf.Lerp(0f, 0.75f, (transform.localRotation.eulerAngles.x/90f));
            Vector3 forward = transform.TransformDirection(0f, 0f, 1f) * (1.5f+ extensiondistance);
            //to 2.1m so that items on floor can be picked up
            Debug.DrawRay(transform.position, forward, Color.green);
            //if a 1m.5 raycast hits an object collider
            RaycastHit hit;
            if (Physics.Raycast(transform.position, forward, out hit, (1.5f + extensiondistance)))
            {
                if (hit.collider.gameObject.GetComponent<InteractionElement>())//don't need to test this every frame
                {
                    //change cursor to interaction sprite
                    if (!showingInteractPointer)
                    {
                        defaultpointer.SetActive(false);
                        interactpointer.SetActive(true);
                        showingInteractPointer = true;
                    }
                    if (Input.GetKeyUp(KeyCode.E))
                    {
                        //get Interaction Element and call backend function
                        hit.collider.gameObject.GetComponent<InteractionElement>().Interact();
                    }
                }
                else
                {
                    if (showingInteractPointer)
                    {
                        defaultpointer.SetActive(true);
                        interactpointer.SetActive(false);
                        showingInteractPointer = false;
                    }
                }
            }
            else
            {
                if (showingInteractPointer)
                {
                    defaultpointer.SetActive(true);
                    interactpointer.SetActive(false);
                    showingInteractPointer = false;
                }
            }
        }
    }
}