using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.Base
{
    public class ScriptedExplosion : MonoBehaviour
    {
        [SerializeField] private GameObject[] objectsToActivate;
        [SerializeField] private GameObject[] objectsToHide;
        [SerializeField] private AudioSource[] soundsToPlay;
        [SerializeField] private MeshRenderer[] materialsToChange;
        [SerializeField] private Material changeToMaterial;
        [SerializeField] private Collider[] colsToDisable;
        [SerializeField] private MeshRenderer[] renderersToDisable;
        public void ExplodeEffect()
        {
            Debug.Log("ExplodeEffectStart");
            for (int i = 0; i < objectsToActivate.Length; i++)
            {
                objectsToActivate[i].SetActive(true);
            }
            for (int j = 0; j < objectsToHide.Length; j++)
            {
                objectsToHide[j].SetActive(false);
            }
            for (int s = 0; s < soundsToPlay.Length; s++)
            {
                soundsToPlay[s].Play();
            }
            for (int m = 0; m < materialsToChange.Length; m++)
            {
                for(int n = 0; n < materialsToChange[m].materials.Length; n++)
                {
                    materialsToChange[m].enabled = false;
                    materialsToChange[m].materials[n] = changeToMaterial;
                    materialsToChange[m].enabled = true;
                }
            }
            for (int c = 0; c < colsToDisable.Length; c++)
            {
                colsToDisable[c].enabled = false;
            }
            for (int m = 0; m < renderersToDisable.Length; m++)
            {
                renderersToDisable[m].enabled = false;
            }
        }
    }
}