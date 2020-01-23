using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class Platform_manager : MonoBehaviour, IInteractable
{
    [SerializeField] private Animator animator;
    [SerializeField] private PlayableDirector animate;

    
    private void OnTriggerEnter(Collider other)
    {
            other.transform.parent = transform;
    }

    private void OnTriggerExit(Collider other)
    {
        other.transform.parent = null;
    }

    public void Interact()
    {
        animate.Play();
        Debug.Log("Elevator Moving!");
    }
    public void RecieveDamage(float damage)
    { }
}
