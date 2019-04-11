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

	int m_numLoads = 2;
	public int m_loadNum;
	#endregion

	#region Methods

	void Awake()
	{
		if (Instance == null)
			Instance = this;

		DontDestroyOnLoad(gameObject);
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
		AllData.Instance = allData;

		saveFile.Close();

		m_isSceneBeingLoaded = true;
		int loadScene = allData.playerSaveData.sceneId;
		SceneManager.LoadScene(loadScene);
	}

	public void CheckLoad()
	{
		if (m_loadNum == m_numLoads)
		{
			m_isSceneBeingLoaded = false;
			m_loadNum = 0;
		}
	}

	#endregion
}

[System.Serializable]
public class SaveObject
{
	public int sceneId;
}

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

[System.Serializable]
public class TestLoadObjectData : SaveObject
{
	public string objectId;
	public float posX, posY, posZ;
	public float rotX, rotY, rotZ;

	public void GetData(TestLoadObject target)
	{
		objectId = target.objectId;

		sceneId = SceneManager.GetActiveScene().buildIndex;

		posX = target.gameObject.transform.position.x;
		posY = target.gameObject.transform.position.y;
		posZ = target.gameObject.transform.position.z;

		rotX = target.gameObject.transform.localEulerAngles.x;
		rotY = target.gameObject.transform.localEulerAngles.y;
		rotZ = target.gameObject.transform.localEulerAngles.z;
	}
}