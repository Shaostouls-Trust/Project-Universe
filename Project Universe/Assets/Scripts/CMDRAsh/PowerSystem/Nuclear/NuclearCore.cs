using ProjectUniverse.Environment.Fluid;
using ProjectUniverse.Environment.Radiation;
using ProjectUniverse.Ship;
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
        [SerializeField] private TMPro.TMP_Text screenModeText;
        //private float timeScaled = 0f;
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
        [SerializeField] private float vesselPres = 215f;
        [SerializeField] private float vesselTemp = 300f;
        [SerializeField] private float vesselMaxPres = 224f;//bar
        [SerializeField] private float vesselMaxTemp = 600;//K, melting point lead is 327C
        //internal temp not to exceed 1350K, because 1400K is melting point of U
        //vessel max temp is 600K (327C), or rads leak out
        private float uiTimer = 0.5f;
        [SerializeField] private float leakAmount = 0f;//0 - 1 for amount of rads to release into Da Worldo
        private float detectedRads;
        [SerializeField] private IRadiationZone radiationArea;
        public bool releaseRadsEvent = false;
        private bool showActivity;
        private bool showTemp = true;
        private bool showFuel;
        private float meltdownMarkiplier = 0f;//0 to 1.0
        private bool manualMeltdownMarkiplier;
        private bool playedOnce;
        [SerializeField] private AudioSource src;
        [SerializeField] private AudioSource[] breachAlarms;
        //but pins will break first, so pressure (normal stress) cannot exceed max shear
        //pins are titanium, to 900 MPa at 10C to 410MPa at 550C. Linear.
        //private float stress;//MPa

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
        public float MaxVesselPres
        {
            get { return vesselMaxPres; }
        }
        public float DetectedRads
        {
            get { return detectedRads; }
        }

        // Start is called before the first frame update
        void Start()
        {
            //timeScaled = Time.deltaTime * 10f;
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
                        
                        GameObject newIcon2 = Instantiate(activityUIPrefab, tempUI.transform);
                        //newIcon2.transform.SetParent(tempUI.transform);
                        newIcon2.transform.localPosition = new Vector3(x, y, 0);
                        rodUISections[i, j] = newIcon2;
                        float temp = Mathf.Round(nfrMatrix[i, j].RodCoreTemp);
                        newIcon2.transform.GetChild(0).GetComponent<TMP_Text>().text = temp.ToString();
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
            //timeScaled = Time.deltaTime * 15f;
            totalRequiredCoolant = 0f;
            float currentCoolant = 0f;
            averageTemperatureFuelRods = 0f;
            float supplyingGens = steamGens.Length;

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
                    //determine how many gens are supplying
                    if(steamGens[i].SysFail || (steamGens[i].ThresholdPumpRate <= 0f && steamGens[i].RequiredPumpRate >= 1f))
                    {
                        supplyingGens--;
                    }
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
                        //split concentration by number of pipes (check end tank state)

                        float gens = steamGens.Length;//assume that coolantOutPipes is equal to number of generators
                        for(int g = 0;g < steamGens.Length; g++)
                        {
                            if (steamGens[g].CoolantReservoir > steamGens[g].CoolantReservoirMaxCap)
                            {
                                gens--;
                            }
                        }
                        if(gens <= 0)
                        {
                            gens = coolantOutPipes.Count;
                        }
                        float conc = ((totalPossibleCoolant / 3600f) / 1000f * Time.fixedDeltaTime) / gens;//coolantOutPipes.Count;
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
            //heat IO is done. Calculate vessel temp, pressure, and stress
            //VesselTemp will gradually increase to 0.5f of the rod temp.
            //the greater the difference, the faster it heats up
            float progress = Mathf.Log10(Mathf.Abs((averageTemperatureFuelRods * 0.47f) - vesselTemp))/10f;
            //Debug.Log(progress);
            vesselTemp = Mathf.Lerp(vesselTemp, averageTemperatureFuelRods * 0.47f,progress);
            /// <summary>
            /// When all generators are running, the pressure will be only a little higher than the 210 input
            /// As generators shut off the pressure must increase as a func of temp to some amount. 
            /// 4 generators supplying: pressure is input + .025 * rod temp(K)
            /// 3 gens: input + 10 + .05 * rod temp(K) (258/900 at 1030C)
            /// 2 gens: input + 20 + .075 * rod temp (288/900 at 1030C)
            /// 1 gen: input + 30 + .1 * rod temp (317/900 at 1030C)
            /// 0 gens: base + .2 * rod temp (210 base 365/900 at 1030C)
            /// </summary>
            switch(supplyingGens)
            {
                case 4:
                    vesselPres = 210f + (0.025f * averageTemperatureFuelRods);
                    break;
                case 3:
                    vesselPres = 210f + 10f + (0.05f * averageTemperatureFuelRods);
                    break;
                case 2:
                    vesselPres = 210f + 20f + (0.075f * averageTemperatureFuelRods);
                    break;
                case 1:
                    vesselPres = 210f + 30f + (0.1f * averageTemperatureFuelRods);
                    break;
                case 0:
                    vesselPres = 210f + (0.2f * averageTemperatureFuelRods);
                    break;
                default:
                    vesselPres = 210f + (0.025f * averageTemperatureFuelRods);
                    break;
            }
            
            //strength is in MPa. Pressures in bar. 1 MPa = 10 bar, but pins will hold, say, 0.1f of that.
            //350 bar at 600K.
            vesselMaxPres = Util.Utils.StrengthVersusTemperature_Titanium(VesselTemp);
            if(vesselPres > vesselMaxPres)
            {
                //boom.
                releaseRadsEvent = true;
                if (!playedOnce)
                {
                    playedOnce = true;
                    src.Play();
                    StartCoroutine(WaitForSeconds());
                }
                //release particles
            }

            if (vesselTemp > vesselMaxTemp)//the lead shielding has melted
            {
                //melt down - the longer we are over temp, the more rads are released
                if(meltdownMarkiplier < 1.0f)
                {
                    meltdownMarkiplier += Time.smoothDeltaTime / 10f;
                    if(meltdownMarkiplier > 1.0f)
                    {
                        meltdownMarkiplier = 1.0f;
                    }
                }
                releaseRadsEvent = true;
            }
            else
            {
                //if we went over temp, but cooled down, stop releasing rads
                if (meltdownMarkiplier > 0f && !manualMeltdownMarkiplier)
                {
                    if (!playedOnce)//the core has not been blown open
                    {
                        meltdownMarkiplier -= Time.smoothDeltaTime / 10f;
                        if (meltdownMarkiplier <= 0f)
                        {
                            meltdownMarkiplier = 0f;
                            releaseRadsEvent = false;
                        }
                    }
                }
                else if (manualMeltdownMarkiplier)
                {
                    meltdownMarkiplier = 0.01f;
                }
            }

            ///leakAmount is controlled by dmg to vessel
            /// 0 means the vessel is intact
            /// < 1 & > .25 means the vessel is damaged
            /// < .25 to 0 means the vessel has been destroyed.
            
            if (releaseRadsEvent)
            {
                leakAmount = (averageTemperatureFuelRods / 1000f) * meltdownMarkiplier;
                if (leakAmount > 1f)
                {
                    leakAmount = 1f;
                }
            }
            else
            {
                leakAmount = 0f;
            }

            radiationArea.GeneratorLeakMultiplier = leakAmount;
            detectedRads = radiationArea.RadiationAtOneMeter();

            
        }

        public IEnumerator WaitForSeconds()
        {
            //pause before turning these on
            yield return new WaitForSeconds(2f);
            for (int i = 0; i < breachAlarms.Length; i++)
            {
                breachAlarms[i].Play();
            }
            //lights red
            RenderStateManager rsm = transform.GetComponentInParent<RenderStateManager>();
            if(rsm != null)
            {
                rsm.AllLightsRed();
                rsm.AllVolumeEffect(1);
            }
        }

        private void OnGUI()
        {
            if (uiTimer <= 0f)
            {
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
                            if (showTemp)
                            {
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
                                screenModeText.text = "Main Screen Mode: | Reactivity | [Temperature] | Fuel Status |";
                            }
                            else if (showActivity)
                            {
                                float activity = Mathf.Round(nfrMatrix[i, j].PositiveActivity);
                                rodUISections[i, j].transform.GetChild(0).GetComponent<TMP_Text>().text =
                                activity.ToString();// + "/" + activity2.ToString();
                                if (activity <= 0f)
                                {
                                    rodUISections[i, j].GetComponent<Image>().color = BLUE;
                                }
                                else if (activity <= 200)
                                {
                                    rodUISections[i, j].GetComponent<Image>().color = GREEN;
                                }
                                else if (activity <= 300)
                                {
                                    rodUISections[i, j].GetComponent<Image>().color = YELLOW;
                                }
                                else
                                {
                                    rodUISections[i, j].GetComponent<Image>().color = RED;
                                }
                                screenModeText.text = "Main Screen Mode: | [Reactivity] | Temperature | Fuel Status |";
                            }
                            else if(showFuel)
                            {
                                float fuel = Mathf.Round(nfrMatrix[i, j].FuelMass/1000f);
                                rodUISections[i, j].transform.GetChild(0).GetComponent<TMP_Text>().text = fuel.ToString();
                                rodUISections[i, j].GetComponent<Image>().color = Color.green;
                                screenModeText.text = "Main Screen Mode: | Reactivity | Temperature | [Fuel Status] |";
                            }
                        }
                    }
                }
                uiTimer = 0.5f;
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
            switch (i)
            {
                case 1:
                    //control rods down 5
                    controlRodGlobal -= .05f;
                    break;
                case 2:
                    //down 10
                    controlRodGlobal -= .10f;
                    break;
                case 3:
                    //up 5
                    controlRodGlobal += .05f;
                    break;
                case 4:
                    // up 10
                    controlRodGlobal += .10f;
                    break;
                case 5:
                    //scram
                    controlRodGlobal = 1f;
                    break;
                case 6:
                    //leak
                    releaseRadsEvent = !releaseRadsEvent;
                    manualMeltdownMarkiplier = releaseRadsEvent;
                    break;
                case 7:
                    //show reactivity
                    showActivity = true;
                    showTemp = false;
                    showFuel = false;
                    break;
                case 8:
                    //show temp
                    showActivity = false;
                    showTemp = true;
                    showFuel = false;
                    break;
                case 9:
                    //show fuel
                    showActivity = false;
                    showTemp = false;
                    showFuel = true;
                    break;
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