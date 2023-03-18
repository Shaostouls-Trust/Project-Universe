using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorBTN : MonoBehaviour
{
    public string Event;
    [HideInInspector] protected ElevatorManager EM;

    private void Awake()
    {
        EM = FindObjectOfType<ElevatorManager>();
    }
    public void EventBTN()
    {
        Debug.Log("Message Received!");
        EM.SendMessage(Event);
        Debug.Log("Sending Signal" + Event);
    }
}
