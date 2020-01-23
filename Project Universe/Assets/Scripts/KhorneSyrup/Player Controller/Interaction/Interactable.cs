using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class Interactable : MonoBehaviour, IInteractable
{
    
    //[SerializeField] private GameObject Target;
    [SerializeField] private PlayableDirector Animate;
    [SerializeField] private GameObject Target;

    //Send Interaction function to script when triggered.
    public void Interact()
    {
        Animate.Play();
        Target.GetComponent<IInteractable>().Interact();
        Debug.Log("WOWWWWWWWWWW!!!");
        return;
    }
    public void RecieveDamage(float dmage)
    {
        return;
    }
    void OnDrawGizmos()
    {
        if (Target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, Target.transform.position);
        }
        else { }
    }
}
