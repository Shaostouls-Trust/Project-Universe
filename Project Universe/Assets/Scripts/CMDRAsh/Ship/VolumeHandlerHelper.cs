using ProjectUniverse.Animation.Controllers;
using ProjectUniverse.Environment.Gas;
using ProjectUniverse.Environment.Volumes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class VolumeHandlerHelper : MonoBehaviour
{
    public bool generate = false;
    public bool generateDuctLink = false;
    public bool repositionNodes = false;
    public GameObject NeighborVolumeNodePrefab;
    public GameObject GasPipeSectionPrefab;
    public GameObject NeighborVolumeDuctPrefab;
    public GameObject[] ThisVolumeDoors;

    // Update is called once per frame
    void Update()
    {
        if (generate)
        {
            generate = !generate;
            for (int i = 0; i < ThisVolumeDoors.Length; i++)
            {
                //Place NVNs by offseting prefab from door by (-1.0, 2.25, 0.5) from each door origin
                GameObject nvn = Instantiate(NeighborVolumeNodePrefab, ThisVolumeDoors[i].transform.position, Quaternion.Euler(0f, 0f, 0f), transform);
                nvn.transform.SetParent(ThisVolumeDoors[i].transform);
                nvn.transform.localPosition = new Vector3(-0.5f, 2.25f, -1f);
                nvn.transform.SetParent(transform);
                nvn.GetComponent<VolumeNode>().SetDoor(ThisVolumeDoors[i]);
                //raycast from door to find neighbor volume. Set as linked volume. Current Door is OriginDoor

                GameObject door = ThisVolumeDoors[i];
                Vector3 back = door.transform.TransformDirection(Vector3.back);
                RaycastHit[] hits;
                hits = Physics.RaycastAll(new Vector3(door.transform.position.x,
                    door.transform.position.y + 0.025f, door.transform.position.z), back, 1.0f);
                foreach (RaycastHit hit in hits)
                {
                    //Debug.Log("Found " + hit.transform);
                    if(hit.collider.TryGetComponent(out VolumeAtmosphereController vac))
                    {
                        nvn.GetComponent<VolumeNode>().SetVolumeLink(hit.collider.gameObject);
                    }
                }
            }
        }

        //for every door, create a NVN Duct linker offset (-.5, 2.65, 0.5) from door origin.
        //raycast a 1m line downwards at 60-30 and check for IGasPipe hit. Hit becomes parent duct.
        if (generateDuctLink)
        {
            generateDuctLink = !generateDuctLink;
            for (int i = 0; i < ThisVolumeDoors.Length; i++)
            {
                Vector3 pos = ThisVolumeDoors[i].transform.position;// + new Vector3(0.5f, 2.65f, 0.5f);
                GameObject nvnd = Instantiate(NeighborVolumeDuctPrefab, pos, Quaternion.Euler(0f, 0f, 0f), transform);
                nvnd.transform.SetParent(ThisVolumeDoors[i].transform);
                nvnd.transform.localPosition = new Vector3(-0.5f, 2.65f, -0.5f);
                nvnd.transform.SetParent(transform);
                
                GameObject door = ThisVolumeDoors[i];
                //raycast in 4 directions. Compare dist of hits to door, with the closest hit being that door's duct.
                Vector3 dir1 = door.transform.TransformDirection(new Vector3(0f, -1f, 1f));
                Vector3 dir2 = door.transform.TransformDirection(new Vector3(0f, -1f, -1f));
                Vector3 dir3 = door.transform.TransformDirection(new Vector3(1f, -1f, 0f));
                Vector3 dir4 = door.transform.TransformDirection(new Vector3(-1f, -1f, 0f));
                RaycastHit[] hits;
                List<GameObject> pipehits = new List<GameObject>();
                hits = Physics.RaycastAll(nvnd.transform.position, dir1, 1.0f);
                foreach (RaycastHit hit in hits)
                {
                    //Debug.Log("1 Found " + hit.transform);
                    if (hit.collider.TryGetComponent(out IGasPipe pipe))
                    {
                        //nvnd.GetComponent<IGasPipeLinker>().SetParentDuct(pipe);
                        pipehits.Add(pipe.gameObject);
                    }
                }
                hits = Physics.RaycastAll(nvnd.transform.position, dir2, 1.0f);
                foreach (RaycastHit hit in hits)
                {
                    //Debug.Log("2 Found " + hit.transform);
                    if (hit.collider.TryGetComponent(out IGasPipe pipe))
                    {
                        pipehits.Add(pipe.gameObject);
                    }
                }
                hits = Physics.RaycastAll(nvnd.transform.position, dir3, 1.0f);
                foreach (RaycastHit hit in hits)
                {
                    //Debug.Log("3 Found " + hit.transform);
                    if (hit.collider.TryGetComponent(out IGasPipe pipe))
                    {
                        pipehits.Add(pipe.gameObject);
                    }
                }
                hits = Physics.RaycastAll(nvnd.transform.position, dir4, 1.0f);
                foreach (RaycastHit hit in hits)
                {
                    //Debug.Log("4 Found " + hit.transform);
                    if (hit.collider.TryGetComponent(out IGasPipe pipe))
                    {
                        pipehits.Add(pipe.gameObject);
                    }
                }
                float dist = 999f;
                GameObject closest = null;
                for(int j = 0; j < pipehits.Count; j++)
                {
                    float tdis = Vector3.Distance(ThisVolumeDoors[i].transform.position, pipehits[j].transform.position);
                    if(tdis < dist)
                    {
                        dist = tdis;
                        closest = pipehits[j];
                    }
                }
                if (closest != null)
                {
                    nvnd.GetComponent<IGasPipeLinker>().SetParentDuct(closest.GetComponent<IGasPipe>());
                }
                //also create a GasPipeSection Duct for every door, assigning nothing.
                //GameObject gpsd = Instantiate(GasPipeSectionPrefab,transform,false);
                //gpsd.transform.localPosition = new Vector3(0f,0f,0f);
            }
        }

        if (repositionNodes)
        {
            repositionNodes = false;
            //get all nodes
            VolumeNode[] vns = GetComponentsInChildren<VolumeNode>();
            for(int i = 0; i < vns.Length; i++)
            {
                GameObject door = ThisVolumeDoors[i];
                Vector3 doorPos = door.transform.localPosition;
                vns[i].transform.SetParent(door.transform);
                vns[i].transform.localPosition = new Vector3(-0.5f, 2.25f, -1f);
                vns[i].transform.SetParent(transform);
            }
        }
    }
}
