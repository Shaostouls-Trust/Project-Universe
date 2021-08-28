using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[Serializable]
public class ItemScript
{
    public enum ItemType
    {
        Stick,
        iron,
        None,
    }
    public ItemType itemtype;
    public int amount = 1;
}
public class DragAnDrop : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField] private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup CG;
    private void Awake()
    {
        canvas = FindObjectOfType<canvasReferencetoscript>().GetComponent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        CG = GetComponent<CanvasGroup>();
    }
    public void OnPointerDown(PointerEventData eventData)
    {
      //  Debug.Log("PointDown..");
    }
    public void OnEndDrag(PointerEventData eventData)
    {
     //   Debug.Log("OnEndDrag");
        CG.alpha = 1f;
        CG.blocksRaycasts = true;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
       // Debug.Log("OnBeginDrag");
        CG.alpha = .5f;
        CG.blocksRaycasts = false;
    }
    public void OnDrag(PointerEventData eventData)
    {
        //Debug.Log("OnDrag");
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

}
