using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using PU_Gases;

/// <summary>
/// Transfer Gases of X types at whatever rate/pressure determined by the pump
/// Vars:
/// IGas[] - Array of gases found in pipe
/// | next - next pipe in linkedlist
/// | temp - temperature of the pipe. Influenced by internal gas temp and ambient temp.
/// | tempTol[H,C] - pipe tollerance of hot or cold before bursting
/// | GlobalPressure - total system/static pressure. Does not factor in temp or volume.
/// | LocalPressure - interal applied pressure based on temp and volume
/// | maxP - maxiumum intenal pressure the pipe can handle
/// | health - pipe's health
/// | leakrate - % leak from bullet holes into local atmo
/// | bulletholes - List of all bullet holes in the pipe. Mainly for VFX placemement purposes.
/// | insulationRating - percent of heat kept in or out. Intended to be higher for superhot pipes, and lower for ducts and water pipes.
/// 
/// Transfer gases, pressure, and temp to the 'next' duct(s)
/// 'next' is set by master node while it traverses the whole duct line.
/// </summary> 

//airducts: Gas: Oxygen at 70F, 1.387132g/L, 1.544215m3 in 0.4m3 at 1.047273atm
public class IGasPipe : MonoBehaviour
{
    [SerializeField] private List<IGas> gasses;
    //private IGasPipe next;
    //private LinkedListNode<IGasPipe> nextLLN;
    [SerializeField] private IGasPipe[] neighbors;
    [SerializeField] private float temp;
    [SerializeField] private float[] tempTol = new float[2];
    //[SerializeField] private float pressure;
    [SerializeField] private float globalPressure;
    [SerializeField] private float appliedPressure;
    [SerializeField] private float maxP;
    [SerializeField] private float volume_m3;//standard duct is .4m3
    [SerializeField] private float health;
    [SerializeField] private float leakRate;
    [SerializeField] private GameObject[] bulletHoles;
    [SerializeField] private float throughput_m3;
    [SerializeField] private GameObject vent;
    [SerializeField] private Volume ductVolume;
    [SerializeField] private float insulationRating = 0.1f;
    [SerializeField] private bool burst;

    /// <summary>
    /// atmoData; ducttemp, ductpressure, List<IGas> gasses
    /// </summary>
    public void TransferTo(LinkedListNode<IGasPipe> next, params object[] atmoData)
    {
        //set temps and pressures
        temp = (float)Math.Round((float)atmoData[0],4);
        ///pressure = (float)Math.Round((float)atmoData[1],4);///
        globalPressure = (float)Math.Round((float)atmoData[1], 4);
        appliedPressure = (float)Math.Round((float)atmoData[1], 4);
        gasses = (List<IGas>)atmoData[2];
        /*
        for(int g = 0; g < gasses.Count; g++)
        {
            List<IGas> outflowGasses = (List<IGas>)atmoData[2];
            IGas remainder;
            float defecit = SubtractGas(gasses[g],outflowGasses[g], out remainder);
            gasses[g] = remainder;
            if (defecit > 0)
            {
                float conc = outflowGasses[g].GetConcentration() + defecit;
                outflowGasses[g].SetConcentration(conc);
            }
        }
        */
        next.Value.Receive(true, atmoData);
    }

    public void TransferTo(IGasPipe next, params object[] atmoData)
    {
        //set temps and pressures
        temp = (float)atmoData[0];
        globalPressure = (float)Math.Round((float)atmoData[1], 4);
        gasses = (List<IGas>)atmoData[2];
        //calculate this duct's applied pressure?
        appliedPressure = (float)Math.Round((float)atmoData[1], 4);
        next.Receive(true, atmoData);
    }

