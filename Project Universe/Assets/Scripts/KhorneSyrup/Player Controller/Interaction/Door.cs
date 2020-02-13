using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    private bool closed = true;
    private bool locked = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    void ActDoor() {
        if (closed)
        {
            closed = !closed;
        }
        if (locked)
        {
            Debug.Log("Door is locked!");
            return;
        }
        else {

            closed = !closed;
        }
    }


    void LockDoor() {
        locked = !locked;
    }

    public void Interact()
    {
        ActDoor();
    }

    public void RecieveDamage(float damage)
    {
        throw new System.NotImplementedException();
    }
}
