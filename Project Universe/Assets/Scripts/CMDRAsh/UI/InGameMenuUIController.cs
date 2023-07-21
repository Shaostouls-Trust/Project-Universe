using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectUniverse.Player.PlayerController;
using Unity.Netcode;
//using UnityEditor;
using UnityEngine.SceneManagement;

public class InGameMenuUIController : MonoBehaviour
{
    private ProjectUniverse.PlayerControls controls;
    [SerializeField] private Canvas menu;
    [SerializeField] private GameObject ConfirmQuitPanel;
    [SerializeField] private GameObject LoadingScreenSplash;
    [SerializeField] private Image LoadingScreenBar;
    [SerializeField] private GameObject SelectScenePanel;
    [SerializeField] private AudioSource musicSrc;
    private string SceneToLoad;
    private GameObject player;
    private AsyncOperation async;
    private bool menuOpen = false;

    private void Start()
    {
        NetworkObject playNet = NetworkManager.Singleton.LocalClient.PlayerObject;
            //NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        if(playNet != null)
        {
            controls = playNet.gameObject.GetComponent<SupplementalController>().PlayerController;
        }
        //if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
        //{
        //    controls = networkedClient.PlayerObject.gameObject.GetComponent<SupplementalController>().PlayerController;
        //}
        else
        {
            controls = new ProjectUniverse.PlayerControls();
        }

        controls.Player.Escape.Enable();
        controls.Player.Escape.performed += ctx =>
        {
            if (player == null)
            {
                //NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient)
                //if ()
                //{
                    player = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;
                //}
            }

            SupplementalController playersc = player.GetComponent<SupplementalController>();
            if (playersc.FleetBoyOut)
            {
                playersc.ShowHideInventory();
            }
            else //only show the main menu if the inventory FB is closed
            {
                if (!menuOpen)
                {
                    menuOpen = true;
                    playersc.LockScreenAndFreeCursor();
                    menu.enabled = true;
                }
                else
                {
                    menuOpen = false;
                    playersc.FreeScreenAndLockCursor();
                    menu.enabled = false;
                }
            }
        };
    }

    public void ReturnToGame()
    {
        menu.enabled = false;
        menuOpen = false;
        player.GetComponent<SupplementalController>().FreeScreenAndLockCursor();
    }

    public void BackToMainMenuBar2()
    {
        SelectScenePanel.SetActive(false);
    }

    public void gotoScenePanel()
    {
        SelectScenePanel.SetActive(true);
    }

    public void SelectSceneToLoad(string scene)
    {
        SceneToLoad = scene;
        Debug.Log("Now We Load That Scene");
    }

    public void LoadScene()
    {
        if (musicSrc != null)
        {
            musicSrc.Stop();
        }
        //Save player
        //player.GetComponent<SupplementalController>().SavePlayer();
        StartCoroutine(LoadingBar(SceneToLoad));
    }

    IEnumerator LoadingBar(string scenename)
    {
        LoadingScreenSplash.SetActive(true);
        async = SceneManager.LoadSceneAsync(scenename);
        async.allowSceneActivation = false;
        //RectTransform rect = LoadingScreenBar.GetComponent<RectTransform>();
        yield return null;
        do
        {
            LoadingScreenBar.fillAmount = async.progress;
            yield return null;
        } while (async.progress < 0.9f);

        LoadingScreenBar.fillAmount = 1;
        async.allowSceneActivation = true;

        //Try to autoconnect player as host
        yield return null;
        //when scene is loaded, connect player to scene 
        //(Eventually need to check if we're connecting to a multiplayer game and so need to connect as client, not host)
        if (!NetworkManager.Singleton.IsHost)
        {
            Debug.Log("No Host");
        }
        NetworkManager.Singleton.StartHost();
        yield return null;
        //load player data
        //if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
        //{
        //   GameObject player = networkedClient.PlayerObject.gameObject;
        //    player.GetComponent<SupplementalController>().LoadPlayer();
        //}
        //take down splash
        LoadingScreenSplash.SetActive(false);
        yield return null;

    }

    public void ConfirmQuitToMenu()
    {
        //Save player
        //player.GetComponent<SupplementalController>().SavePlayer();
        //disconnect
        if (NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.DisconnectClient(NetworkManager.Singleton.LocalClientId);
        }
        else if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.Shutdown();
        }
        //Quit to tile/splash screen
        LoadingScreenSplash.SetActive(true);
        ConfirmQuitPanel.SetActive(false);
        StartCoroutine(LoadingBar("MainMenu"));
        
        //But for now:
        //Application.Quit();
    }

    public void CancelQuitToMenu()
    {
        ConfirmQuitPanel.SetActive(false);
    }

    public void QuitToMenu()
    {
        //if (Application.isEditor)
        //{
            //player.GetComponent<SupplementalController>().SavePlayer();
        //    EditorApplication.ExitPlaymode();
        //}
        //else
        //{
            //Confirmation box
            ConfirmQuitPanel.SetActive(true);
        //}
    }
}
