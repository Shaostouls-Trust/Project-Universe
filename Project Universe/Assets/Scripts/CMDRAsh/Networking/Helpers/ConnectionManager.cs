using Unity.Netcode;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ProjectUniverse.Player.PlayerController;
using UnityEngine.Networking;
using System.Threading.Tasks;
using ProjectUniverse.Ship;
using ProjectUniverse.Environment.Volumes;
using Unity.Netcode.Transports.UTP;

namespace ProjectUniverse.Networking
{
    public class ConnectionManager : MonoBehaviour
    {
        [SerializeField] private GameObject DefSpawnPosMarker;
        [SerializeField] private GameObject ConnectionUIGO;
        public Camera LobbyCam;
        private string IPaddress = "127.0.0.1";
        UnityTransport unityTransportForClient;
        [SerializeField] private GameObject loadScreen;
        public RenderStateManager rsmPlayer;
        //[SerializeField] private NetworkManager networkManager;

        /// <summary>
        /// Temporary Host function for singleplayer demo
        /// </summary>
        private void Start()
        {
            Debug.Log("RT: " + SystemInfo.supportsRayTracing);
            Debug.Log("Linke Starto!");
            Host();
            if(loadScreen != null)
            {
                loadScreen.SetActive(false);
            }
        }

        public void Host()
        {
            Debug.Log("Start Host");
            LobbyCam.gameObject.SetActive(false);
            ConnectionUIGO.SetActive(false);
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
            NetworkManager.Singleton.StartHost();//starthost takes in the starting position
            NetworkObject playNet = NetworkManager.Singleton.LocalClient.PlayerObject;
            //if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
            //{
            if (playNet != null)
            {
                Debug.Log("Startcode");
                //move to start position (temp)
                playNet.GetComponent<SupplementalController>().TeleportPlayerServerRPC(DefSpawnPosMarker.transform.position);
                //Debug.Log(playNet.name);
                playNet.gameObject.GetComponent<SupplementalController>().LockOnlyCursor();
                playNet.gameObject.GetComponent<PlayerVolumeController>().tempRSMPlayer = rsmPlayer;
            }
            //}
            Debug.Log("Started Host");
        }

        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            //bool approved = System.Text.Encoding.ASCII.GetString(request.Payload) == "79213";
            //if (approved)
            //{
            var clientId = request.ClientNetworkId;
            Debug.Log("Payload: "+System.Text.Encoding.ASCII.GetString(request.Payload));
            //Debug.Log("Spawn At: " + response.Position);
            response.Approved = true;
            response.Position = new Vector3?(DefSpawnPosMarker.transform.position);
            response.CreatePlayerObject = true;
            //}
            //else
            //{
            //    response.Reason = "Bad Access Code";
            //    response.Approved = false;
            //}
        }

        private System.Action<ulong> ClientDisconnectCallback()
        {
            return delegate {
                Debug.Log("Client Disconnect");
            };
        }

        public async void Join()
        {
            //Debug.Log("Start Client");
            //Can't grab UNetTransport if using Photon.
            //uNetTransportForClient = NetworkManager.Singleton.GetComponent<UNetTransport>();
            //uNetTransportForClient.ConnectAddress = IPaddress;

            //This adds the password to the client data for the server to check
            NetworkManager.Singleton.OnClientDisconnectCallback += ClientDisconnectCallback();
            NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes("79213");
            NetworkManager.Singleton.StartClient();
            //Debug.Log("Awaiting connection");
            await ClientConnected();
            //Debug.Log("Client connected or dropped");
            LobbyCam.gameObject.SetActive(false);
            ConnectionUIGO.SetActive(false);
            //Debug.Log("Connected Client "+NetworkManager.Singleton.IsConnectedClient);
            //Debug.Log("Client "+NetworkManager.Singleton.IsClient);
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
            {
                networkedClient.PlayerObject.gameObject.GetComponent<PlayerController>().LockCursor();
            }
        }
        public void Server()
        {
            Debug.Log("Start Server");
            NetworkManager.Singleton.StartServer();
        }
        public void SetIPAddressOnChanged(string value)
        {
            IPaddress = value;
        }

        public async Task<bool> ClientConnected()
        {
            while (!NetworkManager.Singleton.IsConnectedClient)
            {
                await Task.Delay(10);
            }
            return true;
        }
    }
}