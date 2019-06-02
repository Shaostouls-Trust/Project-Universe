using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

	#region Variables
	public static GameManager gameManager;

	public enum MessageTypes {
		GAME_ERROR,
		INFORMATION,
		USER_INTERFACE
	}

	public MessageObject baseMessageObject;

	public Transform canvas;

	public Dictionary<int,GameObject> messages = new Dictionary<int, GameObject>();

	private int messageInt;

	public Texture2D defaultTexture;
	public GameObject defaultModel;
	#endregion

	#region Functions

	void Awake() {
		if (gameManager == null) gameManager = this;
		else Destroy(gameObject);
	}

	void Start() {
        
	}

	void Update() {
        
	}

	public void ShowErrorMessage(string message, MessageTypes messageType, int messageWidth, int messageHeight) {
		MessageObject messageObject = baseMessageObject;
		GameObject messageGameObject = Instantiate(messageObject.messageParent);
		messageGameObject.transform.SetParent(canvas);
		int messageId = Mathf.RoundToInt(Random.Range(1, 1000));
		messages.Add(messageId, messageGameObject);
		MessageItem currentItem = messageGameObject.GetComponent<MessageItem>();
		currentItem.messageId = messageId;
		switch (messageType) {
			case MessageTypes.GAME_ERROR:
				currentItem.background.color = Color.red;
				currentItem.titleText.text = "Critical Game Error!";
				currentItem.messageText.text = message;
				currentItem.background.GetComponent<RectTransform>().sizeDelta = new Vector2(messageWidth,messageHeight);
				break;
			case MessageTypes.INFORMATION:
				currentItem.background.color = Color.green;
				currentItem.titleText.text = "Info!";
				currentItem.messageText.text = message;
				currentItem.background.GetComponent<RectTransform>().sizeDelta = new Vector2(messageWidth, messageHeight);
				break;
			case MessageTypes.USER_INTERFACE:
				currentItem.background.color = Color.cyan;
				currentItem.titleText.text = "Alert!";
				currentItem.messageText.text = message;
				currentItem.background.GetComponent<RectTransform>().sizeDelta = new Vector2(messageWidth, messageHeight);
				break;
		}
		messageGameObject.transform.localScale = new Vector3(1, 1, 1);
		messageGameObject.transform.localPosition = new Vector3(0, 0, 0);
	}

	public void CloseMessage(int messageId, GameObject obj) {
		messages.Remove(messageId);
		Destroy(obj);
	}

	public void CreateRandomMessage() {
		int randType = Mathf.RoundToInt(Random.Range(0, 2));
		if (randType == 0)
			ShowErrorMessage("This is a test critical game error, hopefully it works but it may not and thats ok", MessageTypes.GAME_ERROR, 800, 300);
		else if (randType == 1)
			ShowErrorMessage("This is a test information message, hopefully it works, but it may not, and thats ok", MessageTypes.INFORMATION, 500, 200);
		else if (randType == 2)
			ShowErrorMessage("This is a test user interface message, hopefully it works, but it may not, and thats ok", MessageTypes.USER_INTERFACE, 1200, 800);
		else
			ShowErrorMessage("This is an actual error message, idk what the fuck you did to get it, but you did", MessageTypes.GAME_ERROR, 1000, 600);
	}

	#endregion
}

[CreateAssetMenu()]
public class MessageObject : ScriptableObject {

	// THE ONLY THING THAT SHOULD BE SET
	public GameObject messageParent;

}