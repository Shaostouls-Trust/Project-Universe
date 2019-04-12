using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
	#region Variables
	public static Player Instance;

	[HideInInspector]
	private PlayerSaveData saveData;
	public Transform playerCam;

	public GameObject pauseMenu;
	bool isPaused;
	#endregion

	#region Methods

	void Awake()
	{
		if(Instance == null)
			Instance = this;
	}

	void Start()
	{
		//Checks if the scene is being loaded, then loads the player data from the load  file
		if (SaveLoad.Instance.m_isSceneBeingLoaded)
		{
			saveData = SaveLoad.Instance.allData.playerSaveData;
			transform.position = new Vector3(saveData.posX, saveData.posY, saveData.posZ + 0.1f);
			GetComponent<PlayerLook>().mouseY = saveData.camLook;
			transform.rotation = Quaternion.Euler(new Vector3(0, saveData.playerLook, 0));
			SaveLoad.Instance.requiredLoads[0] = true;
			SaveLoad.Instance.CheckLoad();
		}

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

	}

	void Update()
	{
		//Saves the game state
		if (Input.GetKeyDown(KeyCode.F5))
		{
			SaveLoad.Instance.SaveData();
		}

		//Loads the save file
		if (Input.GetKeyDown(KeyCode.F9))
		{
			SaveLoad.Instance.LoadData();
		}

		//Adds explosive force to loadable objects when you left click, temporary for testing purposes
		if (Input.GetMouseButtonDown(0))
		{
			RaycastHit hit;
			if(Physics.Raycast(playerCam.position, playerCam.transform.forward, out hit))
			{
				if(hit.transform.GetComponent<Rigidbody>() != null && hit.transform.GetComponent<LoadableObject>() != null)
				{
					hit.transform.GetComponent<LoadableObject>().Explode(600f);
				}
			}
		}

		//Handles pausing
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			isPaused = !isPaused;
			Cursor.visible = isPaused;
			pauseMenu.SetActive(isPaused);
			if (isPaused)
				Cursor.lockState = CursorLockMode.None;
			else
				Cursor.lockState = CursorLockMode.Locked;
		}

	}

	#endregion
}
