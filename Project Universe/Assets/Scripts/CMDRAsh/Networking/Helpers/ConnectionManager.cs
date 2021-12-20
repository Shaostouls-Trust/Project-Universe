using MLAPI;
using MLAPI.Spawning;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI.Transports.UNET;
using TMPro;
using ProjectUniverse.Player.PlayerController;
using UnityEngine.Networking;
using MLAPI.Connection;
using System.Threading.Tasks;

namespace ProjectUniverse.Networking
{
    public class ConnectionManager : MonoBehaviour
    {
        [SerializeField] private GameObject DefSpawnPosMarker;
        [SerializeField] private GameObject ConnectionUIGO;
        public Camera LobbyCam;
        private string IPaddress = "127.0.0.1";
        UNetTransport uNetTransportForClient;

        public void Host()
        {
            Debug.Log("Start Host");
            LobbyCam.gameObject.SetActive(false);
            ConnectionUIGO.SetActive(false);
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
            NetworkManager.Singleton.StartHost(new Vector3?(DefSpawnPosMarker.transform.position));//starthost takes in the starting position
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
            {
                networkedClient.PlayerObject.gameObject.GetComponent<SupplementalController>().LockCursor();
            }
            Debug.Log("Started Host");
        }

        private void ApprovalCheck(byte[] connectionData, ulong clientID, NetworkManager.ConnectionApprovedDelegate callback)
        {
            Debug.Log("ApprovalCheck");
            //check the incoming data (password) against a the server password
            bool approved = System.Text.Encoding.ASCII.GetString(connectionData) == "79213";
            callback(true, null, approved, DefSpawnPosMarker.transform.position, Quaternion.identity);
        }

        private System.Action<ulong> ClientDisconnectCallback()
        {
            return delegate {
                Debug.Log("Client Disconnect");
            };
        }

        public async void Join()
        {
            Debug.Log("Start Client");
            //Can't grab UNetTransport if using Photon.
            //uNetTransportForClient = NetworkManager.Singleton.GetComponent<UNetTransport>();
            //uNetTransportForClient.ConnectAddress = IPaddress;

            //This adds the password to the client data for the server to check
            NetworkManager.Singleton.OnClientDisconnectCallback += ClientDisconnectCallback();
            NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes("79213");
            NetworkManager.Singleton.StartClient();
            Debug.Log("Awaiting connection");
            await ClientConnected();
            Debug.Log("Client connected or dropped");
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