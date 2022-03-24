using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Serve as a backend for interacting with world space objects. All objects the player can interact with will call from here.
/// </summary>
public class InteractionElement : MonoBehaviour
{
    [SerializeField] private GameObject scriptedObject;
    [SerializeField] private int parameter = -1;
    /// Native Functions
    ///

    public int Parameter
    {
        get { return parameter; }
        set { parameter = value; }
    }

    public void Interact()
    {
        //send an id to prevent calling every interact func on the object?
        if(parameter == -1)
        {
            scriptedObject.SendMessage("ExternalInteractFunc", null, SendMessageOptions.DontRequireReceiver);
        }
        else
        {
            scriptedObject.SendMessage("ExternalInteractFunc", parameter, SendMessageOptions.DontRequireReceiver);
        }
        
    }
}
