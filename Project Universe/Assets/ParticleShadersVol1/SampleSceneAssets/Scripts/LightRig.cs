using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class LightRig : EMUI {

	public float rotSens = 15f;
	public float offsetSens = 0.3f;
	public GameObject DirectionalLight;
	public GameObject PointLights;
	public GameObject SpotLights;
	public ColorPicker m_ColorPicker;
	public enum LightsType {Directional, Point, Spot};

	[HideInInspector] public Light[] m_Lights;

	private LightsType curLightType;
	private GameObject curLightObject;
	private bool m_UILockInstigator = false;

	private bool m_AnimateLight = false;
	
	void Start() {
		SetDirectionalLight ();
	}

	public void SetPointLights() {
		ChangeLights (LightsType.Point);
	}
	public void SetSpotLights() {
		ChangeLights(LightsType.Spot);
	}
	public void SetDirectionalLight() {
		ChangeLights (LightsType.Directional);
	}

	public void ToggleLightAnimation() {
		if (m_AnimateLight == true)
			m_AnimateLight = false;
		else
			m_AnimateLight = true;
	}
		          

	private void ChangeLights(LightsType lightTypes)
	{
		Destroy (curLightObject);
		m_Lights = null;

		switch (lightTypes)
		{
		case LightsType.Directional:
			curLightType = LightsType.Directional;
			curLightObject = Instantiate(DirectionalLight);
			curLightObject.transform.position = new Vector3 (0, 1.8f, 0);
			break;

		case LightsType.Point:
			curLightType = LightsType.Point;
			curLightObject = Instantiate(PointLights);
			break;

		case LightsType.Spot:
			curLightType = LightsType.Spot;
			curLightObject = Instantiate(SpotLights);
			break;
		}

		m_Lights = curLightObject.GetComponentsInChildren<Light>();

		if (m_ColorPicker) {
			m_ColorPicker.SetCurrentColor ();
			m_ColorPicker.SetCurrentIntensity();
		}
	}

	void Update ()
	{
		if (curLightObject != null)
		{
			if (CheckGUI (2, ref m_UILockInstigator))
			{
				switch (curLightType)
				{
				case LightsType.Directional:
					curLightObject.transform.Rotate (Vector3.up * Input.GetAxis ("Mouse X") * rotSens, Space.World);
					curLightObject.transform.Rotate (Vector3.left * Input.GetAxis ("Mouse Y") * rotSens, Space.Self);
					break;

				case LightsType.Point:
					curLightObject.transform.Rotate (Vector3.up * Input.GetAxis ("Mouse X") * rotSens, Space.World);
					curLightObject.transform.Translate (Vector3.up * Input.GetAxis ("Mouse Y") * offsetSens, Space.World);
					break;

				case LightsType.Spot:
					curLightObject.transform.Rotate (Vector3.up * Input.GetAxis ("Mouse X") * rotSens, Space.World);
					curLightObject.transform.Translate (Vector3.up * Input.GetAxis ("Mouse Y") * offsetSens, Space.World);
					break;
				}

				Vector3 pos = curLightObject.transform.position;
				pos.y = Mathf.Clamp (pos.y, 0, 3.3f);
				curLightObject.transform.position = pos;
			}
				
			if (m_AnimateLight == true)
			{
				curLightObject.transform.RotateAround (transform.position, Vector3.up, 0.6f);
			}
		}

	}
}
