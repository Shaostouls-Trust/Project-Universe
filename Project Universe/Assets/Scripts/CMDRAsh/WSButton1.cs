using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WSButton1 : MonoBehaviour
{

    private MeshRenderer renderer;
    [SerializeField]
    private GameObject door;

    // Start is called before the first frame update
    void Start()
    {
        renderer = this.GetComponent<MeshRenderer>();
        renderer.enabled = false;
    }

    // Update is called once per frame
    //void Update()
    //{
    //   
    //}

    /// <summary>
    /// Universal linking function to be present in all button classes. A universal backend.
    /// </summary>
    public void externalInteractFunc()
    {
        //Debug.Log("Handshake");
        doorButtonOverride();
    }

    //close and lock door, or unlock if locked.
    public void doorButtonOverride()
    {
        door.GetComponentInChildren<DoorAnimator>().buttonResponse();
    }

    void OnMouseOver()
    {
        if (!renderer.enabled)
        {
            //Debug.Log("enabled?");
            renderer.enabled = true;
        }
    }

    void OnMouseExit()
    {
        if (renderer.enabled)
        {
            //Debug.Log("disabled?");
            renderer.enabled = false;
        }
    }
}
