using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Purpose: Control behavior between GlobalVolume and LocalVolume Atmospheres.
/// Method: Control the update cycle for volume pointers, and pointer list management
/// Grab all GlobalAtmospheres in scene, and all LocalAtmospheres.
/// Check all LocalAtmosphere lists against all GlobalAtmosphere lists.
/// Set/refreash the links and disable the colliders and rigidbodies.
/// </summary>
namespace ProjectUniverse.Environment.Volumes
{
    public class GlobalVolumeInteractController : MonoBehaviour
    {
        private VolumeAtmosphereController[] VACs;
        private VolumeGlobalAtmosphereController[] VGACs;
        private int cycleNumber = 0;
        private bool clear = false;
        //private bool collidercycle = false;
        //private bool updatecycle = false;

        void Start()
        {
            InvokeRepeating("CoroutineEncap", 0.0f, 3.0f);
        }

        public void CoroutineEncap()
        {
            StartCoroutine(InstanceCollector());
            //Debug.Log("Cycle: "+cycleNumber++);
        }

        IEnumerator InstanceCollector()
        {
            clear = false;
            //get all script instances
            //This will be from the children of this script's empty,
            VGACs = this.gameObject.GetComponentsInChildren<VolumeGlobalAtmosphereController>();
            yield return null;

            //and from any "ShipHead" children.
            GameObject[] shipHeads = GameObject.FindGameObjectsWithTag("ShipHead");
            foreach (GameObject obj in shipHeads)
            {
                VACs = obj.GetComponentsInChildren<VolumeAtmosphereController>();
            }
            //clear was here
            yield return null;

            if (VACs != null && VGACs != null)
            {
                //Debug.Log("Internal Reset and Colldier Cycle On");
                for (int j = 0; j < VGACs.Length; j++)
                {
                    for (int i = 0; i < VACs.Length; i++)
                    {
                        foreach (GameObject node in VACs[i].GetConnectedNeighbors())
                        {
                            //only reset the unlinked volumes
                            if (node.GetComponent<VolumeNode>().LinkByImpact)
                            {
                                node.GetComponent<Rigidbody>().detectCollisions = true;
                                node.GetComponent<BoxCollider>().enabled = true;
                            }
                        }
                        //functionally wipe the neighbormap
                        //VACs[i].SetConnectedNeighbors(new List<GameObject>());
                    }
                    VGACs[j].SetExposedNodes(new List<GameObject>());
                }
            }
            clear = true;
            StopCoroutine("InstanceCollector");
        }

        void Update()
        {
            //IE if the most recent coroutine run is completed
            if (clear)
            {
                //Debug.Log("V(G)AC Cleaning and Colldier Cycle Off");
                clear = false;
                if (VACs != null && VGACs != null)
                {
                    //Check if a VAC node is in a VGAC nodelist
                    for (int j = 0; j < VGACs.Length; j++)
                    {
                        for (int i = 0; i < VACs.Length; i++)
                        {
                            foreach (GameObject node in VACs[i].GetConnectedNeighbors())
                            {
                                if (VGACs[j].GetExposedNodes().Contains(node))
                                {
                                    //if it is in the node list, remove it from the globals list.
                                    //this way only the nodes that are actually exposed to space will be in the VGAC list.
                                    VGACs[j].NullifyNodeLink(VGACs[j].GetExposedNodes());
                                    List<GameObject> temp = VGACs[j].GetExposedNodes();
                                    temp.Remove(node);
                                    VGACs[j].SetExposedNodes(temp);
                                }
                                node.GetComponent<Rigidbody>().detectCollisions = false;
                                node.GetComponent<BoxCollider>().enabled = false;
                            }
                        }
                    }
                }

            }
        }
    }
}