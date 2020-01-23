using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerGuiController : MonoBehaviour
{
    [SerializeField] Camera mainCamera;
    [SerializeField] GameObject mapGameObject;
    [SerializeField] GameObject Ammo_Indicator;
    [SerializeField] Sprite SelectionCursor;
    [SerializeField] Sprite MouseCursor;
    [SerializeField] Image cursorObject;
    [SerializeField] int CursorCase = 0;
    [SerializeField] private bool toggleCursorLock = false;
    [SerializeField] Vector3 DefaultPosition;
    [SerializeField] GameObject[] Windows;
    [SerializeField] bool[] windowBool;

    // Update is called once per frame

    private void Start()
    {
        //Set cursor default Position
        DefaultPosition = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
    }
    void Update()
    {
        SetCursor(CursorCase);
    }
    public void OpenWindow(int Case)
    {
        if (Windows[Case] != null && windowBool[Case] != null)
        {
            Windows[Case].gameObject.SetActive(!windowBool[Case]);
            windowBool[Case] = !windowBool[Case];
        }
        else { Debug.Log("No window in that slot!"); }
    }
    //Lock and unlock cursor to center of screen
    public void SetCursor(int Case)
    {
        if (Case == 1)
        {
            cursorObject.sprite = SelectionCursor;
            cursorObject.rectTransform.position = DefaultPosition;
        }
        if (Case == 2)
        {
            cursorObject.sprite = MouseCursor;
            cursorObject.rectTransform.position = Input.mousePosition;
        }
        else { }
    }

    //Lockcursor function
    public void LockCursor()
    {

        if (toggleCursorLock == false)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = true;
            CursorCase = 1;
        }
        if (toggleCursorLock == true)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = false;
            CursorCase = 2;
        }
        toggleCursorLock = !toggleCursorLock;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }
}