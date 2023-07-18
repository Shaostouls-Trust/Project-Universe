using ProjectUniverse.Environment.Fluid;
using ProjectUniverse.Environment.Gas;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.PowerSystem.Nuclear
{
    /// <summary>
    /// A condenser takes lo pressure steam and turns it back into water at 300K.
    /// The preheated water is cycled back through the steam generator.
    /// The heat from the lo pressure steam is used to run heat engines, which offset the energy
    /// cost of the coolers.
    /// </summary>
    public class SteamCondenser : MonoBehaviour
    {
        [SerializeField] private float steamTemp;
        [SerializeField] private float steamPressure;
        [SerializeField] private float steamRate;
        [SerializeField] private float steamAmount;
        public float waterAmount;
        [SerializeField] private float waterTemp;
        [SerializeField] private IGasPipe radiatorEnd;
        [SerializeField] private IFluidPipe reservoirPipe;
        [SerializeField] private SteamGenerator steamGen;
        [Tooltip("Pressure level at which to pump out water.")]
        [SerializeField] private float pressurizerLevel = 200f;
        [SerializeField] private float pressurizerVelocity = 120f;//Adjust by flow rate?
        private List<IGas> steam;
        private IFluid waterStored;
        private IFluid waterOut;
        //private float condensationRatio = 0.25f;//low pressure steam to water

        public float SteamTemp
        {
            get { return steamTemp; }
            set { steamTemp = value; }
        }
        public float SteamPressure
        {
            get { return steamPressure; }
            set { steamPressure = value; }
        }
        public float SteamRate
        {
            get { return steamRate; }
            set { steamRate = value; }
        }
        public float SteamAmount
        {
            get { return steamAmount; }
            set { steamAmount = value; }
        }
        //public float WaterAmount
        //{
        //    get { return waterAmount; }
        //    set { waterAmount = value; }
        //}
        public float WaterTemp
        {
            get { return waterTemp; }
            set { waterTemp = value; }
        }


        // Start is called before the first frame update
        void Start()
        {
            waterStored = new IFluid("water", 80f, 0f, 10.0f);//m^3
            waterOut = new IFluid("water", 80f, 0f, 10.0f);
            waterStored.SetDensity(1.0f);
            waterOut.SetDensity(1.0f);
        }
        
        // Update is called once per frame
        void Update()
        {
            if (radiatorEnd != null && (waterStored.GetConcentration() < 10f))//just != null
            {
                steam = radiatorEnd.ExtractGasses(-1f);
            }
            steamRate = 0f;
            float steamTtot = 0f;
            if (steam.Count > 0)
            {
                for (int i = 0; i < steam.Count; i++)
                {
                    steamTtot += steam[i].GetTemp();
                }
                SteamTemp = (((steamTtot / steam.Count) - 32f) / 1.8f) + 273.15f;
            }
            else
            {
                steamTemp = 0f;
            }

            for (int s = 0; s < steam.Count; s++)
            {
                SteamAmount += (steam[s].GetConcentration() * 1000f) / 1.093f;
                steamRate += (steam[s].GetConcentration() * 1000f) / 1.093f;
            }
            if(steamAmount > 0f)
            {
                waterStored.AddConcentration((steamAmount/1000f));// * condensationRatio
                //+= steamRate * condensationRatio;
                //L but exp rate was in Kg. 1 - 1 conversion
                steamAmount -= steamRate;
            }
            //Is this losing us water. Is there a limit on the outflow rate?
            //If too much water, water stop being pulled in, and the overcap treated as overflow volume.
            //if (waterStored.GetConcentration() > 10f)
            //{
            //    waterStored.SetConcentration(10f);
            //}
            waterAmount = waterStored.GetConcentration();
            if (reservoirPipe != null)
            {
                if (reservoirPipe.GetConcentration() < reservoirPipe.Volume)
                {
                    if (waterAmount > 0f)//waterStored.GetConcentration()
                    {
                        float rate = reservoirPipe.Throughput * Time.fixedDeltaTime;//m^3/s
                        if (rate + reservoirPipe.GetConcentration() > reservoirPipe.Volume)
                        {
                            rate = reservoirPipe.Volume - reservoirPipe.GetConcentration();
                        }
                        //transfer Rate is pipe throughput
                        if (waterAmount > (rate*1000f))
                        {
                            waterOut.SetConcentration(rate);//1000f
                            waterStored.AddConcentration(-rate);
                            //waterAmount -= (rate * 1000f);//1f
                        }
                        else
                        {
                            waterOut.SetConcentration(waterAmount);// /1000f
                            waterStored.SetConcentration(0f);
                            //waterAmount = 0f;
                        }
                        //Debug.Log(water.GetConcentration());
                        //object[] atmoDatas = { waterOut.GetTemp(), waterOut.GetLocalPressure(), waterOut };
                        //reservoirPipe.Receive(false, atmoDatas);

                        reservoirPipe.Receive(false, pressurizerVelocity, pressurizerLevel, waterOut, waterOut.GetTemp());
                    }
                    
                }
            }

            //empty the condenser of steam for now
            steam.Clear();
        }
    }
}