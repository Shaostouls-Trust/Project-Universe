using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorAnimator : MonoBehaviour
{
    //[SerializeField]
    private BoxCollider trigger;
    //[SerializeField]
    private Animator[] anim;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponentsInChildren<Animator>();//GetComponent<>();

        trigger = GetComponent<BoxCollider>();
    }

    void OnTriggerEnter()
    {
        foreach(Animator ani in anim) { ani.SetTrigger("DoorTrigger"); } 
    }
}
