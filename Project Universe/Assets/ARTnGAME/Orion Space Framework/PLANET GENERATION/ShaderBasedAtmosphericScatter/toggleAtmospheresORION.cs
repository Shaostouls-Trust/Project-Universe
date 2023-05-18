using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class toggleAtmospheresORION : MonoBehaviour
{
    public Transform planet;
    public Transform planetSurfaceFog;
    public Transform planetInnerAtmosphere;
    public Transform planetOuterAtmosphere;
    public float toggleDistance = 1;
    public Transform player;
    public bool toggleFog = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(player == null)
        {
            player = Camera.main.transform;
        }

        if (Vector3.Distance(player.position, planet.position) > toggleDistance)
            {
                if (planetSurfaceFog.gameObject.activeInHierarchy && toggleFog)
                {
                    planetSurfaceFog.gameObject.SetActive(false);
                }
                if (planetInnerAtmosphere.gameObject.activeInHierarchy)
                {
                    planetInnerAtmosphere.gameObject.SetActive(false);
                }
                if (!planetOuterAtmosphere.gameObject.activeInHierarchy)
                {
                    planetOuterAtmosphere.gameObject.SetActive(true);
                }
            }
            else
            {
                if (!planetSurfaceFog.gameObject.activeInHierarchy && toggleFog)
                {
                    planetSurfaceFog.gameObject.SetActive(true);
                }
                if (!planetInnerAtmosphere.gameObject.activeInHierarchy)
                {
                    planetInnerAtmosphere.gameObject.SetActive(true);
                }
                if (planetOuterAtmosphere.gameObject.activeInHierarchy)
                {
                    planetOuterAtmosphere.gameObject.SetActive(false);
                }
            }
       
    }
}
