using Unity.Netcode;
using ProjectUniverse.Environment.Volumes;
using ProjectUniverse.Player.PlayerController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using ProjectUniverse.PowerSystem;

namespace ProjectUniverse.Ship
{
    /// <summary>
    /// Control the render states of all volumes in the ship.
    /// </summary>
    public class RenderStateManager : MonoBehaviour
    {
        [SerializeField] private GameObject[] roomParents;
        private GameObject playerRoom;
        private SupplementalController playerSC;
        public int planeStateTesterTimer = 10;//10 frames
        public int psttremaining = 10;
        [Tooltip("Controls the active state of the listed external ship objects.")]
        [SerializeField] private List<GameObject> externalGameObjects;
        //is the player's perspective outside the ship?
        [SerializeField] private bool controllerIsExternal;
        private bool stahp = false;
        //[SerializeField] private Volume volume;
        [SerializeField] private VolumeProfile oxygenatedVP;

        // Start is called before the first frame update
        void Start()
        {
            //oxygenatedVP = volume.sharedProfile;
            //reset the fog effect
            if (oxygenatedVP.TryGet<Fog>(out var fog))
            {
                fog.enabled.overrideState = true;
                fog.enabled.value = false;
            }
            //reset lights
            AllLightsWhite();

            psttremaining = planeStateTesterTimer;
            foreach (GameObject obj in roomParents)
            {
                if (obj.TryGetComponent(out VolumeAtmosphereController vac))
                {
                    vac.HideRenderVolume();
                }
            }
        }

        public GameObject[] Rooms
        {
            get { return roomParents; }
        }

        public GameObject CurrentRoom
        {
            get { return playerRoom; }
            set { playerRoom = value; }
        }

        public bool ExternalControllerState
        {
            set { controllerIsExternal = value; }
            get { return controllerIsExternal; }
        }

        private void Update()
        {
            if (CurrentRoom != null)
            {
                VolumeAtmosphereController crvac = CurrentRoom.GetComponent<VolumeAtmosphereController>();
                //if (!crvac.RenderEnabled)
                //{
                //    crvac.ShowRenderVolume();
                //}
                List<VolumeAtmosphereController> vacs = new List<VolumeAtmosphereController>();
                foreach (GameObject obj in crvac.GetNeighborEmpties)
                {
                    //add volume to neighbor list
                    if (obj.TryGetComponent(out VolumeNode vn))
                    {
                        if (vn.VolumeLink != null)
                        {
                            if (vn.DoorState())//only render the nearby volumes whose doors are open (or are closing)
                            {
                                vacs.Add(vn.VolumeLink.GetComponent<VolumeAtmosphereController>());
                            }
                        }
                    }
                }
                //These volumes will render regardless of door state.
                foreach (GameObject obj in crvac.AdditionalRenderVolumes)
                { 
                    if(obj.TryGetComponent(out VolumeAtmosphereController vc))
                    {
                        vacs.Add(vc);
                    }
                }
                for (int r = 0; r < roomParents.Length; r++)
                {
                    //if roomparent vac is in vacs, keep it displayed.
                    //otherwise, if it's enabled, hide it
                    VolumeAtmosphereController vac = roomParents[r].GetComponent<VolumeAtmosphereController>();
                    if (vacs.Contains(vac) || vac == crvac)
                    {
                        if (!vac.RenderEnabled)
                        {
                            vac.ShowRenderVolume();
                        }
                    }
                    else
                    {
                        if (vac.RenderEnabled)
                        {
                            vac.HideRenderVolume();
                        }
                    }
                }
            }
            else
            {
                for (int r = 0; r < roomParents.Length; r++)
                {
                    VolumeAtmosphereController vac = roomParents[r].GetComponent<VolumeAtmosphereController>();
                    if (vac.RenderEnabled)
                    {
                        vac.HideRenderVolume();
                    }

                }
            }

            if (controllerIsExternal)
            {
                if (!stahp)
                {
                    Debug.Log("hide all");
                    for (int r = 0; r < roomParents.Length; r++)
                    {
                        VolumeAtmosphereController vac = roomParents[r].GetComponent<VolumeAtmosphereController>();
                        if (vac.RenderEnabled)
                        {
                            vac.HideRenderVolume();
                        }

                    }
                    Debug.Log("show Extern");
                    for (int e = 0; e < externalGameObjects.Count; e++)
                    {
                        if (!externalGameObjects[e].activeInHierarchy)
                        {
                            externalGameObjects[e].SetActive(true);
                        }
                    }
                }
                stahp = true;
            }
            else
            {
                stahp = false;
                //Debug.Log("Hide extern");
                for (int e = 0; e < externalGameObjects.Count; e++)
                {
                    if (externalGameObjects[e].activeInHierarchy)
                    {
                        externalGameObjects[e].SetActive(false);
                    }
                }
            }
        }

