using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GridSlot : MonoBehaviour, IDropHandler
{

    [HideInInspector] public string name;
    [HideInInspector] public string transformofmat;
    [HideInInspector] protected CraftingTablet CT;

   
    
    private void Start()
    {
        CT = FindObjectOfType<CraftingTablet>();

    }
    
    public void OnDrop(PointerEventData eventData)
    {

       // Debug.Log("OnDrop");
        if (eventData.pointerDrag != null)
        {
            eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition;
            name = eventData.pointerDrag.gameObject.name;
            transformofmat = eventData.pointerDrag.transform.position.ToString();
           // Debug.Log(name);
           // Debug.Log(transformofmat);
            CT.RecieveMaterialInfo(name, transformofmat);
        }
    }
}
