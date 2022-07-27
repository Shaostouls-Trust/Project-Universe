using ProjectUniverse.PowerSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventTriggerSys : MonoBehaviour
{
    private bool triggered = false;
    [SerializeField] private AudioSource audsrc;
    [SerializeField] private List<GameObject> GOs_ON = new List<GameObject>();
    [SerializeField] private List<ISubMachine> SMach_OFF = new List<ISubMachine>();
    [SerializeField] private IGenerator Generator;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!triggered)
            {
                audsrc.Play();
                triggered = true;
                foreach (GameObject GO in GOs_ON)
                {
                    GO.SetActive(true);
                }
                foreach (ISubMachine SM in SMach_OFF)
                {
                    SM.RunMachine = false;
                }
                Generator.OutputMax = 1425;
                Generator.Leaking = true;
            }
        }
    }
}
