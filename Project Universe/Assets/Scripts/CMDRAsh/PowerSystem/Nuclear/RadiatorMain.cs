using ProjectUniverse.Environment.Gas;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class distributes low pressure steam into radiator circuits.
/// </summary>
namespace ProjectUniverse.PowerSystem.Nuclear
{
    public class RadiatorMain : MonoBehaviour
    {
        // Start is called before the first frame update
        [SerializeField] private IGasPipe feedPipe;
        [SerializeField] private IGasPipe rad1;
        [SerializeField] private IGasPipe rad2;
        [SerializeField] private IGasPipe rad3;
        [SerializeField] private IGasPipe rad4;
        [SerializeField] private bool valve1;//true is open
        [SerializeField] private bool valve2;
        [SerializeField] private bool valve3;
        [SerializeField] private bool valve4;
        [SerializeField] private bool mainHeadMode;//MHM pushes steam to radiators, rather than collects it.
        [SerializeField] private float rad1Count;//number of radiators on circuit
        [SerializeField] private float rad2Count;
        [SerializeField] private float rad3Count;
        [SerializeField] private float rad4Count;
        private float inlet1temp = 0f;
        private float outlet1temp = 0f;
        private float inlet2temp = 0f;
        private float outlet2temp = 0f;
        private float inlet3temp = 0f;
        private float outlet3temp = 0f;
        private float inlet4temp = 0f;
        private float outlet4temp = 0f;
        private float steamflow1 = 0f;
        private float steamflow2 = 0f;
        private float steamflow3 = 0f;
        private float steamflow4 = 0f;
        //public float SteamTemp { get => steamTemp; set => steamTemp = value; }
        //public float SteamRate { get => steamRate; set => steamRate = value; }
        private float flowVelAll = 120f;
        private float pressureAll = 15f;

        public bool Valve1
        {
            get { return valve1; }
            set { valve1 = value; }
        }
        public bool Valve2
        {
            get { return valve2; }
            set { valve2 = value; }
        }
        public bool Valve3
        {
            get { return valve3; }
            set { valve3 = value; }
        }
        public bool Valve4
        {
            get { return valve4; }
            set { valve4 = value; }
        }
        public float SteamFlow1
        {
            get { return steamflow1; }
        }
        public float SteamFlow2
        {
            get { return steamflow2; }
        }
        public float SteamFlow3
        {
            get { return steamflow3; }
        }
        public float SteamFlow4
        {
            get { return steamflow4; }
        }
        public float InflowTemp1
        {
            get { return inlet1temp; }
        }
        public float InflowTemp2
        {
            get { return inlet2temp; }
        }
        public float InflowTemp3
        {
            get { return inlet3temp; }
        }
        public float InflowTemp4
        {
            get { return inlet4temp; }
        }

        public float OutflowTemp1
        {
            get { return outlet1temp; }
        }
        public float OutflowTemp2
        {
            get { return outlet2temp; }
        }
        public float OutflowTemp3
        {
            get { return outlet3temp; }
        }
        public float OutflowTemp4
        {
            get { return outlet4temp; }
        }
        public float ConnectedRadiators1
        {
            get { return rad1Count; }
        }
        public float ConnectedRadiators2
        {
            get { return rad2Count; }
        }
        public float ConnectedRadiators3
        {
            get { return rad3Count; }
        }
        public float ConnectedRadiators4
        {
            get { return rad4Count; }
        }

