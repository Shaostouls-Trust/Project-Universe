using UnityEngine;
using UnityEngine.UI;

public class MessageItem : MonoBehaviour {
	#region Variables
	public int messageId;

	public Image background;

	public Text titleText;
	public Text messageText;
	#endregion
	
	#region Functions
	void Start() {
        
	}

	void Update() {
        
	}

	public void CloseMessage() {
		GameManager.gameManager.CloseMessage(messageId, gameObject);
	}

	#endregion
}
