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
                    if (obj.TryGetComponent(out VolumeAtmosphereController vc))
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
                for (int f = 0; f < roomParents.Length; f++)
                {
                    //roomParents[f].GetComponent<VolumeAtmosphereController>().Contamination = 1500f;
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
    }
}
        