    /// <summary>
    /// Receive temperature, pressure, and gasses to the pipe/cable.
    /// IE add
    /// Destructive replaces the original gas list with the parameter list
    /// </summary>
    /// <param name="atmoData"></param>
    public void Receive(bool destructive, params object[] atmoData)
    {
        temp = (float)atmoData[0];
        if (destructive)
        {
            globalPressure = (float)Math.Round((float)atmoData[1],4);
            appliedPressure = globalPressure;
            //Debug.Log(gameObject+" global pressure is "+ globalPressure);
            if (atmoData[2].GetType() == typeof(List<IGas>))
            {
                gasses = (List<IGas>)atmoData[2];
            }
            else if (atmoData[2].GetType() == typeof(IGas))
            {
                gasses.Clear();//
                gasses.Add((IGas)atmoData[2]);
            }
        }
        else
        {
            globalPressure += (float)Math.Round((float)atmoData[1],4);
            appliedPressure = globalPressure;
            if (atmoData[2].GetType() == typeof(List<IGas>))
            {
                gasses.AddRange((List<IGas>)atmoData[2]);
            }
            else if (atmoData[2].GetType() == typeof(IGas))
            {
                gasses.Add((IGas)atmoData[2]);
            }
        }
        
        //calculate the change in globalpressure based temp on volume or temp for local pressure calcs
        float totalPressure = 0.0f;
        //Debug.Log("Pre vol adj: " + globalPressure);
        foreach (IGas gas in gasses)
        {
            ///P1*V1/T1 = P2*V2/T2///
            float p1 = gas.GetLocalPressure();
            float v1 = gas.GetLocalVolume();
            //convert temp to K
            float t1 = ((gas.GetTemp() - 32f) * (5f / 9f)) + 273.15f;
            float p2;
            float v2 = volume_m3;
            //convert to K
            float t2 =  ((temp - 32f) * (5f / 9f)) + 273.15f;
            //Debug.Log("p1: " +p1 + "v1: " +v1+ "t1: " +t1 + "v2: " +v2+ "t2: "+t2);
            p2 = (p1 * v1 * t2) / (t1 * v2);
            //add the partial pressure of this gas to the total pressure in the duct
            //Debug.Log(gameObject+" adj totalPressure is "+totalPressure+" + "+p2);
            totalPressure += p2;

            //update the volume params for each gas in this volume in case this volume is not the same as the volume that passed the gas in.
            gas.SetLocalVolume(volume_m3);
            //Debug.Log("Current Concentration(s): " + gas.GetConcentration());
        }
        //Debug.Log("Total Pressure after vol adj: " + (float)Math.Round(totalPressure,4));
        appliedPressure = (float)Math.Round(totalPressure,4);
        //Debug.Log("Post vol adj: " + globalPressure);
        gasses = CheckGasses(globalPressure);//appliedPressure
    }

    /// <summary>
    /// Try to remove X amount from the provided gas. 
    /// Return the removed gas. Out param is the gas from which amount was taken.
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="gasFrom"></param>
    /// <returns></returns>
    public IGas SubtractGas(float amountPerSecond, IGas gasFrom, out IGas remainder)
    {
        float amount = amountPerSecond * Time.deltaTime;
        IGas returnGas = new IGas(gasFrom.GetIDName(), gasFrom.GetTemp(), 0.0f);
        returnGas.SetLocalPressure(gasFrom.GetLocalPressure());
        returnGas.SetLocalVolume(gasFrom.GetLocalVolume());
        float originalConc = gasFrom.GetConcentration();
        //subtract conc
        float remainingConc = gasFrom.GetConcentration() - amount;
        if(remainingConc > 0)
        {
            gasFrom.SetConcentration(remainingConc);
            returnGas.SetConcentration(amount);
        }
        else
        {
            returnGas.SetConcentration(gasFrom.GetConcentration());
            gasFrom.SetConcentration(0.0f);
        }
        ///NOTE: IMPORTANT!
        ///this pressure loss calculation is banjaxed, and is only used to remove pressure from the main duct
        ///The pressure transfered into the outflow here is 2.5-3.0X below the needed 1.0m3 output to fill the 519m3 test room
        ///to 1.0atm while also having +-100% oxygenation. This is likely due to the input volume being 0.4m3, not 1.0m3.
        ///
        /// Due to this, pressure for the room is calculated INSIDE the room volume, based on wholesale concentration v room volume.
        ///

        float gasFromPressurePercent = gasFrom.GetConcentration() / originalConc;
        gasFrom.SetLocalPressure(gasFrom.GetLocalPressure() * gasFromPressurePercent);
        float returnGasPressurePercent = returnGas.GetConcentration() / originalConc;
        returnGas.SetLocalPressure((returnGas.GetLocalPressure() * returnGasPressurePercent));

        remainder = gasFrom;
        return returnGas;
    }

    /// <summary>
    /// Check the gas pipe's internal gas list for duplicates and combine them. totalPressure is the pipe's global pressure
    /// </summary>
    /// <param name="totalPressure"></param>
    /// <returns></returns>
    public List<IGas> CheckGasses(float totalPressure)
    {
        //Debug.Log(gasses.Count+" gasses in pipe.");
        if(gasses.Count > 1)
        {
            List<IGas> newGassesList = gasses;//new List<IGas>();
            //combine all same gasses
            for (int i = 0; i < newGassesList.Count; i++)
            {
                for (int j = 0; j < newGassesList.Count; j++)
                {
                    if (i != j)
                    {
                        if (newGassesList[i].GetIDName() == newGassesList[j].GetIDName())
                        {
                            IGas EQgas = CombineGases(gasses[i], gasses[j], totalPressure);
                            newGassesList.Remove(gasses[i]);
                            newGassesList.Remove(gasses[j-1]);
                            newGassesList.Add(EQgas);
                        }
                    }
                }
            }
            foreach (IGas gas in newGassesList)
            {
                //Debug.Log("EQM result: "+gas.ToString());
            }
            return newGassesList;
        }
        else
        {
            //gasses is empty or only has one has in it
            return gasses;
        }
    }

