using UnityEngine;
using System.Collections.Generic;

public class ObjectManager : MonoBehaviour
{
	#region Variables

	public static ObjectManager Instance;

	public List<TestLoadObject> objects = new List<TestLoadObject>();
	#endregion

	#region Methods

	void Awake()
	{
		if (Instance == null)
			Instance = this;
	}

	void Start()
	{
		objects = new List<TestLoadObject>();
		GameObject[] temp = GameObject.FindGameObjectsWithTag("SaveObject");
		for (int i = 0; i < temp.Length; i++)
			objects.Add(temp[i].GetComponent<TestLoadObject>());
		List<TestLoadObjectData> objectData = AllData.Instance.objects;
		if (SaveLoad.Instance.m_isSceneBeingLoaded)
		{
			for (int i = 0; i < objects.Count; i++)
			{
				objects[i].LoadObjectData(objectData[i]);
			}
			SaveLoad.Instance.m_loadNum++;
			SaveLoad.Instance.CheckLoad();
		}
	}

	#endregion
}
