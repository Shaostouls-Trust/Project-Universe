using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LODfixer : MonoBehaviour
{
    public bool run;

    // Update is called once per frame
    void Update()
    {
        if (run)
        {
            run = false;
            LODGroup[] lodObjs = transform.GetComponentsInChildren<LODGroup>();
            //for each pipe
            for (int j = 0; j < lodObjs.Length; j++)
            {
                List<MeshRenderer> renders = new List<MeshRenderer>();
                foreach (Transform lod in lodObjs[j].transform)//each lod level
                {
                    renders.Add(lod.GetComponent<MeshRenderer>());
                }
                //assign to LOD levels
                LOD[] oldLods = lodObjs[j].GetLODs();
                for(int l = 0; l < oldLods.Length;l++)
                {
                    oldLods[l].renderers = new MeshRenderer[] { renders[l] };
                }
                lodObjs[j].SetLODs(oldLods);
            }
        }
    }
}