        public void AllLightsRed()
        {
            Debug.Log("ALL LIGHTS RED");
            for(int i = 0; i < roomParents.Length; i++)
            {
                VolumeAtmosphereController vac = roomParents[i].GetComponent<VolumeAtmosphereController>();
                for (int j = 0; j < vac.LightGameObjects.Length; j++)
                {
                    if (vac.LightGameObjects[j].TryGetComponent(out Light light))
                    {
                        //if parent has submachine
                        if(vac.LightGameObjects[j].GetComponentInParent<ISubMachine>(true) != null)
                        {
                            light.color = Color.red;
                        }
                    }
                }
            }
        }

        public void AllLightsWhite()
        {
            Debug.Log("ALL LIGHTS WHITE");
            for (int i = 0; i < roomParents.Length; i++)
            {
                VolumeAtmosphereController vac = roomParents[i].GetComponent<VolumeAtmosphereController>();
                for (int j = 0; j < vac.LightGameObjects.Length; j++)
                {
                    if (vac.LightGameObjects[j].TryGetComponent(out Light light))
                    {
                        //if parent has submachine
                        if (vac.LightGameObjects[j].GetComponentInParent<ISubMachine>(true) != null)
                        {
                            light.color = Color.white;
                        }
                    }
                }
            }
        }

        public void AllVolumeEffect(int i)
        {
            if (i == 1)
            {
                //contamination
                for(int f = 0; f < roomParents.Length; f++)
                {
                    roomParents[f].GetComponent<VolumeAtmosphereController>().Contamination = 1500f;
                }
                //fog (thickening) to oxygenated vp (method?)
                //StartCoroutine(VolumeFogThickenOverTime(60f, 1f, 30f));
                if (oxygenatedVP.TryGet<Fog>(out var fog))
                {
                    fog.enabled.overrideState = true;
                    fog.enabled.value = true;
                }
            }
        }

        /// <summary>
        /// log interp between min and max over time seconds.
        /// </summary>
        /// <param name="time"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public IEnumerator VolumeFogThickenOverTime(float time, float min, float max)
        {
            float current = min;
            if (oxygenatedVP.TryGet<Fog>(out var fog))
            {
                fog.enabled.overrideState = true;
                fog.enabled.value = true;
            }
            yield return new WaitForEndOfFrame();
            //attenuation distance is fog intensity, which works logarithmically
            
            while (current < max)
            {
                current += (max - min) / time;
                yield return new WaitForEndOfFrame();
            }
        }

        //collect objects for raycast visiblity testing
        /// <summary>
        /// Bugged, and I've run out of patience to get it to work for now.
        /// </summary>
        /*
        public void FixedUpdate()
        {
            psttremaining--;
            if (psttremaining == 0)
            {
                psttremaining = planeStateTesterTimer;
                if (playerSC != null)
                {
                    //grab the player camera position (worldspace)
                    Vector3 camPosWorld = playerSC.ActiveCamera.transform.position;
                    //raycast
                    for (int r = 0; r < roomParents.Length; r++)
                    {
                        VolumeAtmosphereController vac = roomParents[r].GetComponent<VolumeAtmosphereController>();
                        //if room is rendering and the player is not in it
                        if (vac.RenderEnabled && vac != CurrentRoom.GetComponent<VolumeAtmosphereController>())
                        {
                            for (int j = 0; j < vac.GetFrustrumStates.Length; j++)
                            {
                                if (vac.GetFrustrumStates[j] != null)
                                {
                                    if (vac.GetFrustrumStates[j].visibleInFrustrum)//bug from OnBecomeInvisible
                                    {
                                        MeshCollider[] mcrsp = vac.GetFrustrumStates[j].RenderStatePlanes;
                                        //GameObject room = vac.GetFrustrumStates[j].gameObject;
                                        StartCoroutine(RenderStateRayCastMachine(camPosWorld, mcrsp, vac));
                                    }
                                }
                                else
                                {
                                    Debug.Log("Warning: null at " + vac);
                                }
                            }
                        }
                    }

                }
                else //attempt to find the player
                {
                    if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
                    {
                        playerSC = networkedClient.PlayerObject.GetComponent<SupplementalController>();
                    }
                }
            }
        }
        */

