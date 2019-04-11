using UnityEngine;

public class LockPlayerMouse : MonoBehaviour
{

	#region Variables

	private bool hasEscaped;

	#endregion

	#region Methods

	void Start()
    {
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (hasEscaped)
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
				hasEscaped = false;
			}
			else
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
				hasEscaped = true;
			}
			
		}
	}

	#endregion
}
