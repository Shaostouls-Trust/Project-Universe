using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CameraRig : EMUI {
	private Quaternion defaultRot;
	private Vector3 defaultPos = new Vector3(0f,0f,0f);
	public GameObject cam = null;
	public float zoomSens = 3.0f;
	public float rotSens = 6.0f;

	//private float zoom_amount = 0.0f;
	private Vector3 pos_old = new Vector3(0f, 0f, 0f);
	private GameObject pos_new = null;
	private bool m_UILockInstigator = false;

	void Start ()
	{
		defaultRot = transform.rotation;
		pos_new = new GameObject ("pos_new");

		pos_new.transform.SetParent (transform);
		pos_new.transform.position = new Vector3 (cam.transform.position.x, cam.transform.position.y, cam.transform.position.z);
		Quaternion new_rot = cam.transform.rotation;
		pos_new.transform.rotation = new_rot;
		defaultPos = pos_new.transform.position;

	}
	/*
	private bool CheckGUI()
	{
		bool canClick = false;
		if(Input.GetMouseButton(0))
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
	}*/

	void Update () {
		if (Input.GetAxis("Mouse ScrollWheel") != 0f)
		{
			float shift = Input.GetAxis("Mouse ScrollWheel") * 6f;
			pos_new.transform.Translate(Vector3.forward * shift);
		}
		pos_old = cam.transform.position;
		pos_old = Vector3.Lerp(pos_old, pos_new.transform.position, zoomSens * Time.deltaTime * 0.2f);
		cam.transform.position = pos_old;

		if (CheckGUI(0, ref m_UILockInstigator))
		{
			transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * rotSens, Space.World);
			transform.Rotate(Vector3.left * Input.GetAxis("Mouse Y") * rotSens, Space.Self);
		}
	}

	public void ResetTransform()
	{
		transform.rotation = defaultRot;
		pos_new.transform.position = defaultPos;
	}
}
