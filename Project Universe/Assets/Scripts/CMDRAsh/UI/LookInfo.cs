using ProjectUniverse.Items;
using ProjectUniverse.Player.PlayerController;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ProjectUniverse.UI
{
    /// <summary>
    /// Display information when looking at on object with the LookInfoMsg method
    /// </summary>
    public class LookInfo : MonoBehaviour
    {
        public float range = 5f;
        public SupplementalController playerSuppCtrl;
        public TMP_Text infoTxt;
        public GameObject infoBox;
        public GameObject cam;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            Vector3 forward = cam.transform.TransformDirection(0f, 0f, 1f) * range;
            //Debug.DrawRay(cam.transform.position, forward, Color.blue);
            RaycastHit hit;
            if (Physics.Raycast(cam.transform.position, forward, out hit, 5f))
            {
                if (playerSuppCtrl != null)
                {
                    if (playerSuppCtrl.RightHandEquipped != null)
                    {
                        //if (playerSuppCtrl.RightHandEquipped.gameObject.GetComponent<IEquipable>().EquipmentCategory == Base.ItemStack.Category.Tool)
                        //{
                        //Debug.Log(hit.transform.gameObject);
                        infoBox.SetActive(false);
                        //send a message to 'LookInfoMsg'
                        hit.transform.gameObject.SendMessage("LookInfoMsg", this, SendMessageOptions.DontRequireReceiver);
                        //}
                    }
                }
                else //ship and drone inspector
                {
                    infoBox.SetActive(false);
                    //send a message to 'LookInfoMsg'
                    hit.transform.gameObject.SendMessage("LookInfoMsg", this, SendMessageOptions.DontRequireReceiver);
                }
            }
            else
            {
                infoBox.SetActive(false);
            }
        }

        public void LookInfoCallback(string[] data) 
        {
            //display the info
            infoBox.SetActive(true);
            infoTxt.text = data[0];
        }
    }
}