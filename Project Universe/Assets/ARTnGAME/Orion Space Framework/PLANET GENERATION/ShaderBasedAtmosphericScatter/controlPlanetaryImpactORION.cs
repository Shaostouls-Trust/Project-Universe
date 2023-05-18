using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class controlPlanetaryImpactORION : MonoBehaviour
{
    public Transform impactBody;
    public Material planetMaterial;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(impactBody != null && planetMaterial != null)
        {
            //pass position to shader
            Vector4 getProps = planetMaterial.GetVector("_ImpactObjectPos");
            planetMaterial.SetVector("_ImpactObjectPos", new Vector4(impactBody.position.x, impactBody.position.y, impactBody.position.z, getProps.w) );
        }
    }
}
