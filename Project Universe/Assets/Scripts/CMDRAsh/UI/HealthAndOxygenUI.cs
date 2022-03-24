using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using ProjectUniverse.Player.PlayerController;
using ProjectUniverse.Environment.Volumes;

namespace ProjectUniverse.UI
{
    /// <summary>
    /// Display health and oxygen information for the referenced object
    /// </summary>
    public class HealthAndOxygenUI : MonoBehaviour
    {

        [SerializeField] private GameObject controllerObject;
        public Image healthParent;
        public Image oxygenParent;
        public Image hydrParent;
        public Image foodParent;
        private SupplementalController player;
        private PlayerVolumeController pvc;
        private Color32 GREEN = new Color32(0, 200, 57, 150);// healthy
        private Color32 YELLOW = new Color32(255, 200, 75, 150);// injured
        private Color32 RED = new Color32(237, 0, 0, 150);// incapacitated
        private Color32 DARKRED = new Color32(55, 0, 0, 150);// lost/amputated
        /// <summary>
        /// 0 - Head
        /// 1 - Chest
        /// 2 - Left Arm
        /// 3 - Right Arm
        /// 4 - Left Hand
        /// 5 - Right Hand
        /// 6 - Left Leg
        /// 7 - Right Leg
        /// 8 - Left Foot
        /// 9 - Right Foot
        /// </summary>
        public GameObject[] PartHealthDis;

        private void Start()
        {
            player = controllerObject.GetComponent<SupplementalController>();
            pvc = controllerObject.GetComponent<PlayerVolumeController>();
        }

        public void OnGUI()
        {
            Image healthbar = healthParent.transform.GetChild(0).GetComponent<Image>();
            float barmaxA = healthParent.rectTransform.rect.width;
            float y = (player.PlayerHealth / player.PlayerHealthMax)*barmaxA;
            healthbar.rectTransform.transform.localScale = new Vector3(y, 1, 1);

            Image oxybar = oxygenParent.transform.GetChild(0).GetComponent<Image>();
            float barmaxB = oxygenParent.rectTransform.rect.width;
            float y2 = (pvc.PlayerOxygen / 100f) * barmaxB;
            oxybar.rectTransform.transform.localScale = new Vector3(y2, 1, 1);

            Image hydbar = hydrParent.transform.GetChild(0).GetComponent<Image>();
            float barmaxC = hydrParent.rectTransform.rect.width;
            float y3 = (player.PlayerHydration / 100f) * barmaxC;
            hydbar.rectTransform.transform.localScale = new Vector3(y3, 1, 1);

            Image fdbar = foodParent.transform.GetChild(0).GetComponent<Image>();
            float barmaxD = foodParent.rectTransform.rect.width;
            float y4 = (player.PlayerHydration / 100f) * barmaxD;
            fdbar.rectTransform.transform.localScale = new Vector3(y4, 1, 1);

            CheckHealth(player.HeadHealth, 45f, PartHealthDis[0]);
            CheckHealth(player.ChestHealth, 225f, PartHealthDis[1]);
            CheckHealth(player.LArmHealth, 110f, PartHealthDis[2]);
            CheckHealth(player.RArmHealth, 110f, PartHealthDis[3]);
            CheckHealth(player.LHandHealth, 25f, PartHealthDis[4]);
            CheckHealth(player.RHandHealth, 25f, PartHealthDis[5]);
            CheckHealth(player.LLegHealth, 125f, PartHealthDis[6]);
            CheckHealth(player.RLegHealth, 125f, PartHealthDis[7]);
            CheckHealth(player.LFootHealth, 25f, PartHealthDis[8]);
            CheckHealth(player.RFootHealth, 25f, PartHealthDis[9]);

        }

        public void CheckHealth(float stat, float statmax, GameObject Img)
        {
            float dec = (stat/statmax)*100;
            if(dec > 90)
            {
                Img.GetComponent<Image>().color = GREEN;
            }
            else if (dec >= 25)
            {
                Img.GetComponent<Image>().color = YELLOW;
            }
            else if (dec >= 1)
            {
                Img.GetComponent<Image>().color = RED;
            }
            else
            {
                Img.GetComponent<Image>().color = DARKRED;
            }
        }

    }
}