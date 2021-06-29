using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectUniverse.Environment.Volumes;

namespace ProjectUniverse.Environment.Fluid
{
    public class IFluidTank : MonoBehaviour
    {
        public int capacity_L;
        public float fluidLevel_L;
        public float flowRate_Ls;
        public bool valveState;
        [SerializeField] private VolumeAtmosphereController roomVolume;
        private IFluid fluid;
        private List<IFluid> fluids = new List<IFluid>();

        // Start is called before the first frame update
        void Start()
        {
            fluid = new IFluid("water", 70, fluidLevel_L);
            fluid.SetDensity(1000);
            fluid.SetTemp(60);
            fluid.SetLocalPressure(1f);
            fluid.SetLocalVolume(600);
        }

        // Update is called once per frame
        void Update()
        {
            //EXTREMELY TEMP!!
            if (valveState)
            {
                if (fluidLevel_L > 0)
                {
                    IFluid outFluid = fluid;
                    float redux = flowRate_Ls * Time.deltaTime;
                    if (fluidLevel_L - redux < 0)
                    {
                        redux = fluidLevel_L;
                    }
                    outFluid.SetConcentration(redux);
                    //Debug.Log("Redux: " + redux);
                    //Debug.Log("outfluid: " + outFluid.GetConcentration());
                    roomVolume.AddRoomFluid(outFluid);
                    fluidLevel_L -= redux;
                    fluid.SetConcentration(fluidLevel_L);
                }
            }
        }

        public void OnValueActivated(int mode)
        {
            switch (mode)
            {
                case 0:
                    valveState = !valveState;
                    break;
            }
        }
    }
}