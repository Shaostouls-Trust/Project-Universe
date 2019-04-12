using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveLoad : MonoBehaviour
{
	#region Variables
	public static SaveLoad Instance;

	public AllData allData;

	public bool m_isSceneBeingLoaded = false;

	public bool[] requiredLoads = new bool[2];
	#endregion

	#region Methods

	void Awake()
	{
		if (Instance == null)
			Instance = this;
		else if (Instance != null)
		{
			Destroy(gameObject);
		}

		DontDestroyOnLoad(gameObject);
		requiredLoads = new bool[2];
	}

	public void SaveData()
	{
		allData = new AllData();
		allData.GetAllData();

		if (!Directory.Exists("Saves"))
			Directory.CreateDirectory("Saves");

		BinaryFormatter formatter = new BinaryFormatter();
		FileStream saveFile = File.Create("Saves/save.binary");

		formatter.Serialize(saveFile, allData);

		saveFile.Close();
	}
	
	public void LoadData()
	{
		BinaryFormatter formatter = new BinaryFormatter();
		FileStream saveFile = File.Open("Saves/save.binary", FileMode.Open);

		allData = (AllData)formatter.Deserialize(saveFile);
		AllData.Instance.SetAllData(allData);

		saveFile.Close();

		m_isSceneBeingLoaded = true;
		int loadScene = allData.playerSaveData.sceneId;
		SceneManager.LoadScene(loadScene);
	}

	public void CheckLoad()
	{
		if (IsAllLoaded())
		{
			m_isSceneBeingLoaded = false;
			for (int i = 0; i < requiredLoads.Length-1; i++)
			{
				requiredLoads[i] = false;
			}
		}
	}

	//Checks if all the required scripts have loaded their data
	bool IsAllLoaded()
	{
		for (int i = 0; i < requiredLoads.Length-1; i++)
		{
			if (requiredLoads[i] == false)
				return false;
		}

		return true;
	}

	#endregion
}

//The base object for savable objects, not used on its own
[System.Serializable]
public class SaveObject
{
	public int sceneId;
}

//The players save data
[System.Serializable]
public class PlayerSaveData : SaveObject
{
	#region Variables
	public float posX, posY, posZ;
	public float camLook, playerLook;
	#endregion

	public void GetData()
	{
		sceneId = SceneManager.GetActiveScene().buildIndex;

		posX = Player.Instance.gameObject.transform.position.x;
		posY = Player.Instance.gameObject.transform.position.y;
		posZ = Player.Instance.gameObject.transform.position.z;

		camLook = Player.Instance.gameObject.GetComponent<PlayerLook>().mouseY;
		playerLook = Player.Instance.gameObject.transform.localEulerAngles.y;
	}

}

//An objects save data
[System.Serializable]
public class SaveObjectData : SaveObject
{
	public string objectId;
	public bool savePos, saveRot, hasRigidbody;
	public float posX, posY, posZ;
	public float rotX, rotY, rotZ;
	public float velX, velY, velZ;

	public void GetData(LoadableObject target)
	{
		objectId = target.objectId;

		sceneId = SceneManager.GetActiveScene().buildIndex;

		savePos = target.savePos;
		saveRot = target.saveRot;
		hasRigidbody = target.hasRigidbody;

		if (savePos)
		{
			posX = target.gameObject.transform.position.x;
			posY = target.gameObject.transform.position.y;
			posZ = target.gameObject.transform.position.z;
		}
		
		if (saveRot)
		{
			rotX = target.gameObject.transform.localEulerAngles.x;
			rotY = target.gameObject.transform.localEulerAngles.y;
			rotZ = target.gameObject.transform.localEulerAngles.z;
		}
		
		if (hasRigidbody)
		{
			velX = target.gameObject.GetComponent<Rigidbody>().velocity.x;
			velY = target.gameObject.GetComponent<Rigidbody>().velocity.y;
			velZ = target.gameObject.GetComponent<Rigidbody>().velocity.z;
		}
	}
}