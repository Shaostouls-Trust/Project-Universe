using ProjectUniverse.Animation.Controllers;
using ProjectUniverse.Environment.Fluid;
using ProjectUniverse.Environment.Gas;
using ProjectUniverse.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;

namespace ProjectUniverse.Environment.Volumes
{
    public class VolumeAtmosphereJobs : MonoBehaviour
    {
        List<VolumeAtmosphereController> vacs = new List<VolumeAtmosphereController>();
        //get instanceIDs for all VACs and the gameobject reference
        JobHandle handle;
        //grab the memmory pointers of the volumes
        [DeallocateOnJobCompletion]
        NativeArray<IntPtr> ptrArray;
        List<IntPtr> intPtrs = new List<IntPtr>();

        public void AddVolume(VolumeAtmosphereController vac)
        {
            try
            {
                GCHandle handle = GCHandle.Alloc(vac, GCHandleType.Pinned);
                IntPtr pointer = (IntPtr)GCHandle.ToIntPtr(handle);
                intPtrs.Add(pointer);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            vacs.Add(vac);
        }

        /// <summary>
        /// When the game is running and new VACs are introduced outside of main start, 
        /// we will need to regenerate the reference list
        /// </summary>
        private void Update()
        {
            //set the last deltatime
            Utils.LastDeltaTime = Time.deltaTime;
        }


        private void FixedUpdate()
        {
            // Use half the available worker threads, clamped to a minimum of 1 worker thread
            int numBatches = Math.Max(1, JobsUtility.JobWorkerCount / 2);

            ptrArray = new NativeArray<IntPtr>(intPtrs.Count, Allocator.TempJob);
            for(int i = 0; i < vacs.Count; i++)
            {
                ptrArray[i] = intPtrs[i];
            }

            AtmosphereUpdateJob jobData = new AtmosphereUpdateJob
            {
                volRefs = ptrArray
            };
            handle = jobData.Schedule(vacs.Count, numBatches);
            
        }

        private void LateUpdate()
        {
            handle.Complete();
            //here, the nativearrays are disposed automatically
        }

        //[BurstCompile]
        public struct AtmosphereUpdateJob : IJobParallelFor
        {
            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<IntPtr> volRefs;

            ///
            /// Volume Gas Pipe Section Updates
            /// 
            public void Execute(int index)
            {
                ///
                /// Room temp will slowly drop to -200f over time without the addition of heat through radiators.
                /// Radiators will heat the room according to how open the radiator valve is.
                /// Larger rooms heat and cool more slowly b/c room gasses will must heat and cool as well.
                /// 
                //RoomHeatAmbiLoss();
                //there is no point running this until reactor radiators are set up for the ship

                ///UnityEngine.Profiling.Profiler.BeginSample("Volume Equalization");
                //combine all same gasses in the volume
                //if (roomGases.Count > 1)
                //{
                //    roomGases = CheckGasses(false,0.0f);
                //}
                //check for the surround volumes
                //bool[] doorstates = DoorStates();

                GCHandle h = GCHandle.FromIntPtr((IntPtr)volRefs[index]);
                VolumeAtmosphereController thisvac = (VolumeAtmosphereController)h.Target;
                //VolumeAtmosphereController thisvac = theseVacs[index];
                if (thisvac.Flood)
                {
                    IFluid tWat = new IFluid("water", 80f, 0.2f);
                    thisvac.AddRoomFluid(tWat);
                    //render control is not showing/hiding water plane
                    //hall to control is not hiding water
                }

                ///
                /// Volume Gas Pipe Section Updates
                /// 
                for (int i = 0; i < thisvac.VolumeGasPipeSections.Count; i++)
                {
                    List<IGasPipe> sectionList = thisvac.VolumeGasPipeSections[i].GasPipesInSection;
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

                    thisvac.GasPipeSectionEqualization(equalizeList, true);

                    ///
                    /// Duct has burst. Begin venting contents into volume.
                    /// The contents of the duct after venting must be equal to the ambient atmo.
                    /// Ambient atmo will be transfered to connected volumes.
                    ///
                    List<IGasPipe> ventList = new List<IGasPipe>();
                    for (int j = 0; j < sectionList.Count; j++)
                    {
                        bool compiled = false;
                        IGasPipe burstPipe = sectionList[j];
                        if (burstPipe.GlobalPressure > burstPipe.MaxPressure)
                        {
                            burstPipe.IsBurst = true;
                        }
                        if (burstPipe.IsBurst)
                        {
                            //Dump contents into volume
                            foreach (IGas gas in burstPipe.Gasses)
                            {
                                thisvac.AddRoomGas(gas);
                            }

                            // Equalize the duct gasses with the volume
                            float volumeratio = burstPipe.Volume / thisvac.RoomVolume;
                            burstPipe.Gasses.Clear();
                            List<IGas> roomGasses = thisvac.RoomGasses;
                            for (int g = 0; g < thisvac.RoomGasses.Count; g++)
                            {
                                IGas gas = roomGasses[g];
                                gas.SetLocalPressure(thisvac.Pressure);
                                gas.SetConcentration(roomGasses[g].GetConcentration() * volumeratio);
                                burstPipe.Gasses.Add(gas);
                            }
                            burstPipe.Temperature = thisvac.Temperature;

                            //Transfer these contents to the ducts after the breach.
                            // If the duct is in equalizeList then go in the other direction
                            // Compile this list for only the first burst in the section
                            if (!compiled)
                            {
                                compiled = true;
                                int q = 0;
                                for (int p = 0; p < sectionList.Count; p++)
                                {
                                    if (sectionList[p] == burstPipe)
                                    {
                                        q = p;
                                    }
                                }
                                for (; q < sectionList.Count; q++)
                                {
                                    ventList.Add(sectionList[q]);
                                }
                            }
                        }
                    }
                    if (ventList.Count > 0)
                    {
                        thisvac.GasPipeSectionEqualization(ventList, false);
                    }

                    //if (temp > tempTol[1] || temp < tempTol[0])
                    //{
                    //melt and explode
                    //    throughput_m3 = 0;//temp
                    //}

                    //if bulletholes
                    //yada yada
                }
                ///Profiler.EndSample();
            }
        }
    }
}