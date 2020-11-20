using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    public GameObject ObjectToTeleportTo;
    public GameObject PlayersPos;
    private float TimerTime = 3f;
    public GameObject PanelAnimation;
    public Animator panelaniminspector;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {           
            PanelAnimation.SetActive(true);
           PlayersPos.transform.position = ObjectToTeleportTo.transform.position;
            if (panelaniminspector.isInitialized == false)
            
            {                
                PanelAnimation.SetActive(false);           
            }
        }
    }

}
