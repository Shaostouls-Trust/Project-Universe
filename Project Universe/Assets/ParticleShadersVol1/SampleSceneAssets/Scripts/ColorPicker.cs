using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ColorPicker : EMUI {

	private Texture2D m_ColorField;
	private RectTransform m_RectTransform;
	private Rect m_Rect;
	private Canvas m_Canvas;
	private Slider m_IntensitySlider;

	private bool m_UILockInstigator = false;
	[SerializeField] private LightRig m_LightRig;
	[SerializeField] private Image m_KnobImage;
	[SerializeField] private RectTransform m_KnobTransform;

	// Use this for initialization
	void Start () {
		m_ColorField = gameObject.GetComponent<Image> ().sprite.texture;
		m_RectTransform = gameObject.GetComponent<RectTransform>();
		m_Rect = m_RectTransform.rect;
		m_Canvas = GetComponentInParent<Canvas> ();
		m_IntensitySlider = GetComponentInChildren<Slider> ();

		SetCurrentColor ();
		SetCurrentIntensity ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButton (0))
		{
			if (!UIHelpOverlay)
			{
				if (!UIClicked || (UIClicked && m_UILockInstigator) )
				{
					PointerEventData pointer = new PointerEventData (EventSystem.current);
					pointer.position = Input.mousePosition;
				
					List<RaycastResult> raycastResults = new List<RaycastResult> ();
					EventSystem.current.RaycastAll (pointer, raycastResults);
				
					if (raycastResults.Count > 0)
					{
						if (raycastResults [0].gameObject == this.gameObject) {
							Vector2 imageClickPos = (raycastResults [0].screenPosition - new Vector2 (m_RectTransform.position.x, m_RectTransform.position.y)) / m_Canvas.scaleFactor;
							m_KnobImage.color = m_ColorField.GetPixel ((int)(imageClickPos.x / (m_Rect.width) * m_ColorField.width),
						                                          (int)(imageClickPos.y / (m_Rect.height) * m_ColorField.height));
							m_KnobTransform.localPosition = new Vector3 (imageClickPos.x,
						                                            imageClickPos.y,
						                                            m_KnobTransform.localPosition.z);
							SetCurrentColor ();
						}
					}

					UIClicked = true;
					m_UILockInstigator = true;
				}
			}
		}

		if (Input.GetMouseButtonUp (0) && m_UILockInstigator) {
			UIClicked = false;
			m_UILockInstigator = false;
		}
	}

	public Color GetCurrentColor() {
		if (m_KnobImage)
			return m_KnobImage.color;
		else
			return Color.white;
	}
	
	public void SetCurrentColor() {
		if (m_LightRig && m_KnobImage)
		{
			for (int i = 0; i < m_LightRig.m_Lights.Length; i++)
			{
				m_LightRig.m_Lights[i].color = m_KnobImage.color;
			}
		}
	}

	public float GetCurrentIntensity() {
		if (m_IntensitySlider)
			return m_IntensitySlider.value;
		else
			return 1f;
	}

	public void SetCurrentIntensity() {
		if (m_LightRig && m_IntensitySlider)
		{
			for (int i = 0; i < m_LightRig.m_Lights.Length; i++)
			{
				m_LightRig.m_Lights[i].intensity = m_IntensitySlider.value;
			}
		}
	}

	private bool CheckGUI()
	{
		bool canClick = false;
		if(Input.GetMouseButton(2))
		{
			PointerEventData pointer = new PointerEventData(EventSystem.current);
			pointer.position = Input.mousePosition;
			
			List<RaycastResult> raycastResults = new List<RaycastResult>();
			EventSystem.current.RaycastAll(pointer, raycastResults);
			
			if(raycastResults.Count > 0)
			{
				if(raycastResults[0].gameObject.layer == 5)
					canClick = false;
				else
					canClick = true;
			}
			else
				canClick = true;
		}
		return canClick;
	}
}
