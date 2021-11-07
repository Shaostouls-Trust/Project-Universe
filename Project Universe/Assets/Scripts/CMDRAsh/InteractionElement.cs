using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Serve as a backend for interacting with world space objects. All objects the player can interact with will call from here.
/// </summary>
public class InteractionElement : MonoBehaviour
{
    [SerializeField] private GameObject scriptedObject;
    /// Native Functions
    ///
    public void Interact()
    {
        //send an id to prevent calling every interact func on the object?
        scriptedObject.SendMessage("ExternalInteractFunc", null, SendMessageOptions.DontRequireReceiver);
    }
}
