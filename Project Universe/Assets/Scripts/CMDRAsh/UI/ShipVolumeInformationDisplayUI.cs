using ProjectUniverse.Environment.Volumes;
using ProjectUniverse.Ship;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ProjectUniverse.UI
{
    public class ShipVolumeInformationDisplayUI : MonoBehaviour
    {
        [SerializeField] private RenderStateManager rsm;
        private bool running;
        [SerializeField] private Transform streaming1;
        [SerializeField] private Transform streaming2;
        [SerializeField] private Transform streaming3;
        [SerializeField] private Transform streaming4;
        [SerializeField] private Transform streaming5;
        private TMP_Text[] stream1TXT = new TMP_Text[56];
        private TMP_Text[] stream2TXT = new TMP_Text[56];
        private TMP_Text[] stream3TXT = new TMP_Text[56];
        private TMP_Text[] stream4TXT = new TMP_Text[56];
        private TMP_Text[] stream5TXT = new TMP_Text[56];

        private void Start()
        {
            for(int a = 0; a < streaming1.childCount; a++)
            {
                stream1TXT[a] = streaming1.GetChild(a).GetComponent<TMP_Text>();
            }
            for (int b = 0; b < streaming2.childCount; b++)
            {
                stream2TXT[b] = streaming2.GetChild(b).GetComponent<TMP_Text>();
            }
            for (int c = 0; c < streaming3.childCount; c++)
            {
                stream3TXT[c] = streaming3.GetChild(c).GetComponent<TMP_Text>();
            }
            for (int d = 0; d < streaming4.childCount; d++)
            {
                stream4TXT[d] = streaming4.GetChild(d).GetComponent<TMP_Text>();
            }
            for (int e = 0; e < streaming5.childCount; e++)
            {
                stream5TXT[e] = streaming5.GetChild(e).GetComponent<TMP_Text>();
            }
        }

        private void Update()
        {
            if (!running)
            {
                StartCoroutine(RoomDataUIUpdate());
            }
        }

        private IEnumerator RoomDataUIUpdate()
        {
            running = true;
            for (int a = 0; a < stream1TXT.Length; a++)
            {
                VolumeAtmosphereController vac = rsm.Rooms[a].GetComponent<VolumeAtmosphereController>();
                stream1TXT[a].text = rsm.Rooms[a].name + ": " + vac.Contamination + " ppmw";
                stream2TXT[a].text = rsm.Rooms[a].name + ": " + vac.Toxicity;
                stream3TXT[a].text = rsm.Rooms[a].name + ": " + vac.Oxygenation + "%";
                stream4TXT[a].text = rsm.Rooms[a].name + ": " + vac.Pressure + " atm";
                stream5TXT[a].text = rsm.Rooms[a].name + ": " + vac.Temperature + "F";
                yield return null;
            }
            running = false;
        }
    }
}