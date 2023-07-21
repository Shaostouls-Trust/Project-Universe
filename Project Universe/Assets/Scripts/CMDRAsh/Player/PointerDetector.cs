using Unity.Netcode;
using ProjectUniverse;
using ProjectUniverse.Player.PlayerController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PointerDetector : MonoBehaviour
{
    private GraphicRaycaster m_Raycaster;
    private PointerEventData m_PointerEventData;
    public EventSystem m_EventSystem;
    private PlayerControls controls;
    public Canvas canvas;

    void Start()
    {
        //Fetch the Raycaster from the GameObject (the Canvas)
        m_Raycaster = GetComponent<GraphicRaycaster>();
        //Fetch the Event System from the Scene
        //m_EventSystem = GetComponent<EventSystem>();
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
        {
            //controls = networkedClient.PlayerObject.gameObject.GetComponent<SupplementalController>().PlayerController;
        }
        else
        {
            //controls = new PlayerControls();
        }
        //controls.Player.Fire.Enable();
        //controls.Player.Fire.performed += ctx =>
        //{
        //    OnLeftClickUI();
        //};
    }

    private void OnLeftClickUI()
    {
        Debug.Log("LeftClickUI?");
        //Set up the new Pointer Event
        m_PointerEventData = new PointerEventData(m_EventSystem);
        //Set the Pointer Event Position to that of the mouse position
        m_PointerEventData.position = new Vector3(960f,540f,0f);//Input.mousePosition;
        //Debug.Log("MP:"+Input.mousePosition);

        //Create a list of Raycast Results
        List<RaycastResult> results = new List<RaycastResult>();

        //Raycast using the Graphics Raycaster and mouse click position
        //m_Raycaster.Raycast(m_PointerEventData, results);
        EventSystem.current.RaycastAll(m_PointerEventData, results);

        //For every result returned, output the name of the GameObject on the Canvas hit by the Ray
        foreach (RaycastResult result in results)
        {
            Debug.Log("Hit " + result.gameObject.name);
        }
    }

    // Update is called once per frame
    //void Update()
    //{
        
    //}

    public void ExternalInteractFunc()
    {
        //Debug.Log("Unlock cursor");
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
        {
            SupplementalController sc = networkedClient.PlayerObject.gameObject.GetComponent<SupplementalController>();
            sc.HideCenterUI();
            sc.UnlockOnlyCursor();
            Debug.Log("Camera: "+Camera.main);
            canvas.worldCamera = Camera.main;
        }
    }
}
