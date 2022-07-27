using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GetName : MonoBehaviour {
	public int m_Index = 0;
	void Start () {
		Text text = GetComponent<Text> ();
		ExamplesController controller = transform.parent.parent.GetComponent<ExamplesController> ();

		if (controller)
			text.text = controller.m_Examples [m_Index].Name;
	}
}