    /// <summary>
    /// Equalize the Temperature and add the concentration of the two passed gasses. Then, recalc density.
    /// </summary>
    /// <param name="gasA"></param>
    /// <param name="gasB"></param>
    /// <returns></returns>
    public IGas CombineGases(IGas gasA, IGas gasB, float localPressure)
    {
        float gasTemp;
        float gasConc;
        //float gasPressure;

        float gasAt = gasA.GetTemp();
        float gasBt = gasB.GetTemp();
        gasTemp = (gasAt + gasBt) / 2;
        gasA.SetTemp(gasTemp);

        gasConc = gasA.GetConcentration() + gasB.GetConcentration();
        gasA.SetConcentration(gasConc);

        ///overall pressure is computed seperately
        //float gasAp = gasA.GetLocalPressure();
        //float gasBp = gasB.GetLocalPressure();
        //gasPressure = (gasAp + gasBp);
        //gas pressure and duct pressure are the same
        gasA.SetLocalPressure((float)Math.Round(localPressure,4));
        gasA.CalculateAtmosphericDensity();
        return gasA;
    }

    void Update()
    {
        //Debug.Log("////////////////////////////////////////////////////////////////////////////////////////");
        if (gasses.Count > 0)
        {
            //Debug.Log("PIPE " + this.gameObject.name + " UPDATE BLOCK");

            //when we intro multiple gasses this will need rewritten. Different gasses will equalize temp over time.
            //all will share uniform pressure, volume, etc. However the differences in temp means that every gas will exert
            //differing amounts of partial pressure on the pipe.
            foreach (IGas gas in gasses)
            {
                //Debug.Log("Gas / Pipe Eq, density, and pressure calcs");
                //calculate the new temp for duct and gas
                float ductTemp;
                float gasTemp;
                if (temp < gas.GetTemp())//the gas is hotter than the duct
                {
                    ductTemp = ((gas.GetTemp() + temp) / 2) + (temp * insulationRating); //heat bleeds from the gas into the duct.
                    gasTemp = ((gas.GetTemp() + temp) / 2) - (temp * insulationRating); //heat bleeds from the gas to the duct.
                }
                else if (temp > gas.GetTemp())//the gas is cooler than the duct
                {
                    ductTemp = ((gas.GetTemp() + temp) / 2) - (temp * insulationRating); //insulation to keep heat from bleeding into the pipe.
                    gasTemp = ((gas.GetTemp() + temp) / 2) + (temp * insulationRating); //heating the pipe will not affect the bulk of gas as much.
                }
                else//gas and duct are equal temps
                {
                    ductTemp = ((gas.GetTemp() + temp) / 2);
                    gasTemp = ((gas.GetTemp() + temp) / 2);
                }
                //set the new temp
                gas.SetTemp(gasTemp);
                temp = ductTemp;

                
            }
            
            ///
            /// Maybe make vents (that havn't been breached) one-way? IE air can only flow out into the room. Then, airvents that have
            /// been kicked or busted out will Eq both ways w/out a throttle (1000L/s or whatev)
            ///
            if (vent != null)
            {
                //Play the vent airflow sound from the vent object
                AudioSource ventAud = vent.GetComponentInChildren<AudioSource>();//GetComponent<AudioSource>();
                if (!ventAud.isPlaying)
                {
                    ventAud.Play();
                }
                //Debug.Log("Vent?");
                VolumeAtmosphereController roomVAC = ductVolume.GetComponent<VolumeAtmosphereController>();
                float roomVolume = roomVAC.GetVolume();
                IGas remainder;
                //Vents will fill rooms with IGas Oxygen at 1000L/s. 
                //Room oxygenation will be determined by ratio of oxygen(m3) to roomVolume(m3)
                //Room temp will be set according to the avg gas temp

                //1000Ls in total, composition will be based off of the percent concentration in the duct
                //this is so that hazardous or explosive gasses will circulate into every ventilated room.

                if (roomVAC.GetPressure() < 1.0f)
                {
                    //Debug.Log("Checking room gasses");
                    gasses = CheckGasses(globalPressure);//precautionary check
                    if (gasses.Count > 1)
                    {
                        float totalConc = 0.0f;
                        //calc total concentration
                        foreach (IGas gas in gasses)
                        {
                            totalConc += gas.GetConcentration();
                        }
                        for (int g = 0; g < gasses.Count; g++)
                        {
                            //determine what percentage of all present gasses this single gas accounts for
                            float ratio = gasses[g].GetConcentration() / totalConc;
                            Debug.Log("transfer ratio: " + ratio);
                            //create the gas that will be sent into the room
                            IGas outflowX = SubtractGas(1f, gasses[g], out remainder);
                            gasses[g] = remainder;
                            //the change in pressure of the gas needs reflected to the duct.
                            globalPressure = gasses[g].GetLocalPressure();
                            roomVAC.AddRoomGas(outflowX);
                        }
                    }
                    else
                    {
                        IGas outflow = SubtractGas(1f, gasses[0], out remainder);//gasses[0] will eventually not be oxygen
                        gasses[0] = remainder;
                        //the change in pressure of the gas needs reflected to the duct.
                        globalPressure = gasses[0].GetLocalPressure();
                        //Debug.Log("Outflow: " + outflow.ToString());
                        roomVAC.AddRoomGas(outflow);
                    }
                }
            }

            if (!burst)
            {
                //Debug.Log("TRANSFER");
                float totalVolume = volume_m3;
                float totalPressures = globalPressure;
                float totalConc = 0.0f;
                float totalTemp = temp;

                //get total volume, pressure, conc of all gasses in this and neighbors
                foreach (IGas gass in gasses)
                {
                    totalConc += gass.GetConcentration();
                }
                foreach (IGasPipe pipe in neighbors)
                {
                    totalVolume += pipe.volume_m3;
                    totalPressures += pipe.GetGlobalPressure();
                    totalTemp += pipe.temp;
                    //get total concentration
                    foreach (IGas gas in pipe.gasses)
                    {
                        totalConc += gas.GetConcentration();
                    }
                }
                //Debug.Log("total pressure: " + totalPressures + " between " + (neighbors.Length + 1));
                //Equalize pressure for appliedPressure and globalPressure
                //P1V1T2/V2T1=P2 where V2 is wholesale volume, and P2 is Eq pressure
                //float TEqK = ((temp - 32f) * (5f / 9f)) + 273.15f;
                //float TNeighbors = (((totalTemp/(neighbors.Length + 1)) - 32f) * (5f / 9f)) + 273.15f;
                //float pEq_applied = (totalPressures * volume_m3 * TEqK)/(totalVolume * TNeighbors);
                //Global Pressure Eq calc
                float tEq_global = totalTemp / (neighbors.Length + 1);
                float pEq_global = totalPressures / (neighbors.Length + 1);
                float cEq_global = totalConc / (neighbors.Length + 1);                    
                for (int g = 0; g < neighbors.Length; g++)
                {
                    List<IGas> newGassesList = new List<IGas>();
                    for (int j = 0; j < gasses.Count; j++)
                    {
                        //this gas is the Eq'd gas.
                        IGas tempGas = new IGas(gasses[j].GetIDName(), tEq_global, cEq_global, pEq_global, volume_m3);
                        tempGas.CalculateAtmosphericDensity();
                        newGassesList.Add(tempGas);
                    }
                    object[] newAtmoComp = { tEq_global, pEq_global, newGassesList };
                    //This needs to be limitable by throughput, somehow
                    TransferTo(neighbors[g], newAtmoComp);
                }
            }

            ///*
            if (appliedPressure > maxP)
            {
                Debug.Log("BURST");
                burst = true;
                //pressure = 0.0f;
                VolumeAtmosphereController ductVol = ductVolume.GetComponent<VolumeAtmosphereController>();
                //dump pressure into room (UNTESTED)
                float roomPressure = ductVol.GetPressure();
                float roomVolume = ductVol.GetVolume();
                float volumeratio = volume_m3 / roomVolume;
                roomPressure += (appliedPressure * volumeratio);
                ductVol.SetPressure(roomPressure);
                globalPressure = roomPressure;
                //dump gasses into room
            }
            //*/

            if (temp > tempTol[1] || temp < tempTol[0])
            {
                //melt and explode
                throughput_m3 = 0;//temp
            }
            //if bulletholes
            //yada yada
        }
    }

    public float GetAppliedPressure()
    {
        return appliedPressure;
    }
    public float GetGlobalPressure()
    {
        return globalPressure;
    }

    public void AddNeighbor(IGasPipe neighborDuct)
    {
        for(int i = 0; i < neighbors.Length; i++)
        {
            if (neighbors[i].GetInstanceID() != neighborDuct.GetInstanceID())
            {
                //Debug.Log(gameObject + " is adding duct to neighbor");
                //Debug.Log("Current length: " + neighbors.Length);
                IGasPipe[] neighborTemp = new IGasPipe[neighbors.Length + 1];
                neighbors.CopyTo(neighborTemp, 0);
                neighborTemp[neighbors.Length] = neighborDuct;
                neighbors = neighborTemp;
                //Debug.Log("New Length is: " + neighbors.Length);
            }
            //else
            //{
                //Debug.Log("DUPE!!");
            //}
        }
    }
}