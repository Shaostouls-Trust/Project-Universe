using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

public class VolumeAtmosphereJobs : IJob
{
    ///
    /// Volume Gas Pipe Section Updates
    /// 
    public void Execute()
    {
        /*
          for(int i = 0; i < volumeGasPipeSections.Count; i++)
            {
                List<IGasPipe> sectionList = volumeGasPipeSections[i].GasPipesInSection;
                List<IGasPipe> equalizeList = new List<IGasPipe>();
                for (int j = 0; j < sectionList.Count; j++)
                {
                    //check the status of every pipe - if a pipe is burst, do not equalize it
                    // or the pipes after it
                    if (!sectionList[j].IsBurst)
                    {
                        equalizeList.Add(sectionList[j]);
                    }
                    else
                    {
                        equalizeList.Add(sectionList[j]);
                        break;
                    }
                }

                float totalPressures = equalizeList[0].GlobalPressure;
                float totalConc = 0.0f;
                float totalTemp = equalizeList[0].Temperature;

                //get total volume, pressure, conc of all gasses in this and neighbors
                foreach (IGas gass in equalizeList[0].Gasses)
                {
                    totalConc += gass.GetConcentration();
                }

                // Skip the first duct
                for (int j = 1; j < equalizeList.Count; j++)
                {
                    IGasPipe pipe = equalizeList[j];
                    pipe.TempEQWithDuct();
                    if(pipe.Vent != null && pipe.Gasses.Count > 0)
                    {
                        pipe.VentToVolume();
                    }

                    totalPressures += pipe.GlobalPressure;
                    totalTemp += pipe.Temperature;
                    //get total concentration
                    foreach (IGas gas in pipe.Gasses)
                    {
                        totalConc += gas.GetConcentration();
                    }
                }
                //Global Pressure Eq calc
                float tEq_global = totalTemp / (equalizeList.Count);
                float pEq_global = totalPressures / (equalizeList.Count);
                float cEq_global = totalConc / (equalizeList.Count);

                // Skip the first duct
                for (int j = 1; j < equalizeList.Count; j++)
                {
                    List<IGas> newGassesList = new List<IGas>();
                    for (int u = 0; u < equalizeList[0].Gasses.Count; u++)
                    {
                        //this gas is the Eq'd gas.
                        IGas tempGas = new IGas(equalizeList[0].Gasses[u].GetIDName(),
                            tEq_global, cEq_global, pEq_global, equalizeList[0].Volume);
                        tempGas.CalculateAtmosphericDensity();
                        newGassesList.Add(tempGas);
                    }
                    object[] newAtmoComp = { tEq_global, pEq_global, newGassesList };
                    //This needs to be limitable by throughput, somehow?
                    //first duct TransferTo(other ducts, newAtmoComp)
                    equalizeList[0].TransferTo(equalizeList[j], newAtmoComp);
                }
            }
        */
    }
}
