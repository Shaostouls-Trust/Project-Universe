using UnityEngine;
using Mirror;

public class Player : NetworkBehaviour
{
	#region Variables
	[HideInInspector]
	public PlayerSaveData saveData;

	public GameObject playerCam;
	public GameObject pauseMenu;

	[SyncVar]
	bool isPaused;
	#endregion

	#region Methods

	void Start()
	{
		if (!isLocalPlayer)
		{
			playerCam.SetActive(false);
			return;
		}

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
		if (!isLocalPlayer)
			return;

		CmdGetInput();
	}

	[Command]
	void CmdGetInput()
	{
		//Saves the game state | Disabled while i work on network integration
		if (Input.GetKeyDown(KeyCode.F5))
		{
			//SaveLoad.Instance.SaveData(this);
		}

		//Loads the save file | Disabled while i work on network integration
		if (Input.GetKeyDown(KeyCode.F9))
		{
			//SaveLoad.Instance.LoadData(this);
		}

		//Adds explosive force to loadable objects when you left click, temporary for testing purposes
		if (Input.GetMouseButtonDown(0))
		{
			RaycastHit hit;
			if (Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out hit))
			{
				if (hit.transform.GetComponent<Rigidbody>() != null && hit.transform.GetComponent<LoadableObject>() != null)
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
