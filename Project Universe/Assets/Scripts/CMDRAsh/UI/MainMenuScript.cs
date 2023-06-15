using MLAPI;
using ProjectUniverse.Player.PlayerController;
using ProjectUniverse.Serialization;
using ProjectUniverse.Serialization.Handler;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
//using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuScript : MonoBehaviour
{
    private string playerLastLoadedScene;
    private string selectedSceneToLoad = "";
    [SerializeField] private Button Continue;
    [SerializeField] private Button Load;
    //[SerializeField] private Button Quit;
    [SerializeField] private string NewGameSceneString;
    private AsyncOperation async;
    [SerializeField] private GameObject ConfirmQuitPanel;
    [SerializeField] private GameObject ConfirmContinuePanel;
    [SerializeField] private GameObject ConfirmNewPanel;
    [SerializeField] private GameObject LoadingScreenSplash;
    [SerializeField] private Image LoadingScreenBar;
    [SerializeField] private GameObject SelectScenePanel;
    [SerializeField] private GameObject SelectSceneContentParent;
    [SerializeField] private GameObject LoadGameButtonPrefabs;
    [SerializeField] private GameObject NotesWindow;
    [SerializeField] private AudioSource musicSrc;

    private void Awake()
    {
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        //LoadPlayerLastScene();
        if (musicSrc != null && !musicSrc.isPlaying)
        {
            musicSrc.Play();
        }
        LoadingScreenSplash.SetActive(false);
    }

    public void LoadPlayerLastScene()
    {
        PlayerData data = SerializationHandler.Load("Player_current");
        if (data != null)
        {
            try
            {
                //This will load the player's last scene, if any. Failure means to previous scenes.
                playerLastLoadedScene = data.PlayerSceneData.PlayerSceneString;
            }catch(Exception e)
            {
                Continue.enabled = false;
                Load.enabled = false;
            }
        }
        else
        {
            Debug.LogError("Failed to Load Last Level");
        }
    }

    public void CancelNewGame()
    {
        ConfirmNewPanel.SetActive(false);
    }
    public void ConfirmNewGame()
    {
        ConfirmNewPanel.SetActive(false);
        StartCoroutine(LoadScene(NewGameSceneString));
    }
    public void NewGame()
    {
        ConfirmNewPanel.SetActive(true);
    }

    public void CancelContinue()
    {
        ConfirmContinuePanel.SetActive(false);
    }
    public void ContinueGame()
    {
        ConfirmNewPanel.SetActive(false);
        StartCoroutine(LoadScene(playerLastLoadedScene));
    }

    
    public void ContinueGameConfirmBox()
    {
        //confirmation box
        ConfirmContinuePanel.SetActive(true);
    }

    private IEnumerator LoadScene(string sceneString)
    {
        if (musicSrc != null)
        {
            musicSrc.Stop();
        }
        //load the scene
        Debug.Log("Begin Load");
        LoadingScreenSplash.SetActive(true);
        yield return null;
        async = SceneManager.LoadSceneAsync(sceneString,LoadSceneMode.Single);
        //SceneManager.LoadScene(playerLastLoadedScene);
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
        yield return null;
        Debug.Log("Finished Load Stack");
        //when scene is loaded, connect player to scene 
        //(Eventually need to check if we're connecting to a multiplayer game and so need to connect as client, not host)
        /// Do we need to use the connection manager for this?
        /// Not sure how the player will load in, or what objects will need disabled/enabled.
        /*if (!NetworkManager.Singleton.IsHost)
        {
            Debug.Log("No Host");
        }*/
        ///Start HOST in Scene
        ///
        //NetworkManager.Singleton.StartHost();
        //yield return null;
        //load player data
        //if (sceneString.CompareTo(NewGameSceneString) != 0)
        //{
        //    if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
        //    {
        //        GameObject player = networkedClient.PlayerObject.gameObject;
                //player.GetComponent<SupplementalController>().LoadPlayer();
        //    }
        //}
        //take down splash
        //LoadingScreenSplash.SetActive(false);
        yield return null;
    }

    public void ShowLoadPanel()
    {
        for(int i = SelectSceneContentParent.transform.childCount-1; i >= 0; i--)
        {
            //try
            //{
                Destroy(SelectSceneContentParent.transform.GetChild(i).gameObject);
            //}
            //catch (Exception e) { }
        }

        SelectScenePanel.SetActive(true);
        //fill the panel with saves
        PlayerData data = SerializationHandler.Load("Player_current");
        if (data != null)
        {
            //create a new button for every save that is found. For now there is only one.
            GameObject instanceButton = Instantiate(LoadGameButtonPrefabs, SelectSceneContentParent.transform);
            //instanceButton.transform.GetChild(0).GetComponent<RawImage>().texture = data.PlayerSceneData.GetLastSceneThumbnail();
            instanceButton.transform.GetChild(1).GetComponent<TMP_Text>().text = data.PlayerSceneData.CharacterName;
            instanceButton.transform.GetChild(2).GetComponent<TMP_Text>().text = data.PlayerSceneData.PlayerSceneString;
            instanceButton.transform.GetChild(3).GetComponent<TMP_Text>().text = data.PlayerSceneData.PlayerSceneSubLocation;
            instanceButton.transform.GetChild(4).GetComponent<TMP_Text>().text = data.PlayerSceneData.DateTime;
            //Clicking this button will allow us to load it.
            instanceButton.GetComponent<Button>().onClick.AddListener(delegate { selectedSceneToLoad = data.PlayerSceneData.PlayerSceneString; });
        }
    }

    public void CancelLoadSelectedScene()
    {
        selectedSceneToLoad = "";
        SelectScenePanel.SetActive(false);
    }

    public void LoadScene()
    {
        SelectScenePanel.SetActive(false);
        if (selectedSceneToLoad != "")
        {
            StartCoroutine(LoadScene(selectedSceneToLoad));
        }
        //SceneManager.LoadScene(selectedSceneToLoad);
    }

    public void ShowMulitplayerMenu()
    {

    }

    public void ShowNotesMenu()
    {
        NotesWindow.SetActive(true);

    }
    public void HideNotesMenu()
    {
        NotesWindow.SetActive(false);
    }

    public void ConfirmQuit()
    {
        
        if (Application.isEditor)
        {
            //EditorApplication.ExitPlaymode();
        }
        else
        {
            Application.Quit();
        }
    }

    public void CancelQuit()
    {
        ConfirmQuitPanel.SetActive(false);
    }
    public void QuitMenu()
    {
        ConfirmQuitPanel.SetActive(true);
    }

}
