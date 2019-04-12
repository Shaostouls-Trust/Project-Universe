using UnityEngine;
using System.Collections.Generic;

public class ObjectManager : MonoBehaviour
{
	#region Variables

	public static ObjectManager Instance;

	public List<TestLoadObject> objects = new List<TestLoadObject>();
	List<TestLoadObjectData> objectsData = new List<TestLoadObjectData>();
	#endregion

	#region Methods

	void Awake()
	{
		if (Instance == null)
			Instance = this;
		ReloadObjects();
	}

	public void ReloadObjects()
	{
		objects = new List<TestLoadObject>();
		GameObject[] temp = GameObject.FindGameObjectsWithTag("SaveObject");
		for (int i = 0; i < temp.Length; i++)
			objects.Add(temp[i].GetComponent<TestLoadObject>());
		if (SaveLoad.Instance.m_isSceneBeingLoaded)
		{
			objectsData = SaveLoad.Instance.allData.objectsData;
			if (objectsData.Count != 0)
			{
				for (int i = 0; i < objects.Count; i++)
				{
					objects[i].LoadObjectData(objectsData[i]);
				}
				SaveLoad.Instance.requiredLoads[1] = true;
				SaveLoad.Instance.CheckLoad();
			}
		}
	}

	void OnLevelWasLoaded(int level)
	{
		if (level == SaveLoad.Instance.allData.playerSaveData.sceneId)
			ReloadObjects();
	}

	#endregion
}
