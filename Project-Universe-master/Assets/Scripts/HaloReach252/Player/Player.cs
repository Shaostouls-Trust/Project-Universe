using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
	#region Variables
	public static Player Instance;

	[HideInInspector]
	private PlayerSaveData saveData;
	public Transform playerCam;
	#endregion

	#region Methods

	void Awake()
	{
		if(Instance == null)
			Instance = this;
	}

	void Start()
	{
		if (SaveLoad.Instance.m_isSceneBeingLoaded)
		{
			saveData = SaveLoad.Instance.allData.playerSaveData;
			transform.position = new Vector3(saveData.posX, saveData.posY, saveData.posZ + 0.1f);
			GetComponent<PlayerLook>().mouseY = saveData.camLook;
			transform.rotation = Quaternion.Euler(new Vector3(0, saveData.playerLook, 0));
			SaveLoad.Instance.requiredLoads[0] = true;
			SaveLoad.Instance.CheckLoad();
		}
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.F5))
		{
			SaveLoad.Instance.SaveData();
		}

		if (Input.GetKeyDown(KeyCode.F9))
		{
			SaveLoad.Instance.LoadData();
		}

		if (Input.GetMouseButtonDown(0))
		{
			RaycastHit hit;
			if(Physics.Raycast(playerCam.position, playerCam.transform.forward, out hit))
			{
				if(hit.transform.GetComponent<TestLoadObject>() != null)
				{
					hit.transform.GetComponent<TestLoadObject>().Explode(600f);
				}
			}
		}

	}

	#endregion
}
