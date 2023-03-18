using ProjectUniverse.Environment.Volumes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.Ship
{
    /// <summary>
    /// Control the render states of all volumes in the ship.
    /// </summary>
    public class RenderStateManager : MonoBehaviour
    {
        [SerializeField] private GameObject[] roomParents;
        private GameObject playerRoom;

        // Start is called before the first frame update
        void Start()
        {
            foreach(GameObject obj in roomParents)
            {
                if(obj.TryGetComponent(out VolumeAtmosphereController vac))
                {
                    vac.HideRenderVolume();
                }

            }
        }

        public GameObject CurrentRoom
        {
            get { return playerRoom; }
            set { playerRoom = value; }
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
                    if(obj.TryGetComponent(out VolumeNode vn))
                    {
                        if (vn.VolumeLink != null)
                        {
                            vacs.Add(vn.VolumeLink.GetComponent<VolumeAtmosphereController>());
                        }
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
        }
    }
}