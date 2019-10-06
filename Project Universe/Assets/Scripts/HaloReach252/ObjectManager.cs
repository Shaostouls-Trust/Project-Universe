using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ObjectManager : MonoBehaviour
{
	#region Variables
	public static ObjectManager Instance;

	public List<LoadableObject> objects = new List<LoadableObject>();
	List<SaveObjectData> objectsData = new List<SaveObjectData>();
	#endregion

	#region Methods

	void Awake()
	{
		if (Instance == null)
			Instance = this;
		ReloadObjects();
		SceneManager.sceneLoaded += LoadObjectsDelegate;
	}

	private void LoadObjectsDelegate(Scene scene, LoadSceneMode mode)
	{
		ReloadObjects();
	}

	public void ReloadObjects()
	{
		//Creates a new list of loadable objects and populates it at the beginning of the game & at every scene load
		objects = new List<LoadableObject>();
		GameObject[] temp = GameObject.FindGameObjectsWithTag("SaveObject");
		for (int i = 0; i < temp.Length; i++)
			objects.Add(temp[i].GetComponent<LoadableObject>());

		//Checks if the scene is being loaded, then loops through all the objects and loads them from the save file
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

	private void OnDestroy()
	{
		SceneManager.sceneLoaded -= LoadObjectsDelegate;
	}

	#endregion
}
