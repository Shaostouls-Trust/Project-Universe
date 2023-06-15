using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.PowerSystem.Nuclear
{
    public class NuclearFuelRod : MonoBehaviour
    {
        [SerializeField] private NuclearCore nuclearCoreBody;//gives access to neighbor data
        [SerializeField] private float coreTemp = 273.15f;
        [SerializeField] private float baseActivity = 19.34f;
        [SerializeField] private float contributedActivity = 0.12f;
        [SerializeField] private float absorbedSelfActivity = 0.88f;
        [SerializeField] private float absorbedNeighborActivity = 0.74355f;
        [SerializeField] private float neutronSecondsToKelvin = 8.32E-9f;//8.32E-9?//3.85E-8
        [SerializeField] private float kelvinToJoule = 0.00851f;
        [SerializeField] private float tempToActivityMult = 0.0245f;
        [SerializeField] private float negativeActivityPercent = 0.38f;
        [SerializeField] private float negativeActivitySteam = 0.002f;//0.00001f//0.184
        [SerializeField] private float rodMass = 1640830.078f;
        [Range(0f, 1f)]
        [SerializeField] private float controlRodInsertion = 0.0f;
        private float neighborActivity=0f;
        private float positiveActivity=0f;
        private float positiveActivityNoRods = 0f;
        private float positiveHeat=0f;
        private float primaryCoolantEfficiency = 0.9125f;
        private float heatEFrRod=0f;
        private float btuPerHour=0f;
        private float coolantMDotPerHour=0f;
        //private float coolantMDotReal = 0f;
        private float kiloWatHourThermal=0f;
        private float megaWatHourThermal=0f;
        private float negativeHeat=0f;
        private float netHeat = 0f;
        private float negativeActivity=0f;
        private float netActivity = 0f;
        private float timeScaled = 0f;
        //core position
        private int[] pos = new int[2];
        private float maxTemp = 1350f;

        /// <summary>
        /// The net activity of the cell in question. Used in neighbor and nextframe calcs
        /// </summary>
        public float AppliedNetActivity
        {
            get { return netActivity; }
        }
        
        /// <summary>
        /// Activity used to generate steam and run the reactor
        /// </summary>
        public float PositiveActivity
        {
            get { return positiveActivity; }
        }

        public float ContributedActivity
        {
            get { return contributedActivity; }
        }
        
        public float RodCoreTemp
        {
            get { return coreTemp; }
        }
        public float TempToActivityMult
        {
            get { return tempToActivityMult; }
        }
        public float AbsorbedNeighborActivity
        {
            get { return absorbedNeighborActivity; }
        }
        public int[] RodPosInCoreMatrix
        {
            get { return pos; }
            set
            {
                pos = value;
            }
        }
        public float ControlRodInsertion
        {
            get { return controlRodInsertion; }
            set { controlRodInsertion = value; }
        }
        public float MegaWattsThermal
        {
            get { return megaWatHourThermal; }
        }
        public float CoolantMDot
        {
            get { return coolantMDotPerHour; }
        }

        public float DeltaTemp
        {
            get { return netHeat; }
        }

        public NuclearCore NuclearCore
        {
            get
            {
                return nuclearCoreBody;
            }
            set 
            {
                nuclearCoreBody = value; 
            }
        }

        public float NeighborActivity
        {
            get { return neighborActivity; }
        }

        public float HeatEFRRod
        {
            get { return heatEFrRod; }
        }

        public float FuelMass
        {
            get { return rodMass; }
        }

        public float BTUPerHour { get { return btuPerHour; } }

        // Start is called before the first frame update
        void Start()
        {
            netActivity = baseActivity;
            positiveActivity = baseActivity;
            timeScaled = Time.deltaTime * 10f;
        }
        
        
        // Update is called once per frame
        /*void Update()
        {
            timeScaled = nuclearCoreBody.TimeScaled;
        }*/

        public void RecalculateData()
        {
            controlRodInsertion = nuclearCoreBody.GlobalControlRodInsertion;
            timeScaled = Time.deltaTime * 10f;//15f
            
            coreTemp += netHeat;
            neighborActivity = nuclearCoreBody.NeighborActivityData[pos[0],pos[1]];
            positiveActivity = ((absorbedSelfActivity * netActivity) + (baseActivity * 0.5f)
                + ((coreTemp - 273.15f) * tempToActivityMult)) * timeScaled;
            positiveActivity += neighborActivity;
            //positive activityNoRods is the activity before control rods are applied. Flat lim on pos act.
            positiveActivityNoRods = positiveActivity;
            //positiveActivity *= (1 - controlRodInsertion);
            //The positive activity is a flat limit on the activityNoRods
            positiveActivity = positiveActivityNoRods * (1 - controlRodInsertion);
            positiveHeat = neutronSecondsToKelvin * positiveActivity * rodMass * timeScaled;
            //energy exchange
            if (coreTemp >= 373.15f)
            {
                heatEFrRod = (coreTemp - 373.15f) * kelvinToJoule * 3600f;
                btuPerHour = (heatEFrRod * primaryCoolantEfficiency) / 155.06f;
                //coolantMDotPerHour is the required/proper amount
                coolantMDotPerHour = (btuPerHour / 1.0f * (heatEFrRod*0.05f)) / 2.2f;//heatEFr / 3600f
                
                //else the coolant rate is too high, which will be adjusted by the core/steam generators
                kiloWatHourThermal = (coolantMDotPerHour * 2086.8524f) / 3412.142f;//coolantMDotPerHour
                megaWatHourThermal = kiloWatHourThermal / 1000f;
                negativeActivity = ((positiveActivity * negativeActivityPercent)
                    + (kiloWatHourThermal * negativeActivitySteam)) * timeScaled;
            }
            else
            {
                heatEFrRod = 0f;
                btuPerHour = 0f;
                coolantMDotPerHour = 0f;
                kiloWatHourThermal = 0f;
                megaWatHourThermal = 0f;
                negativeActivity = 0f;
            }
            negativeHeat = (heatEFrRod * 0.0008f) * timeScaled;

            //recalc net vars for next update
            netHeat = (positiveHeat - negativeHeat);
            // also remove reactivity according to control rod insertion
            netActivity = (positiveActivity - negativeActivity);// - ((controlRodInsertion)*PositiveActivity);
        }

        public void RecalcDataStage1()
        {
            controlRodInsertion = nuclearCoreBody.GlobalControlRodInsertion;
            timeScaled = Time.deltaTime * 15f;

            coreTemp += netHeat;
            //float appliedBaseActivity = baseActivity * (1 - controlRodInsertion);
            neighborActivity = nuclearCoreBody.NeighborActivityData[pos[0], pos[1]];
            positiveActivity = ((absorbedSelfActivity * netActivity) + (baseActivity * 0.5f)
                + ((coreTemp - 273.15f) * tempToActivityMult)) * timeScaled;
            positiveActivity += neighborActivity;

            //reduce positive activity
            positiveActivity *= (1 - controlRodInsertion);

            positiveHeat = neutronSecondsToKelvin * positiveActivity * rodMass * timeScaled;

            //energy exchange
            if (coreTemp >= 373.15f)
            {
                heatEFrRod = (coreTemp - 373.15f) * kelvinToJoule * 3600f;
                btuPerHour = (heatEFrRod * primaryCoolantEfficiency) / 155.06f;
                //coolantMDotPerHour is the required/proper amount
                coolantMDotPerHour = (btuPerHour / 1.0f * (heatEFrRod * 0.05f)) / 2.2f;//heatEFr / 3600f
            }
            else
            {
                heatEFrRod = 0f;
                btuPerHour = 0f;
                coolantMDotPerHour = 0f;
            }
        }

        /// <summary>
        /// Stage 2 is the application of primary coolant to the fuel rod.
        /// Coolant efficiency is the relativity in temperature of the input coolant and the output coolant.
        /// </summary>
        /// <param name="realMDot"></param>
        /// <param name="coolantEfficiency"></param>
        public void RecalcDataStage2(float realMDot, float coolantEfficiency)
        {
            //coolantMDotReal = realMDot;
            if (coreTemp >= 373.15f)
            {
                kiloWatHourThermal = (realMDot * 2086.8524f) / 3412.142f;//coolantMDotPerHour
                megaWatHourThermal = kiloWatHourThermal / 1000f;
                negativeActivity = ((positiveActivity * negativeActivityPercent)
                    + (kiloWatHourThermal * negativeActivitySteam)) * timeScaled;
            }
            else
            {
                kiloWatHourThermal = 0f;
                megaWatHourThermal = 0f;
                negativeActivity = 0f;
            }
            //replace neg heat with realMDot calculation
            if (float.IsNaN(realMDot))
            {
                negativeHeat = (heatEFrRod * 0.0008f) * timeScaled;
            }
            else
            {
                //Debug.Log("realMDot:" + realMDot);
                negativeHeat = (realMDot * 0.0004f) * coolantEfficiency * timeScaled;//.0005
            }
            //recalc net vars for next update
            netHeat = (positiveHeat - negativeHeat);
            // also remove reactivity according to control rod insertion
            netActivity = (positiveActivity - negativeActivity);
        }
    }
}