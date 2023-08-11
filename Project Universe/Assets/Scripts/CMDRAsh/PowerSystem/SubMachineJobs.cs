using ProjectUniverse.Environment.Fluid;
using ProjectUniverse.Environment.Gas;
using ProjectUniverse.PowerSystem;
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
using static Unity.Collections.AllocatorManager;

namespace ProjectUniverse.PowerSystem
{
    public class SubMachineJobs : MonoBehaviour
    {
        JobHandle handle;
        //list of pointers to each submachine
        //private Dictionary<ISubMachine, IntPtr> machinePointers = new Dictionary<ISubMachine, IntPtr>();
        List<ISubMachine> subs = new List<ISubMachine>();

        //get/set data
        NativeArray<int> state;
        NativeArray<float> netReqEng;
        NativeArray<float> netAskEng;
        NativeArray<float> lastEng;
        NativeArray<float> buffer;
        NativeArray<bool> powered;

        NativeArray<IntPtr> ptrArray;
        List<IntPtr> intPtrs = new List<IntPtr>();
        NativeArray<bool> runMachine;
        NativeArray<float> timers;
        NativeArray<int> legsReceived;
        NativeArray<int> legsRequired;

        public unsafe void AddMachine(ISubMachine sub)
        {
            try
            {
                GCHandle handle = GCHandle.Alloc(sub, GCHandleType.Pinned);
                IntPtr pointer = (IntPtr)GCHandle.ToIntPtr(handle);
                intPtrs.Add(pointer);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            subs.Add(sub);
        }

        // Update is called once per frame
        void Update()
        {
            // Use half the available worker threads, clamped to a minimum of 1 worker thread
            int numBatches = Math.Max(1, JobsUtility.JobWorkerCount / 2);

            //intPtrs.Clear();
            //get the memory pointers for the machines
            //for (int p = 0; p < subs.Count; p++)
            //{
            //    GCHandle handle = GCHandle.Alloc(subs[p], GCHandleType.Pinned);
            //    IntPtr pointer = (IntPtr)GCHandle.ToIntPtr(handle);
            //    intPtrs.Add(pointer);
            //}

            state = new NativeArray<int>(subs.Count, Allocator.TempJob);//machinePointers
            netReqEng = new NativeArray<float>(subs.Count, Allocator.TempJob);
            netAskEng = new NativeArray<float>(subs.Count, Allocator.TempJob);
            lastEng = new NativeArray<float>(subs.Count, Allocator.TempJob);
            buffer = new NativeArray<float>(subs.Count, Allocator.TempJob);
            powered = new NativeArray<bool>(subs.Count, Allocator.TempJob);
            runMachine = new NativeArray<bool>(subs.Count, Allocator.TempJob);
            timers = new NativeArray<float>(subs.Count, Allocator.TempJob);
            legsReceived = new NativeArray<int>(subs.Count, Allocator.TempJob);
            legsRequired = new NativeArray<int>(subs.Count, Allocator.TempJob);
            //
            ptrArray = new NativeArray<IntPtr>(subs.Count, Allocator.TempJob);

            int idx = 0;
            foreach (ISubMachine sub in subs)//KeyValuePair<ISubMachine, IntPtr> kvp in machinePointers
            {
                state[idx] = 0;
                netReqEng[idx] = 0f;
                netAskEng[idx] = 0f;
                lastEng[idx] = sub.LastEnergyReceived;
                buffer[idx] = 0f;
                powered[idx] = false;
                runMachine[idx] = sub.RunMachine;
                timers[idx] = sub.Timer;
                legsReceived[idx] = sub.LegsReceived;
                legsRequired[idx] = sub.LegsRequired;
                //
                //try
                //{
                    ptrArray[idx] = intPtrs[idx];
                //}
                //catch (Exception e)
                //{
                //    Debug.Log(e);
                //}

                idx++;
            }

            SubMachineJob jobData = new SubMachineJob
            {
                state = state,
                netReqEng = netReqEng,
                netAskEng = netAskEng,
                lastEng = lastEng,
                buffer = buffer,
                powered = powered,
                runMachine = runMachine,
                timers = timers,
                legsReceived = legsReceived,
                legsRequired = legsRequired,
                ptrArray = ptrArray
            };

            handle = jobData.Schedule(ptrArray.Length, numBatches);
        }

        /// <summary>
        /// Execute the submachine code in parallel tasks
        /// 
        /// Returns: The type and state of the machine for main thread behavior
        /// 
        /// </summary>
        //[BurstCompile]
        public struct SubMachineJob : IJobParallelFor
        {
            //return the type and state of machine for run logic
            public NativeArray<int> state;
            public NativeArray<float> netReqEng;
            public NativeArray<float> netAskEng;
            public NativeArray<float> lastEng;
            public NativeArray<float> buffer;
            public NativeArray<bool> powered;
            public NativeArray<bool> runMachine;
            public NativeArray<float> timers;
            public NativeArray<int> legsReceived;
            public NativeArray<int> legsRequired;

            //memory pointers to access managed types
            public NativeArray<IntPtr> ptrArray;

            public unsafe void Execute(int index)
            {
                ISubMachine sub = null;
                //try
                //{
                    //get the machine from the pointer
                    GCHandle h = GCHandle.FromIntPtr((IntPtr)ptrArray[index]);
                    sub = (ISubMachine)h.Target;
                //}
                //catch (Exception e) { JobLogger.LogError(e); }
                //return to unBursted code
                if (sub != null)
                {
                    if (runMachine[index])//sub.RunMachine
                    {
                        //instance variables
                        float requiredEnergy = sub.RequiredEnergy;
                        buffer[index] = sub.BufferCurrent;
                        float energyBuffer = sub.EnergyBuffer;
                        //reset requestedEnergy
                        netReqEng[index] = requiredEnergy;
                        //Recalculate drawToFill based on draw percent
                        float floatDrawToFill = (float)sub.PercentDrawToFill;
                        float drawToFill = requiredEnergy + (requiredEnergy * (floatDrawToFill / 100)); //105% or 110% draw
                                                                                                        //If the energy buffer is not full
                        if (buffer[index] < energyBuffer)
                        {
                            //Get the deficit between the energybuffer(max) and the current buffer amount
                            float deficit = energyBuffer - buffer[index];
                            if (deficit >= drawToFill)
                            {
                                //send energy request
                                netAskEng[index] = drawToFill;
                                RequestHelper(sub, index);//sub, 

                            }
                            else if (deficit < drawToFill && deficit > requiredEnergy)
                            {
                                netAskEng[index] = deficit + netReqEng[index];
                                //Debug.Log(this.gameObject.name + " Request Helper");
                                RequestHelper(sub, index);
                            }
                            else
                            {
                                netAskEng[index] = requiredEnergy;
                                //Debug.Log(this.gameObject.name + " Request Helper");
                                RequestHelper(sub, index);
                            }
                            //if (buffer[index] < 0f)
                            //{
                            //    buffer[index] = 0f;
                            //}
                        }
                        else if (buffer[index] >= energyBuffer)
                        {
                            //send request
                            netAskEng[index] = requiredEnergy;
                            buffer[index] = energyBuffer;
                            //Debug.Log(this.gameObject.name + " Request Helper");
                            RequestHelper(sub, index);
                        }
                        //update the buffer from the submachine
                        buffer[index] = sub.BufferCurrent;

                        //run machines
                        //Debug.Log("Running "+this.gameObject.name);
                        RunLogic(index);
                    }
                    else
                    {
                        buffer[index] = sub.BufferCurrent;
                        //turn the machine off
                        netAskEng[index] = 0f;
                        lastEng[index] = 0f;
                        RunLogic(index);
                    }

                }
            }

            [BurstCompile]
            public void RunLogic(int index)//ISubMachine sub, 
            {
                timers[index]--;//sub.Timer
                if (timers[index] < 0f)
                {
                    timers[index] = 7f;
                }
                if (runMachine[index])//sub.RunMachine
                {
                    if (legsReceived[index] == legsRequired[index])
                    {
                        //Debug.Log("Legs received");
                        if (buffer[index] > 0f)
                        {
                            powered[index] = true;
                            if (buffer[index] - netReqEng[index] < 0.0f)//not enough power to run at full
                            {
                                if (buffer[index] >= netReqEng[index] * 0.75f)//75% power
                                {
                                    ///return type and int from job to handle this in main thread
                                    //any slower locks emiss to blinking yellow.
                                    state[index] = 1;
                                }
                                else if (buffer[index] >= netReqEng[index] * 0.5f)//no lower than 50%
                                {
                                    state[index] = 2;
                                }
                                else//lower than 50%
                                {
                                    state[index] = 3;
                                }
                                //no matter what, the buffer is emptied
                                //set this index of the buffer to the negative of itself
                                buffer[index] = 0f;
                                //bufferCurrent = 0.0f;
                            }
                            else
                            {
                                //run full power
                                state[index] = 0;
                                //bufferCurrent -= requiredEnergy;
                                //set this buffer to the required energy
                                //JobLogger.Log("from " + buffer[index]);
                                buffer[index] -= netReqEng[index];
                                //JobLogger.Log("to "+buffer[index]);
                                if (buffer[index] <= 0f)
                                {
                                    buffer[index] = 0f;
                                }
                            }
                        }
                        else
                        {
                            powered[index] = false;
                            //isPowered = false;
                            //'run' at 0 power
                            state[index] = 4;
                        }
                    }
                    else if (legsReceived[index] < legsRequired[index] && legsReceived[index] >= 1)
                    {
                        //Shut down machine due to leg requirement
                        state[index] = 4;
                        //electrical damage (if the buffer is not empty)
                        //if (bufferCurrent > 0)
                        //{
                        //NYI
                        //}
                    }
                    else
                    {
                        //Shut down machine due to leg requirement
                        state[index] = 4;
                        //NO electrical damage, because no legs attached.
                    }
                }
                else
                {
                    state[index] = 5;
                }
            }

            // Causes netcode errors - remove net stuff
            public void RequestHelper(ISubMachine sub, int index)// 
            {
                if (runMachine[index]) //sub.RunMachine
                {
                    foreach (IBreakerBox box in sub.Breakers)
                    {
                        //JobLogger.Log("request from breakers: "+ netAskEng[index] / sub.Breakers.Count);
                        box.RequestPowerFromBreaker(netAskEng[index] / sub.Breakers.Count, sub);
                    }
                }
                else
                {
                    netAskEng[index] = 0f;
                }
            }
        }

        private unsafe void LateUpdate()
        {
            handle.Complete();

            //set data and update network variables
            //reconstruct ISubMachine from Ptr to ensure it's the right machine
            for (int i = 0; i < ptrArray.Length; i++)
            {
                try
                {
                    GCHandle h = GCHandle.FromIntPtr((IntPtr)ptrArray[i]);
                    ISubMachine sub = (ISubMachine)h.Target;
                    float b = netAskEng[i];
                    float d = buffer[i];
                    bool e = powered[i];
                    float f = timers[i];
                    int g = state[i];
                    sub.SetData(b, d, e, f, g);
                }
                catch(Exception e)
                {
                    Debug.Log(e);
                    //JobLogger.Log(netReqEng[i] + " " + netAskEng[i] + " " + lastEng[i] + " " + buffer[i] + " " + powered[i] + " " + timers[i]);   
                }
            }

            //for (int i = 0; i < ptrArray.Length; i++)
            //{
            //    GCHandle.FromIntPtr((IntPtr)ptrArray[i]).Free();
            //}

            state.Dispose();
            netReqEng.Dispose();
            netAskEng.Dispose();
            lastEng.Dispose();
            buffer.Dispose();
            powered.Dispose();
            runMachine.Dispose();
            timers.Dispose();
            legsReceived.Dispose();
            legsRequired.Dispose();
            ptrArray.Dispose();
        }

        private unsafe void OnDestroy()
        {
            try
            {
                for (int i = 0; i < ptrArray.Length; i++)
                {
                    GCHandle.FromIntPtr((IntPtr)ptrArray[i]).Free();
                }
            }
            catch (Exception e) { }
            try
            {
                ptrArray.Dispose();
            }
            catch (Exception e) { }
            try
            {
                state.Dispose();
            }
            catch (Exception e) { }
            try
            {
                netReqEng.Dispose();
            }
            catch (Exception e) { }
            try
            {
                netAskEng.Dispose();
            }
            catch (Exception e) { }
            try
            {
                lastEng.Dispose();
            }
            catch (Exception e) { }
            try
            {
                buffer.Dispose();
            }
            catch (Exception e) { }
            try
            {
                powered.Dispose();
            }
            catch (Exception e) { }
            try
            {
                runMachine.Dispose();
            }
            catch (Exception e) { }
            try
            {
                timers.Dispose();
            }
            catch (Exception e) { }
            try
            {
                legsReceived.Dispose();
            }
            catch (Exception e) { }
            try
            {
                legsRequired.Dispose();
            }
            catch (Exception e) { }
        }
    }
}