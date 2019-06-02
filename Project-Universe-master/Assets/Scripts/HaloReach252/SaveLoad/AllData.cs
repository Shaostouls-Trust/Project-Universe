using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class AllData
{
	public PlayerSaveData playerSaveData;
	public List<SaveObjectData> objectsData;
	public static AllData Instance;

	public AllData()
	{
		Instance = this;
	}
	/*
	public void GetAllData(Player player)
	{
		playerSaveData = new PlayerSaveData();
		objectsData = new List<SaveObjectData>();

		playerSaveData.GetData(player);

		for (int i = 0; i < ObjectManager.Instance.objects.Count; i++)
		{
			ObjectManager.Instance.objects[i].SaveObjectData();
			SaveObjectData temp = ObjectManager.Instance.objects[i].m_objectData;
			objectsData.Add(temp);
		}
	}*/

	public void SetAllData(AllData data)
	{
		playerSaveData = data.playerSaveData;
		objectsData = data.objectsData;
		Instance = data;
	}
}