        /// <summary>
        /// Raycast from the camera to every center and 4 corner of the frustrum planes. If the rays do not hit
        /// a frustrum plane first (that is, before the plane of an external volume), then render that section.
        /// Take the number of frustrum planes in a frustrum state * 5, then split those rays over ten frames.
        /// If a door is hit, that ray's state depends on the state of the door.
        /// Repeat after ten frames.
        /// Eventually convert to jobs.
        /// </summary>
        /// <returns></returns>
        private IEnumerator RenderStateRayCastMachine(Vector3 camPosWorld, MeshCollider[] renderStatePlanes, VolumeAtmosphereController vac) 
        { 
            //number of runs required for this frame set
            //int countmax = renderStatePlanes.Length * 5;
            //NOTE: intdiv (in effect) rounds down to nearest whole, so to complete the stack in 10 frames or less,
            //add one to the number of itterations per frame.
            int ittsLimit = ((renderStatePlanes.Length) / planeStateTesterTimer)+1;
            int totalItterations = 0;
            int frameItts = 0;
            bool complete = false;
            //Debug.Log("====");
            //Debug.Log(renderStatePlanes.Length);
            //list of the render state planes that should be visible
            List<MeshCollider> stateRenderersTrue = new List<MeshCollider>();
            while (totalItterations < renderStatePlanes.Length)// && !complete
            {
                ///
                /// The raycasts are not properly detecting the planes they should be, despite making contact.
                ///
                frameItts++;
                totalItterations++;
                if (frameItts >= ittsLimit)
                {
                    yield return new WaitForFixedUpdate();
                    frameItts = 0;
                }

                //raycast to the center and edges of the plane in question
                //get a vector pointing to the center of the plane from the campos
                // V3b - V3a = vector from a to b
                LayerMask mask = LayerMask.GetMask("Bounding");
                //render state planes is the array of the planes defining the adjacent volume being looked at
                Vector3 pointing = renderStatePlanes[totalItterations - 1].transform.position - camPosWorld;
                RaycastHit[] hitsCenter = Physics.RaycastAll(camPosWorld, pointing.normalized, pointing.magnitude+0.01f, mask, QueryTriggerInteraction.Ignore);
                Debug.DrawRay(camPosWorld, pointing, Color.red, 0.5f);
                //the order (index) is everything here. If we hit a door or local COLplane first, we must assume* that the
                //target plane is occluded. Other itts will test this hypothesis. Local planes will be identified by them
                //not being in the renderStatePlanes array
                for(int i = 0; i< hitsCenter.Length; i++)
                {
                    bool contains = false;
                    //Debug.Log("plane "+ hitsCenter[i].transform.localPosition);
                    //basically a .Contains() loop
                    foreach (MeshCollider mc in renderStatePlanes)
                    {
                        //Debug.Log("mc "+mc.transform.gameObject + " " + mc.transform.localPosition);
                        //if true, then this is just another part of the target volume
                        if ((hitsCenter[i].collider as MeshCollider) == mc)
                        {
                            contains = true;
                            Debug.Log(hitsCenter[i].transform.gameObject.name +" is of target volume");
                            //break;
                        }
                        //if the plane we hit was a door plane, not an adjacent plane
                        //if hit door, (for now) assume door is open, therefore render what's past it
                        if (hitsCenter[i].transform.gameObject.tag == "Door")
                        {
                            contains = true;//occlusion depends on the state of the door
                            //Debug.Log(hitsCenter[i].transform.gameObject.name + " is door");
                        }
                    }
                    if (contains)
                    {
                        if(hitsCenter[i].transform.gameObject.tag == "Door")
                        {
                            //Debug.Log("c1 door");
                            stateRenderersTrue.Add(hitsCenter[i].transform.GetComponent<MeshCollider>());
                        }
                        else 
                        {
                            //Debug.Log("c1 wall");
                            stateRenderersTrue.Add(hitsCenter[i].transform.GetComponent<MeshCollider>());
                        }
                    }
                    //else //The COLplane was from a different volume, so do not render
                }
                ///NOTE: if the above center tests returned visible, there's no need to continue for that specific plane.
                
                /*
                //the colplanes have 4 verticies
                Vector3[] meshVerts = renderStatePlanes[totalItterations - 1].sharedMesh.vertices;

                Vector3 v0pos = renderStatePlanes[totalItterations - 1].transform.TransformPoint(meshVerts[0]);
                Vector3 pointingV0 = v0pos - camPosWorld;
                //Debug.Log("Transform plane corner pos to: " + v0pos);

                Vector3 v1pos = renderStatePlanes[totalItterations - 1].transform.TransformPoint(meshVerts[1]);
                Vector3 pointingV1 = v1pos - camPosWorld;

                Vector3 v2pos = renderStatePlanes[totalItterations - 1].transform.TransformPoint(meshVerts[2]);
                Vector3 pointingV2 = v2pos - camPosWorld;

                Vector3 v3pos = renderStatePlanes[totalItterations - 1].transform.TransformPoint(meshVerts[3]);
                Vector3 pointingV3 = v3pos - camPosWorld;

                RaycastHit[] hitsCorners0 = Physics.RaycastAll(camPosWorld, pointingV0.normalized, pointingV0.magnitude, mask, QueryTriggerInteraction.Ignore);
                RaycastHit[] hitsCorners1 = Physics.RaycastAll(camPosWorld, pointingV1.normalized, pointingV1.magnitude, mask, QueryTriggerInteraction.Ignore);
                RaycastHit[] hitsCorners2 = Physics.RaycastAll(camPosWorld, pointingV2.normalized, pointingV2.magnitude, mask, QueryTriggerInteraction.Ignore);
                RaycastHit[] hitsCorners3 = Physics.RaycastAll(camPosWorld, pointingV3.normalized, pointingV3.magnitude, mask, QueryTriggerInteraction.Ignore);
                //combine hitlists
                List<RaycastHit> hitsCorners = new List<RaycastHit>();
                hitsCorners.AddRange(hitsCorners0);
                hitsCorners.AddRange(hitsCorners1);
                hitsCorners.AddRange(hitsCorners2);
                hitsCorners.AddRange(hitsCorners3);
                Debug.DrawRay(camPosWorld, pointingV0, Color.blue, 0.5f);
                Debug.DrawRay(camPosWorld, pointingV1, Color.yellow, 0.5f);
                Debug.DrawRay(camPosWorld, pointingV2, Color.green, 0.5f);
                Debug.DrawRay(camPosWorld, pointingV3, Color.cyan, 0.5f);

                //finally test the hitscans
                for (int i = 0; i < hitsCorners.Count; i++)
                {
                    bool contains = false;
                    //Debug.Log("plane " + hitsCorners[i].transform.localPosition);
                    foreach (MeshCollider mc in renderStatePlanes)
                    {
                        if ((hitsCorners[i].collider as MeshCollider) == mc)
                        {
                            contains = true;
                            //Debug.Log("plane is not external");
                        }
                        //if hit door, (for now) assume door is open, therefore render what's past it
                        else if (hitsCorners[i].transform.gameObject.tag == "Door")
                        {
                            contains = true;//occlusion depends on the state of the door
                        }
                    }
                    if (contains)
                    {
                        //if hit door, (for now) assume door is open
                        if (hitsCorners[i].transform.gameObject.tag == "Door")
                        {
                            //complete = true;
                            //Debug.Log("c2 door");
                            stateRenderersTrue.Add(hitsCorners[i].transform.GetComponent<MeshCollider>());
                            //break;
                        }
                        else
                        {
                            //Debug.Log("c2 wall");
                            stateRenderersTrue.Add(hitsCorners[i].transform.GetComponent<MeshCollider>());
                        }
                    }
                }*/
            }
            //send the state machine data to the VAC
            vac.ReceiveActiveFrustrumPlanes(stateRenderersTrue);
        }
    }
}