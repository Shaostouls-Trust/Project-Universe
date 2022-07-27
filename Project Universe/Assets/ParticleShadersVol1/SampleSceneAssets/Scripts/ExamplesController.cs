using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class ExamplesController : MonoBehaviour {

	[Serializable]
	public class Example
	{
		public string Name;
		[Multiline]
		public string Description;
		public GameObject Prefab;
	}

	public Example[] m_Examples;
	public GameObject m_Canvas = null;
	public Slider m_ExamplesSlider = null;

	private Text m_Decription = null;
	private Text m_DecriptionTitle = null;
	private GameObject m_CurrentPrefab = null;
	private int m_CurrentExample = 0;

	void Start()
	{
		m_Decription = GameObject.Find ("ExampleDescription").GetComponent<Text>();
		m_DecriptionTitle = GameObject.Find ("ExampleDescriptionTitle").GetComponent<Text>();
		activateExample (0);

		if (m_ExamplesSlider != null) {
			m_ExamplesSlider.maxValue = (int)(m_Examples.Length -1);
		}
	}

	public void NextExample() {
		if (m_Examples[m_CurrentExample].Prefab != m_CurrentPrefab && m_CurrentPrefab != null)
			Destroy (m_CurrentPrefab);
		m_CurrentExample ++;
		ClampExampleCount ();
		m_CurrentPrefab = GameObject.Instantiate (m_Examples[m_CurrentExample].Prefab);

		if (m_Decription != null)
			m_Decription.text = m_Examples [m_CurrentExample].Description;
		if (m_DecriptionTitle != null)
			m_DecriptionTitle.text = m_Examples [m_CurrentExample].Name;
	}

	public void PreviousExample() {
		if (m_Examples[m_CurrentExample].Prefab != m_CurrentPrefab && m_CurrentPrefab != null)
			Destroy (m_CurrentPrefab);
		m_CurrentExample --;
		ClampExampleCount ();
		m_CurrentPrefab = GameObject.Instantiate (m_Examples[m_CurrentExample].Prefab);

		if (m_Decription != null)
			m_Decription.text = m_Examples [m_CurrentExample].Description;
		if (m_DecriptionTitle != null)
			m_DecriptionTitle.text = m_Examples [m_CurrentExample].Name;
	}

	private void ClampExampleCount() {
		if (m_CurrentExample < 0)
			m_CurrentExample = m_Examples.Length - 1;
		if (m_CurrentExample > m_Examples.Length - 1)
			m_CurrentExample = 0;
	}

	public void activateExampleFromSlider () {
		if (m_ExamplesSlider != null) {
			activateExample((int)(m_ExamplesSlider.value));
		}
	}

	public void activateExample(int index)
	{
		index = Mathf.Clamp (index, 0, m_Examples.Length - 1);

		if (m_CurrentPrefab != m_Examples [index].Prefab)
		{
			if (m_CurrentPrefab != null) {
				Destroy (m_CurrentPrefab);
			}
				
			m_CurrentPrefab = null;
			m_CurrentPrefab = Instantiate (m_Examples [index].Prefab, Vector3.zero, Quaternion.identity);

			if (m_Decription != null) {
				m_Decription.text = m_Examples [index].Description;
			}
				
			if (m_DecriptionTitle != null) {
				m_DecriptionTitle.text = m_Examples [index].Name;
			}
		}

	}


}
