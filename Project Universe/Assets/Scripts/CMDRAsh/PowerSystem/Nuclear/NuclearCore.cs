using ProjectUniverse.Environment.Fluid;
using ProjectUniverse.Environment.Radiation;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectUniverse.PowerSystem.Nuclear
{
    public class NuclearCore : MonoBehaviour
    {
        public MultiDimensionNFR[] mdnfrRows;
        [System.Serializable]
        public class MultiDimensionNFR
        {
            public NuclearFuelRod[] fuelRodsCol;
        }

        private NuclearFuelRod[,] nfrMatrix;
        private float[,] nfrNeighborData;
        private float totalMWt;//add all rod
        private float averageActivity;//average all rod
        //private float averageTemp;//average all rod
        private float averageHeatEFr;
        private float totalEFr;
        private float totalBTU;
        //private float totalkWh;
        private GameObject[,] rodUISections;
        private GameObject[,] neighborUISections;
        [SerializeField] private SteamGenerator[] steamGens;
        public GameObject activityUIPrefab;
        //[SerializeField] private GameObject activityUI;
        [SerializeField] private GameObject tempUI;
        [SerializeField] private Color32 BLUE;
        [SerializeField] private Color32 GREEN;
        [SerializeField] private Color32 RED;
        [SerializeField] private Color32 YELLOW;
        private float timeScaled = 0f;
        [Range(0,1)]
        public float controlRodGlobal = 1f;
        private int rodsReal = 0;
        private float totalRequiredCoolant = 0f;
        private float averageTemperatureFuelRods = 0f;
        [SerializeField] private List<IFluidPipe> coolantInPipes;
        [SerializeField] private List<IFluidPipe> coolantOutPipes;
        [SerializeField] private float outputVelocity = 32f;
        [SerializeField] private float outputPressure = 200f;
        private IFluid coolantHotStored;
        private float hotCoolantLevel = 0f;
        public float monitorHotCoolant;
        [SerializeField] private float vesselPres = 200f;
        [SerializeField] private float vesselTemp = 300f;
        private float uiTimer = 0.5f;
        [SerializeField] private float leakAmount = 0f;//0 - 1 for amount of rads to release into Da Worldo
        private float detectedRads;
        [SerializeField] private IRadiationZone radiationArea;
        public bool releaseRadsEvent = false;

        public float[,] NeighborActivityData
        {
            get
            {
                return nfrNeighborData;
            }
        }
        
        public float GlobalControlRodInsertion
        {
            get
            {
                return controlRodGlobal;
            }
        }
        
        public SteamGenerator[] SteamGenerators
        {
            get
            {
                return steamGens;
            }
        }

        public NuclearFuelRod[,] NFRMatrix
        {
            get { return nfrMatrix; }
        }
        public int RodsReal
        {
            get { return rodsReal; }
        }
        public float VesselPres
        {
            get { return vesselPres; }
        }
        public float VesselTemp
        {
            get { return vesselTemp; }
        }
        public float DetectedRads
        {
            get { return detectedRads; }
        }

        // Start is called before the first frame update
        void Start()
        {
            timeScaled = Time.deltaTime * 10f;
            coolantHotStored = new IFluid("Coolant", 80.33f, 0, 200f, 0f);
            //for every row and column add nuclearfuelrods to nfrMatrix
            nfrMatrix = new NuclearFuelRod[mdnfrRows.Length, mdnfrRows[0].fuelRodsCol.Length];
            nfrNeighborData = new float[mdnfrRows.Length, mdnfrRows[0].fuelRodsCol.Length];
            rodUISections = new GameObject[mdnfrRows.Length, mdnfrRows[0].fuelRodsCol.Length];
            neighborUISections = new GameObject[mdnfrRows.Length, mdnfrRows[0].fuelRodsCol.Length];

            float x = 0.015f;
            float y = -0.18f;//.015f
            GameObject icon = Instantiate(activityUIPrefab);
            Debug.Log(mdnfrRows.Length);
            Debug.Log(mdnfrRows[0].fuelRodsCol.Length);
            for (int i = 0; i < mdnfrRows.Length; i++)
            {
                for (int j = 0; j < mdnfrRows[i].fuelRodsCol.Length; j++)
                {
                    x += 0.25f;
                    if (mdnfrRows[i].fuelRodsCol[j] != null)
                    {
                        nfrMatrix[i, j] = mdnfrRows[i].fuelRodsCol[j];
                        nfrMatrix[i, j].NuclearCore = this;
                        nfrMatrix[i, j].RodPosInCoreMatrix = new int[] { i, j };
                        rodsReal++;
                            
                        //create a cell in the UI display
                    //    GameObject newIcon = Instantiate(activityUIPrefab, activityUI.transform);
                        //newIcon.transform.SetParent(activityUI.transform);
                    //    newIcon.transform.localPosition = new Vector3(x, y, 0);
                    //    neighborUISections[i,j] = newIcon;
                    //    float activity = Mathf.Round(NeighborActivityData[i, j]);
                        // neighbor activity round to whole
                    //    newIcon.transform.GetChild(0).GetComponent<TMP_Text>().text = activity.ToString();
                        // -0 is blue
                        // 0 to 400 is green
                        // 400 to 900 is yellow
                        // 900+ is red.
                    /*    if (activity <= 0f)
                        {
                            newIcon.GetComponent<Image>().color = BLUE;
                        }
                        else if(activity <= 400)
                        {
                            newIcon.GetComponent<Image>().color = GREEN;
                        }
                        else if (activity <= 900)
                        {
                            newIcon.GetComponent<Image>().color = YELLOW;
                        }
                        else
                        {
                            newIcon.GetComponent<Image>().color = RED;
                        }*/

                        GameObject newIcon2 = Instantiate(activityUIPrefab, tempUI.transform);
                        //newIcon2.transform.SetParent(tempUI.transform);
                        newIcon2.transform.localPosition = new Vector3(x, y, 0);
                        rodUISections[i, j] = newIcon2;
                        float temp = Mathf.Round(nfrMatrix[i, j].RodCoreTemp);
                        newIcon2.transform.GetChild(0).GetComponent<TMP_Text>().text = temp.ToString();
                        // 0 to 373.15 is blue
                        // 373.15 to 875 is green
                        // 875 to 1000 is yellow
                        // 1000+ is red
                        if (temp < 373.15f)
                        {
                            newIcon2.GetComponent<Image>().color = BLUE;
                        }
                        else if (temp <= 875)
                        {
                            newIcon2.GetComponent<Image>().color = GREEN;
                        }
                        else if (temp <= 1000)
                        {
                            newIcon2.GetComponent<Image>().color = YELLOW;
                        }
                        else
                        {
                            newIcon2.GetComponent<Image>().color = RED;
                        }
                    }
                }
                y -= 0.25f;
                x = 0.015f;

            }
            Debug.Log("Gens: " + SteamGenerators.Length);
        }

        // Update is called once per frame
        void Update()
        {
            uiTimer -= Time.deltaTime;
            timeScaled = Time.deltaTime * 15f;
            totalRequiredCoolant = 0f;
            float currentCoolant = 0f;
            averageTemperatureFuelRods = 0f;

            //calculate neighbor activity
            NeighborActivityStep();
            //recalc rod vars for update period 1
            for (int i = 0; i < nfrMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < nfrMatrix.GetLength(1); j++)
                {
                    if (nfrMatrix[i, j] != null)
                    {
                        nfrMatrix[i, j].RecalcDataStage1();
                        averageTemperatureFuelRods += nfrMatrix[i, j].RodCoreTemp;
                        totalRequiredCoolant += nfrMatrix[i, j].CoolantMDot;
                    }
                }
            }
            averageTemperatureFuelRods /= rodsReal;

            //for cases where the core is disconnected from everything
            //This assumes that without steam gens, the coolant pipes can be ignored as well
            //This assumes that without steam gens, the core is not "running", IE losing heat or activity
            if (steamGens.Length > 0f)
            {
                //coolant efficiency
                float realDiff = 0f;

                //recalc coolant data
                // get total possible coolant rate from generators
                // include manual rate
                float totalPossibleCoolant = 0f;//Kg/hr
                float totalManual = 0f;
                for (int i = 0; i < steamGens.Length; i++)
                {
                    if (!steamGens[i].AutomaticControl)
                    {
                        totalPossibleCoolant += steamGens[i].ThresholdPumpRate;
                        totalManual += steamGens[i].ThresholdPumpRate;
                    }
                    else
                    {
                        totalPossibleCoolant += steamGens[i].MaxPumpRate;
                    }
                    realDiff += steamGens[i].CoolantCoolTemp;
                }

                //set total possible coolant to the amount of coolant extracted
                for (int l = 0; l < coolantInPipes.Count; l++)
                {
                    // extract coolant from the coolantpipe
                    //tPC is Kg/hr. Need m3/s. 
                    //totalPossibleCoolant \/
                    List<IFluid> coolantInFluid = coolantInPipes[l].ExtractFluid((totalRequiredCoolant / 1000f) / 3600f);//rate is m3/s
                    for (int k = 0; k < coolantInFluid.Count; k++)
                    {
                        //coolantHotStored.AddConcentration(coolantInFluid[k].GetConcentration());
                        hotCoolantLevel += coolantInFluid[k].GetConcentration();//m^3[inst]
                                                                                //Debug.Log("Core In (m3): " + coolantInFluid[k].GetConcentration());
                    }
                }
                monitorHotCoolant = hotCoolantLevel;
                //Debug.Log("Core Hot Coolant (m3): "+ coolantHotStored.GetConcentration());
                //adjust the totalPossibleCoolant and totalManual rates by the relative percent of cooland extracted
                //m3[inst] to m3/s to m3/hr to Kg/hr
                float inputkghr = (hotCoolantLevel / Time.fixedDeltaTime) * 3600f * 1000f;
                float Inputratio = (inputkghr) / totalPossibleCoolant;//hotCoolantLevel * 1000f
                                                                      //Debug.Log("hotCoolantm3[to Kg/Hr]: "+ inputkghr + "/ req[Kg/Hr]: " + totalRequiredCoolant);//totalPossibleCoolant
                totalPossibleCoolant *= Inputratio;
                totalManual *= Inputratio;


                realDiff /= steamGens.Length;
                float efficiencyCoolant = (averageTemperatureFuelRods - realDiff) / (averageTemperatureFuelRods - 300f);

                // if we don't have enough coolant (or barely enough)
                if (totalRequiredCoolant >= totalPossibleCoolant)
                {
                    float ratio = totalPossibleCoolant / totalRequiredCoolant;
                    for (int i = 0; i < nfrMatrix.GetLength(0); i++)
                    {
                        for (int j = 0; j < nfrMatrix.GetLength(1); j++)
                        {
                            if (nfrMatrix[i, j] != null)
                            {
                                float newCoolant = nfrMatrix[i, j].CoolantMDot * ratio;
                                currentCoolant += newCoolant;
                                nfrMatrix[i, j].RecalcDataStage2(newCoolant, efficiencyCoolant);
                            }
                        }
                    }
                }
                else // we have more than enough
                {
                    // caveat is manual coolant flow rate
                    if (totalManual >= totalRequiredCoolant)
                    {
                        float ratio = totalManual / totalRequiredCoolant;
                        for (int i = 0; i < nfrMatrix.GetLength(0); i++)
                        {
                            for (int j = 0; j < nfrMatrix.GetLength(1); j++)
                            {
                                if (nfrMatrix[i, j] != null)
                                {
                                    float newCoolant = nfrMatrix[i, j].CoolantMDot * ratio;
                                    currentCoolant += newCoolant;
                                    nfrMatrix[i, j].RecalcDataStage2(newCoolant, efficiencyCoolant);
                                }
                            }
                        }
                    }
                    else // we have enough coolant but not enough manual - no consequence
                    {
                        for (int i = 0; i < nfrMatrix.GetLength(0); i++)
                        {
                            for (int j = 0; j < nfrMatrix.GetLength(1); j++)
                            {
                                if (nfrMatrix[i, j] != null)
                                {
                                    float newCoolant = nfrMatrix[i, j].CoolantMDot;
                                    currentCoolant += newCoolant;
                                    nfrMatrix[i, j].RecalcDataStage2(newCoolant, efficiencyCoolant);
                                }
                            }
                        }
                    }
                }

                //set generator coolant required rate and temp
                int num = steamGens.Length;
                float reqRate = totalRequiredCoolant;
                for (int i = 0; i < steamGens.Length; i++)
                {
                    if (!steamGens[i].AutomaticControl)
                    {
                        steamGens[i].CurrentPumpRate = steamGens[i].ThresholdPumpRate;
                        steamGens[i].RequiredPumpRate = steamGens[i].ThresholdPumpRate;
                        reqRate -= steamGens[i].ThresholdPumpRate;//currentCoolant
                                                                  //Debug.Log("requiredCoolant: " + steamGens[i].RequiredPumpRate);
                        num--;
                    }
                    steamGens[i].CoolantHotTemp = averageTemperatureFuelRods;
                }
                if (reqRate <= 0f)
                {
                    reqRate = 0f;
                }
                for (int i = 0; i < steamGens.Length; i++)
                {
                    if (steamGens[i].AutomaticControl)
                    {
                        steamGens[i].RequiredPumpRate = reqRate / num;
                        //Debug.Log("requiredCoolant: " + steamGens[i].RequiredPumpRate);
                        steamGens[i].CurrentPumpRate = currentCoolant / num;
                    }
                }


                //pass coolant into coolantOutPipes
                //Debug.Log("hot coolant: " + hotCoolantLevel);
                if (hotCoolantLevel > 0f)
                {
                    for (int q = 0; q < coolantOutPipes.Count; q++)
                    {
                        //assuming coolant in same in all pipes
                        IFluid coolantHot = new IFluid(coolantHotStored);
                        coolantHot.SetTemp(averageTemperatureFuelRods);
                        //split concentration by number of pipes
                        float conc = ((totalPossibleCoolant / 3600f) / 1000f * Time.fixedDeltaTime) / coolantOutPipes.Count;
                        coolantHot.SetConcentration(conc);
                        //Debug.Log("Core Out (m3): " + coolantHot.GetConcentration() + " of " + hotCoolantLevel);
                        hotCoolantLevel -= conc;
                        if (hotCoolantLevel < 0f)
                        {
                            hotCoolantLevel = 0f;
                        }
                        coolantOutPipes[q].Receive(false, outputVelocity, outputPressure, coolantHot, coolantHot.GetTemp());
                    }
                }
            }
            ///leakAmount is controlled by dmg to vessel
            /// 0 means the vessel is intact
            /// < 1 & > .25 means the vessel is damaged
            /// < .25 to 0 means the vessel has been destroyed.
            radiationArea.GeneratorLeakMultiplier = leakAmount;
            detectedRads = radiationArea.RadiationAtOneMeter();

            if (releaseRadsEvent)
            {
                leakAmount = averageTemperatureFuelRods / 1000f;
                if(leakAmount > 1f)
                {
                    leakAmount = 1f;
                }
            }
            else
            {
                leakAmount = 0f;
            }
        }

        private void OnGUI()
        {
            if (uiTimer <= 0f)
            {
                //recalc core data
                //float totalActivity = 0f;//average all rod
                //float totalTemp = 0f;//average all rod
                //float totalHeatEFr = 0f;
                //float coolantFlowReq = 0f;
                totalMWt = 0f;
                totalBTU = 0f;

                //update core information
                for (int i = 0; i < nfrMatrix.GetLength(0); i++)
                {
                    for (int j = 0; j < nfrMatrix.GetLength(1); j++)
                    {
                        if (nfrMatrix[i, j] != null)
                        {
                            //totalActivity += nfrMatrix[i, j].PositiveActivity;
                            //totalTemp += nfrMatrix[i, j].RodCoreTemp;
                            //totalHeatEFr += nfrMatrix[i, j].HeatEFRRod;
                            totalMWt += nfrMatrix[i, j].MegaWattsThermal;
                            //coolantFlowReq += nfrMatrix[i, j].CoolantMDot;
                            totalBTU += nfrMatrix[i, j].BTUPerHour;

                            //if(neighborUISections[i, j] != null) { 
                            //if (uiTimer <= 0f)
                            //{
                                //update UI with rod data
                                //float activity = Mathf.Round(NeighborActivityData[i, j]);
                                //float activity = Mathf.Round(nfrMatrix[i, j].PositiveActivity);

                                // neighbor activity round to whole
                                /*neighborUISections[i, j].transform.GetChild(0).GetComponent<TMP_Text>().text =
                                    activity.ToString();// + "/" + activity2.ToString();
                                if (activity <= 0f)
                                {
                                    neighborUISections[i, j].GetComponent<Image>().color = BLUE;
                                }
                                else if (activity <= 400)
                                {
                                    neighborUISections[i, j].GetComponent<Image>().color = GREEN;
                                }
                                else if (activity <= 900)
                                {
                                    neighborUISections[i, j].GetComponent<Image>().color = YELLOW;
                                }
                                else
                                {
                                    neighborUISections[i, j].GetComponent<Image>().color = RED;
                                }*/
                                float temp = Mathf.Round(nfrMatrix[i, j].RodCoreTemp);
                                rodUISections[i, j].transform.GetChild(0).GetComponent<TMP_Text>().text = temp.ToString();
                                if (temp < 373.15f)
                                {
                                    rodUISections[i, j].GetComponent<Image>().color = BLUE;
                                }
                                else if (temp <= 875)
                                {
                                    rodUISections[i, j].GetComponent<Image>().color = GREEN;
                                }
                                else if (temp <= 1000)
                                {
                                    rodUISections[i, j].GetComponent<Image>().color = YELLOW;
                                }
                                else
                                {
                                    rodUISections[i, j].GetComponent<Image>().color = RED;
                                }
                            //}
                            //}
                        }
                    }
                }
                //averageActivity = totalActivity / rodsReal;
                //averageTemp = totalTemp / rodsReal;
                //averageHeatEFr = totalHeatEFr / rodsReal;
                //totalEFr = totalHeatEFr;

                //if (uiTimer <= 0f)
                //{
                    uiTimer = 0.5f;
                    //set text
                    //activityText.text = averageActivity.ToString();
                    //tempText.text = averageTemperatureFuelRods.ToString();
                   // avgEFrTxt.text = averageHeatEFr.ToString();
                    //totalEFrTxt.text = totalEFr.ToString();
                    //mwhText.text = totalMWt.ToString();
                    //coolantText.text = coolantFlowReq.ToString();
                    //btuText.text = totalBTU.ToString();
                //ctrl
                //if (controlRodGlobalText != null)
                //{
                //    controlRodGlobalText.text = ((int)(controlRodGlobal * 100f)).ToString();
                    //}
                //}
            }
        }

        /// <summary>
        /// Use the activity and position of each rod to calculate the activity they release into surroundings.
        /// First calculation performed in a rod update
        /// </summary>
        public void NeighborActivityStep()
        {
            for (int i = 0; i < nfrMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < nfrMatrix.GetLength(1); j++)
                {
                    //for every cell that is not null
                    if (nfrMatrix[i, j] != null)
                    {
                        NuclearFuelRod nfr = nfrMatrix[i, j];
                        //get total distributed rod activity (at i,j)
                        float currentRodActivity = ((nfr.AppliedNetActivity * nfr.ContributedActivity) +
                            ((nfr.RodCoreTemp - 273.15f) * nfr.TempToActivityMult)) * (Time.deltaTime*2f);
                        //currentRodActivity *= (1-controlRodGlobal);
                        //control rods absorb neighbor activity at a rate
                        currentRodActivity -= (controlRodGlobal * 900f * Time.deltaTime);
                        if(currentRodActivity < 0f)
                        {
                            currentRodActivity = 0f;
                        }
                        float perSideActivity = currentRodActivity / 4;
                        //get every cell in the 4 directions and add the activity to the neighbor data
                        //forwards row
                        float remainingActivity = perSideActivity;
                        for (int fr = i; fr < nfrMatrix.GetLength(0); fr++)
                        {
                            //calculate activity based on [fr,j]'s absorbtion rate
                            //cells absorb x% of the outgoing activity
                            if (nfrMatrix[fr, j] != null)
                            {
                                //Debug.Log(fr + "," + j);
                                nfrNeighborData[fr, j] += remainingActivity * nfrMatrix[fr, j].AbsorbedNeighborActivity;//+
                                //and pass the rest of the activity to the next cell
                                remainingActivity -= (remainingActivity * nfrMatrix[fr, j].AbsorbedNeighborActivity);
                            }
                            else //nothing to absorb the radiation
                            {
                                nfrNeighborData[fr, j] += remainingActivity;//+
                            }
                            if(controlRodGlobal > 0.5f)
                            {
                                nfrNeighborData[fr, j] -= (nfrNeighborData[fr, j] 
                                    * (controlRodGlobal - .5f) * Time.deltaTime * .1f);
                            }
                            if(nfrNeighborData[fr, j] < 0f)
                            {
                                nfrNeighborData[fr, j] = 0f;
                            }
                        }
                        //backwards row
                        remainingActivity = perSideActivity;
                        for (int br = i; br >= 0; br--)
                        {
                            //calculate activity based on [fr,j]'s absorbtion rate
                            //cells absorb x% of the outgoing activity
                            if (nfrMatrix[br, j] != null)
                            {
                                nfrNeighborData[br, j] += remainingActivity * nfrMatrix[br, j].AbsorbedNeighborActivity;//+
                                //and pass the rest of the activity to the next cell
                                remainingActivity -= (remainingActivity * nfrMatrix[br, j].AbsorbedNeighborActivity);
                            }
                            else //nothing to absorb the radiation
                            {
                                nfrNeighborData[br, j] += remainingActivity;//+
                            }
                            if (controlRodGlobal > 0.5f)
                            {
                                nfrNeighborData[br, j] -= (nfrNeighborData[br, j]
                                    * (controlRodGlobal - .5f) * Time.deltaTime * .1f);
                            }
                            if (nfrNeighborData[br, j] < 0f)
                            {
                                nfrNeighborData[br, j] = 0f;
                            }
                        }
                        //forwards col
                        remainingActivity = perSideActivity;
                        for (int fc = j; fc < nfrMatrix.GetLength(1); fc++)
                        {
                            //calculate activity based on [i,fr]'s absorbtion rate
                            //cells absorb x% of the outgoing activity
                            if (nfrMatrix[i, fc] != null)
                            {
                                nfrNeighborData[i, fc] += remainingActivity * nfrMatrix[i, fc].AbsorbedNeighborActivity;//+
                                //and pass the rest of the activity to the next cell
                                remainingActivity -= (remainingActivity * nfrMatrix[i, fc].AbsorbedNeighborActivity);
                            }
                            else //nothing to absorb the radiation
                            {
                                nfrNeighborData[i, fc] += remainingActivity;//+
                            }
                            if (controlRodGlobal > 0.5f)
                            {
                                nfrNeighborData[i, fc] -= (nfrNeighborData[i, fc]
                                    * (controlRodGlobal - .5f) * Time.deltaTime * .1f);
                            }
                            if (nfrNeighborData[i, fc] < 0f)
                            {
                                nfrNeighborData[i, fc] = 0f;
                            }
                        }
                        //backwards col
                        remainingActivity = perSideActivity;
                        for (int bc = j; bc >= 0; bc--)
                        {
                            //calculate activity based on [i,fr]'s absorbtion rate
                            //cells absorb x% of the outgoing activity
                            if (nfrMatrix[i, bc] != null)
                            {
                                nfrNeighborData[i, bc] += remainingActivity * nfrMatrix[i, bc].AbsorbedNeighborActivity;//+
                                //and pass the rest of the activity to the next cell
                                remainingActivity -= (remainingActivity * nfrMatrix[i, bc].AbsorbedNeighborActivity);
                            }
                            else //nothing to absorb the radiation
                            {
                                nfrNeighborData[i, bc] += remainingActivity;//+
                            }
                            if (controlRodGlobal > 0.5f)
                            {
                                nfrNeighborData[i, bc] -= (nfrNeighborData[i, bc]
                                    * (controlRodGlobal - .5f) * Time.deltaTime * 0.1f);
                            }
                            if (nfrNeighborData[i, bc] < 0f)
                            {
                                nfrNeighborData[i, bc] = 0f;
                            }
                        }
                    }

                }
            }
        }

        public void ExternalInteractFunc(int i)
        {
            if (i == 1)
            {
                //control rods down 5
                controlRodGlobal -= .05f;
            }
            else if(i == 2)
            {
                //down 10
                controlRodGlobal -= .10f;
            }
            else if(i == 3)
            {
                //up 5
                controlRodGlobal += .05f;
            }
            else if(i == 4)
            {
                // up 10
                controlRodGlobal += .10f;
            }
            else if(i == 5)
            {
                //scram
                controlRodGlobal = 1f;
            }
            else if (i == 6)
            {
                //leak
                releaseRadsEvent = !releaseRadsEvent;
            }
            if (controlRodGlobal < 0)
            {
                controlRodGlobal = 0f;
            }
            else if (controlRodGlobal > 1)
            {
                controlRodGlobal = 1f;
            }
            
        }
    }
}