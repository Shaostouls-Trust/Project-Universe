using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class AllData
{
	public PlayerSaveData playerSaveData;
	public List<TestLoadObjectData> objects = new List<TestLoadObjectData>();
	public static AllData Instance;

	public AllData()
	{
		Instance = this;
	}

	public void GetAllData()
	{
		playerSaveData = new PlayerSaveData();
		objects = new List<TestLoadObjectData>();

		playerSaveData.GetData();

		for (int i = 0; i < ObjectManager.Instance.objects.Count; i++)
		{
			ObjectManager.Instance.objects[i].SaveObjectData();
			TestLoadObjectData temp = ObjectManager.Instance.objects[i].m_objectData;
			objects.Add(temp);
		}
	}
}
