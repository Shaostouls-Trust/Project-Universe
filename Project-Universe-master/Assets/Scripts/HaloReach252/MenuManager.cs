using UnityEngine;
using Mirror;

public class MenuManager : MonoBehaviour
{
	#region Variables
	public NetworkManager networkManager;
	public TelepathyTransport transport;
	NetworkClient myClient;
	#endregion

	#region Methods

	private void Start()
	{
		networkManager.networkAddress = "65.130.240.224";
		transport.port = 25565;
	}

	public void StartServer()
	{
		networkManager.StartServer();
	}

	public void StartLocalServer()
	{
		networkManager.StartHost();
	}

	public void JoinServer()
	{
		networkManager.StartClient();
	}
	
	#endregion
}
