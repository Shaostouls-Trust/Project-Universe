using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RenderCamera : MonoBehaviour
{
    [SerializeField] private Camera[] cameras;
    [SerializeField] private GameObject cameraPrefab;
    GameObject CameraBin;
    [SerializeField] private GameObject[] renderObjects;
    [SerializeField] private RawImage[] renderIcons;
    [SerializeField] private Texture2D[] renderIconsTex;
    [SerializeField] private CustomRenderTexture[] renderTextures;
    [SerializeField] private GameObject targetParent;
    [SerializeField] private GameObject[] targets;
    GameObject TargetBin;
    [SerializeField] private int inventorySpace;
    [SerializeField] private CustomRenderTexture[] targetTex;
    public  int poop = 0;
    private int a = 0;
    private int c;
    [SerializeField] private bool processed = false;
    private bool targetsCalculated = false;
    [SerializeField] private bool texturesProcessed = false;
    private Texture2D image;
    // Start is called before the first frame update
    void Start()
    {
        // targetTex = new CustomRenderTexture[1];
        //targetTex[1] = new CustomRenderTexture(80, 80,RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        //camera[] = Instantiate(new Camera(), transform );
        //targetTex[] = new CustomRenderTexture(80, 80, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        CameraBin = Instantiate(new GameObject(), transform);
        CameraBin.name = "Camera Bin";
        TargetBin = Instantiate(new GameObject());
        TargetBin.name = "Target Bin";
    }
    void CreateTextures(int count) {

    }

    void CreateCameras(int count)
    {

        //Create Render Textures;
        targetTex = new CustomRenderTexture[count];
        if (texturesProcessed == false)
        {
            for (int i = 0; i < count; i++)
            {
                targetTex[i] = new CustomRenderTexture(80, 80, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                targetTex[i].Create();
                targetTex[i].name = "RenderingTexture:" + i;
            }
            texturesProcessed = true;
        }
        else { }
        //Destroy old cameras and targets
        if (texturesProcessed == true)
        {
            foreach (Transform child in CameraBin.transform)
            {
                Destroy(child.gameObject);
                cameras = null;
            }
            foreach (Transform child in TargetBin.transform)
            {
                Destroy(child.gameObject);
                targets = null;
                targetsCalculated = false;
            }
        }

        //Generate Target points =========================================================== START HERE ====================================
        targets = new GameObject[count];
        for (int i = 0; i < count; i++)
        {
            targets[i] = Instantiate(new GameObject(), TargetBin.transform);
            targets[i].name = "Target Point:" + i;
            targets[i].transform.position = targetParent.transform.position + new Vector3(0, 0, i);
        }

        //Create cameras
        cameras = new Camera[count];
        for (int i = 0; i < count; i++)
        {
            GameObject cam = Instantiate(cameraPrefab, CameraBin.transform);
            cameras[i] = cam.GetComponent<Camera>();
            cameras[i].name = "IconRenderingCamera:" + i;
            cameras[i].targetTexture = targetTex[i];
            Debug.Log("wtf");
        }

    }
    void ProcessGameObjects()
    {
        if (processed == false)
        {
            c = 0;
            a = 0;
            processed = true;
            inventorySpace = targetParent.transform.childCount;
            renderObjects = new GameObject[inventorySpace];
            renderTextures = new CustomRenderTexture[inventorySpace];
            renderIcons = new RawImage[inventorySpace];
            renderIconsTex = new Texture2D[inventorySpace];
            //camera.targetTexture[] = targetTex;
            foreach (Transform child in targetParent.transform)
            {
                Debug.Log(a + "___before a++");
                renderObjects[a] = child.gameObject;
                //renderObjects[a].transform.parent = targets[c].transform;
                renderIcons[a] = child.GetComponent<ItemClass>().icon;
                renderTextures[a] = new CustomRenderTexture(512, 512, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
                renderTextures[a].Create();
                renderTextures[a].name = renderObjects[a].name + "RenderText";
                child.gameObject.layer = 14;
                if (a % 5 == 0)
                {
                    c++;
                }
                else { }
                Debug.Log(a + "___after a++");
                //SetParent and position
                a++;
            }
            CreateCameras(c);
        }
        else { return; }
    }
    // Update is called once per frame
    int count = 0;
    float rotation_x = 0;
    void Update()
    {
        rotation_x += 180;
        ProcessGameObjects();
        //StartCoroutine(ProcessTex(count));
        //camera.targetTexture = renderTextures[count];
        ProcessTex(count);

    }
    private void HideObjects()
    {
        foreach (GameObject g in renderObjects)
            g.layer = 14;
    }
    private void ProcessTex(int i = 0)
    {
        HideObjects();
        renderObjects[i].layer = 13;
        //renderIcons[i].texture = renderIconsTex[i];
        renderIcons[i].texture = renderTextures[i];
        //RenderTexture.active = renderTextures[i];
        renderObjects[i].gameObject.transform.Rotate(0, 0, 1);
        renderTextures[i].Update();

        count++;
        i++;
        if (i >= renderObjects.Length)
        {
            i = 0;
            count = 0;
        }
    }
}

