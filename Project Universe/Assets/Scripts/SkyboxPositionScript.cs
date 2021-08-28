using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxPositionScript : MonoBehaviour
{
    public int scale = 10000;
    public GameObject HDRISkyboxObj;
    public GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        HDRISkyboxObj.transform.position = player.transform.position;
        HDRISkyboxObj.transform.localScale = new Vector3(scale, scale, scale);
    }

    // LateUpdate is called last, hence all camera transforms and whatnot will be done with. Depending on situation,
    //skybox might update before some other physics, player, or camera.
    void LateUpdate()
    {
        HDRISkyboxObj.transform.position = player.transform.position;
    }
}