        private void Update()
        {
            int pathcnt = 0;
            if (valve1)
            {
                pathcnt++;
            }
            if (valve2)
            {
                pathcnt++;
            }
            if (valve3)
            {
                pathcnt++;
            }
            if (valve4)
            {
                pathcnt++;
            }

            if (mainHeadMode)
            {
                inlet1temp = 0f;
                inlet2temp = 0f;
                inlet3temp = 0f;
                inlet4temp = 0f;

                List<IGas> Insteam = new List<IGas>();
                if (valve1 || valve2 || valve3 || valve4)
                {
                    //pull steam from feedpipe
                    pressureAll = feedPipe.GlobalPressure;
                    flowVelAll = feedPipe.FlowVelocity / pathcnt;
                    Insteam = feedPipe.ExtractGasses(-1f);
                }
                else
                {
                    pressureAll = feedPipe.GlobalPressure;
                    flowVelAll = 0f;
                    inlet1temp = 0f;
                }

                float steamFlowRate = 0f;
                //get flow rate
                for (int f = 0; f < Insteam.Count; f++)
                {
                    //m^3[inst] to m^3/s to kg/s
                    steamFlowRate += (Insteam[f].GetConcentration() / Time.fixedDeltaTime) * 1000f * 3600f;
                    //sfr is kg/hr
                }

                steamflow1 = 0f;
                steamflow2 = 0f;
                steamflow3 = 0f;
                steamflow4 = 0f;
                //send steam through as many radiators as are open
                for (int i = 0; i < Insteam.Count; i++)
                {
                    //create a gas that is 1/x the conc and velocity of the inlet gas
                    //assuming that p1v1 = xp2v2:

                    //push the gas into the open pipes
                    if (valve1)
                    {
                        IGas branchGas = new IGas(Insteam[i]);
                        branchGas.SetConcentration(Insteam[i].GetConcentration() / pathcnt);
                        inlet1temp = branchGas.GetTemp();
                        //Debug.Log("flow 1");
                        rad1.Receive(false, (flowVelAll), pressureAll, branchGas, branchGas.GetTemp());
                        //Debug.Log(feedPipe.GlobalPressure);
                        steamflow1 = steamFlowRate / pathcnt;

                    }
                    else
                    {
                        inlet1temp = 300f;
                        steamflow1 = 0f;
                    }
                    if (valve2)
                    {
                        IGas branchGas = new IGas(Insteam[i]);
                        branchGas.SetConcentration(Insteam[i].GetConcentration() / pathcnt);
                        inlet2temp = branchGas.GetTemp();
                        //Debug.Log("flow 2");
                        rad2.Receive(false, (flowVelAll), pressureAll, branchGas, branchGas.GetTemp());
                        steamflow2 = steamFlowRate / pathcnt;
                    }
                    else
                    {
                        inlet2temp = 300f;
                        steamflow2 = 0f;
                    }
                    if (valve3)
                    {
                        IGas branchGas = new IGas(Insteam[i]);
                        branchGas.SetConcentration(Insteam[i].GetConcentration() / pathcnt);
                        inlet3temp = branchGas.GetTemp();
                        //Debug.Log("flow 3");
                        rad3.Receive(false, (flowVelAll), pressureAll, branchGas, branchGas.GetTemp());
                        steamflow3 = steamFlowRate / pathcnt;
                    }
                    else
                    {
                        inlet3temp = 300f;
                        steamflow3 = 0f;
                    }
                    if (valve4)
                    {
                        IGas branchGas = new IGas(Insteam[i]);
                        branchGas.SetConcentration(Insteam[i].GetConcentration() / pathcnt);
                        inlet4temp = branchGas.GetTemp();
                        //Debug.Log("flow 4");
                        rad4.Receive(false, (flowVelAll), pressureAll, branchGas, branchGas.GetTemp());
                        steamflow4 = steamFlowRate / pathcnt;
                    }
                    else
                    {
                        inlet4temp = 300f;
                        steamflow4 = 0f;
                    }
                }
            }
            else //extract steam from radiator circuits
            {
                outlet1temp = 0f;
                outlet2temp = 0f;
                outlet3temp = 0f;
                outlet4temp = 0f;

                //get steam from all pipes
                List<IGas> Insteam1 = new List<IGas>();
                if (valve1)
                {
                    Insteam1 = rad1.ExtractGasses(-1f);
                    if (Insteam1.Count >= 1)
                    {
                        outlet1temp = Insteam1[0].GetTemp();
                    }
                }
                List<IGas> Insteam2 = new List<IGas>();
                if (valve2)
                {
                    Insteam1 = rad2.ExtractGasses(-1f);
                    if (Insteam2.Count >= 1)
                    {
                        outlet2temp = Insteam2[0].GetTemp();
                    }
                }
                List<IGas> Insteam3 = new List<IGas>();
                if (valve3)
                {
                    Insteam1 = rad3.ExtractGasses(-1f);
                    if (Insteam3.Count >= 1)
                    {
                        outlet3temp = Insteam3[0].GetTemp();
                    }
                }
                List<IGas> Insteam4 = new List<IGas>();
                if (valve4)
                {
                    Insteam1 = rad4.ExtractGasses(-1f);
                    if (Insteam4.Count >= 1)
                    {
                        outlet4temp = Insteam4[0].GetTemp();
                    }
                }

                List<IGas> AllSteams = new List<IGas>();
                AllSteams.AddRange(Insteam1);
                AllSteams.AddRange(Insteam2);
                AllSteams.AddRange(Insteam3);
                AllSteams.AddRange(Insteam4);

                float avgV = (rad1.FlowVelocity + rad2.FlowVelocity + rad3.FlowVelocity + rad4.FlowVelocity) / 4f;//should be avg of open pipes,
                //but flow V doesn't get set to 0 when no flow through pipe (yet).
                float avgP = (rad1.GlobalPressure + rad2.GlobalPressure + rad3.GlobalPressure + rad4.GlobalPressure) / 4f;
                float avgT = (outlet1temp + outlet2temp + outlet3temp + outlet4temp) / pathcnt;

                //push into feedpipe
                feedPipe.Receive(false, avgV, avgP, AllSteams, avgT);
            }
        }
    }
}