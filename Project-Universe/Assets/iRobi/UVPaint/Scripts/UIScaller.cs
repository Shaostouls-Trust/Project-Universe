using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIScaller : MonoBehaviour {
	public RectTransform LeftClick,RightClick,ScrollClick;
	public RectTransform Container;
	static public Texture2D current;
	static public int selected;
	static public float DecalSize=2f;
	int lastSelected;
	float height;
	public RectTransform TestButton;
//	int scrollCount = 0;
	RawImage[] images;
	float minY;

	void Awake()
	{
		images = Container.GetComponentsInChildren<RawImage> ();
	}

	void Update () {
		height = images [images.Length-1].rectTransform.rect.height;
		if (minY == 0)
			minY=Container.position.y;

		if (Input.GetMouseButton (0))
			LeftClick.localScale = Vector3.Lerp (LeftClick.localScale, Vector3.one * 1.5f, Time.deltaTime * 10);
		else
			LeftClick.localScale = Vector3.Lerp (LeftClick.localScale, Vector3.one, Time.deltaTime * 5);

		if (Input.GetMouseButton (1))
			RightClick.localScale = Vector3.Lerp (RightClick.localScale, Vector3.one * 1.5f, Time.deltaTime * 10);
		else
			RightClick.localScale = Vector3.Lerp (RightClick.localScale, Vector3.one, Time.deltaTime * 5);

		if (Input.GetMouseButton (2))
			ScrollClick.localScale = Vector3.Lerp (ScrollClick.localScale, Vector3.one * 1.5f, Time.deltaTime * 10);
		else
			ScrollClick.localScale = Vector3.Lerp (ScrollClick.localScale, Vector3.one, Time.deltaTime * 5);
		Vector3 oldPos = Container.position;

		Container.position = new Vector3 (oldPos.x,Mathf.Clamp( (oldPos.y - Input.mouseScrollDelta.y * height) ,minY-Container.rect.height+height,minY), oldPos.z);
		selected = (int)((-Container.position.y + Container.rect.height - minY) / height + 0.1f);
		int sel = images.Length - selected - 1;
		if (selected < images.Length && selected >= 0) {
			current = images [sel].texture as Texture2D;

			if (lastSelected != selected) {
				lastSelected = selected;
				DecalSize = images [sel].GetComponent<DecalProperties> ().size;
			}
		}
	}
}
