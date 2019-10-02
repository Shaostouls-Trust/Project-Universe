using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.Rendering;
using TMPro;
public class UI3dItem : MonoBehaviour
{
    //Public Variables we will need to access.
    public ItemClass itemInfo;

    //On GUI stuff
    [SerializeField] private RawImage Icon;
    [SerializeField] private TextMeshProUGUI ItemName;
    [SerializeField] private TextMeshProUGUI ItemDesc;
    [SerializeField] private Image ForeGround;
    [SerializeField] private Image BackGround;
    [SerializeField] private RawImage InspectionViewPort;
    [SerializeField] private TextMeshProUGUI InspItemName;


    //Create RenderTexture Variables and Set Camera
    [SerializeField] private Camera renderCamera;
    [SerializeField] private GameObject Target;
    [SerializeField] private GameObject Model;
    [SerializeField] private bool selected = false;

    //Private Variables
    private CustomRenderTexture texture;
    private CustomRenderTexture viewPortTexture;
    private int frames = 0;




    // Start is called before the first frame update
    void Awake()
    {
        renderCamera.transform.parent = null;
        //texture = new RenderTexture(80, 80, 16, RenderTextureFormat.ARGB32);
        texture = new CustomRenderTexture(80, 80, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        texture.Create();
        texture.updateMode = CustomRenderTextureUpdateMode.OnDemand;

        viewPortTexture = new CustomRenderTexture(261, 382, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
        viewPortTexture.Create();
        texture.updateMode = CustomRenderTextureUpdateMode.OnDemand;
        ItemName.text = itemInfo.itemName;

        
        renderCamera.targetTexture = texture;
        Icon.texture = texture;
        renderCamera.transform.position =  new Vector3(Random.Range(10,-10), Random.Range(20,19), Random.Range(10,-10));
        ItemName.text = itemInfo.itemName;
    }
    private void Update()
    {
        if (selected)
        {
            InspectionViewPort.texture = viewPortTexture;
            UpdateRenderTexture();
            ForeGround.color = new Vector4(0, 255, 0, 255);
            InspItemName.text = itemInfo.itemName;
            ItemDesc.text = itemInfo.itemDescription;
        }
        else if  (!selected)
        {
            ForeGround.color = Color.white;
            InspItemName.text = "No Item Selected";
            ItemDesc.text = "No Item Description for 'No Item Selected'";
        }
    }
    public void ItemSelected()
    {
        selected = !selected;
    }
    void UpdateModel(GameObject model)
    {
        //Instantiate(model, Target.transform);
        //Model = model;
       //Model.transform.Rotate(-90, 0, 0);
    }

    // Update is called once per frame
    void UpdateRenderTexture()
    {
        
        renderCamera.enabled = true;
        //renderCamera.transform.LookAt(Target.transform);
        Target.transform.Rotate(0, 1, 0);
        texture.Update();
        viewPortTexture.Update();
    }
}
