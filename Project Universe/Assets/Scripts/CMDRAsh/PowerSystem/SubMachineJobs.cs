using Unity.Mathematics;
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
using static ProjectUniverse.PowerSystem.SubMachineJobs;

namespace ProjectUniverse.PowerSystem
{
    public class SubMachineJobs : MonoBehaviour
    {
        private JobHandle handle;
        private List<ISubMachine> subs = new List<ISubMachine>();
        private List<GCHandle> gcHandles = new List<GCHandle>(); // Better handle management
        // A follow-up response got rid of gcHandles for some reason??

        // Pre-allocated persistent arrays
        private NativeArray<SubMachineData> machineData;
        private NativeArray<SubMachineResults> results;
        private bool arraysInitialized = false;

        // Struct to reduce data copying
        [System.Serializable]
        public struct SubMachineData
        {
            public float requiredEnergy;
            public float bufferCurrent;
            public float energyBuffer;
            public float lastEnergyReceived;
            public float timer;
            public int percentDrawToFill;
            public int legsReceived;
            public int legsRequired;
            public bool runMachine;
        }

        [System.Serializable]
        public struct SubMachineResults
        {
            public int state;
            public float netReqEng;
            public float netAskEng;
            public float newBuffer;
            public float newTimer;
            public bool powered;
        }

        void Update()
        {
            if (subs.Count == 0) return;

            // Initialize arrays only once or when count changes
            if (!arraysInitialized || machineData.Length != subs.Count)
            {
                InitializeArrays();
            }

            // Populate input data - THIS IS KEY: We need the current buffer state
            for (int i = 0; i < subs.Count; i++)
            {
                var sub = subs[i];
                machineData[i] = new SubMachineData
                {
                    requiredEnergy = sub.RequiredEnergy,
                    bufferCurrent = sub.BufferCurrent, // This gets the buffer AFTER energy was received
                    energyBuffer = sub.EnergyBuffer,
                    lastEnergyReceived = sub.LastEnergyReceived,
                    timer = sub.Timer,
                    percentDrawToFill = sub.PercentDrawToFill,
                    legsReceived = sub.LegsReceived,
                    legsRequired = sub.LegsRequired,
                    runMachine = sub.RunMachine
                };
            }

            // Schedule job
            var jobData = new SubMachineJob
            {
                inputData = machineData,
                results = results
            };

            int batchSize = Mathf.Max(1, subs.Count / (JobsUtility.JobWorkerCount * 2));
            handle = jobData.Schedule(subs.Count, batchSize);
        }

        private void InitializeArrays()
        {
            if (arraysInitialized)
            {
                machineData.Dispose();
                results.Dispose();
            }

            machineData = new NativeArray<SubMachineData>(subs.Count, Allocator.Persistent);
            results = new NativeArray<SubMachineResults>(subs.Count, Allocator.Persistent);
            arraysInitialized = true;
        }

        private void LateUpdate()
        {
            if (subs.Count == 0) return;

            handle.Complete();

            // Apply results
            for (int i = 0; i < subs.Count; i++)
            {
                var result = results[i];
                var sub = subs[i];

                // Apply the job results first
                sub.SetJobResults(result);

                // THEN request power for next frame (this will fill the buffer via ReceiveEnergyAmount)
                if (result.netAskEng > 0f && sub.RunMachine)
                {
                    RequestPowerForMachine(sub, result.netAskEng);
                }
            }
        }

        private void RequestPowerForMachine(ISubMachine sub, float energyRequest)
        {
            var breakers = sub.Breakers;
            if (breakers.Count > 0)
            {
                float requestPerBreaker = energyRequest / breakers.Count;
                for (int i = 0; i < breakers.Count; i++)
                {
                    breakers[i].RequestPowerFromBreaker(requestPerBreaker, sub);
                }
            }
        }

        public unsafe void AddMachine(ISubMachine sub)
        {
            subs.Add(sub);

            // Better GC handle management
            GCHandle handle = GCHandle.Alloc(sub, GCHandleType.Pinned);
            gcHandles.Add(handle);

            // Mark arrays for reinitialization
            arraysInitialized = false;
        }

        private void OnDestroy()
        {
            handle.Complete();

            // Clean up GC handles
            foreach (var gcHandle in gcHandles)
            {
                if (gcHandle.IsAllocated)
                    gcHandle.Free();
            }
            gcHandles.Clear();

            // Dispose arrays
            if (arraysInitialized)
            {
                if (machineData.IsCreated) machineData.Dispose();
                if (results.IsCreated) results.Dispose();
            }
        }
    }

    [BurstCompile]
    public struct SubMachineJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<SubMachineData> inputData;
        public NativeArray<SubMachineResults> results;

        public void Execute(int index)
        {
            var data = inputData[index];
            var result = new SubMachineResults();

            // Timer update
            result.newTimer = data.timer - 1f;
            if (result.newTimer < 0f)
                result.newTimer = 7f;

            if (!data.runMachine)
            {
                result.state = 5;
                result.netAskEng = 0f;
                result.newBuffer = data.bufferCurrent;
                results[index] = result;
                return;
            }

            // Calculate energy requirements (matching original logic)
            result.netReqEng = data.requiredEnergy;
            float floatDrawToFill = (float)data.percentDrawToFill;
            float drawToFill = data.requiredEnergy + (data.requiredEnergy * (floatDrawToFill / 100f));

            // Start with current buffer
            result.newBuffer = data.bufferCurrent;

            // Buffer request logic (matching original)
            if (data.bufferCurrent < data.energyBuffer)
            {
                float deficit = data.energyBuffer - data.bufferCurrent;
                if (deficit >= drawToFill)
                {
                    result.netAskEng = drawToFill;
                }
                else if (deficit < drawToFill && deficit > data.requiredEnergy)
                {
                    result.netAskEng = deficit + data.requiredEnergy;
                }
                else
                {
                    result.netAskEng = data.requiredEnergy;
                }
            }
            else
            {
                result.netAskEng = data.requiredEnergy;
                result.newBuffer = data.energyBuffer; // Cap at max
            }

            // Run logic (matching original)
            if (data.legsReceived == data.legsRequired)
            {
                if (data.bufferCurrent > 0f)
                {
                    result.powered = true;

                    if (data.bufferCurrent - data.requiredEnergy < 0f) // Not enough for full power
                    {
                        if (data.bufferCurrent >= data.requiredEnergy * 0.75f)
                        {
                            result.state = 1; // 75% power
                        }
                        else if (data.bufferCurrent >= data.requiredEnergy * 0.5f)
                        {
                            result.state = 2; // 50% power
                        }
                        else
                        {
                            result.state = 3; // <50% power
                        }
                        result.newBuffer = 0f; // Buffer emptied
                    }
                    else
                    {
                        result.state = 0; // Full power
                        result.newBuffer = data.bufferCurrent - data.requiredEnergy;
                        if (result.newBuffer < 0f)
                            result.newBuffer = 0f;
                    }
                }
                else
                {
                    result.powered = false;
                    result.state = 4; // No power
                    result.newBuffer = 0f;
                }
            }
            else if (data.legsReceived < data.legsRequired && data.legsReceived >= 1)
            {
                result.state = 4; // Insufficient legs (with damage potential)
                result.newBuffer = data.bufferCurrent; // Keep buffer
            }
            else
            {
                result.state = 4; // No legs
                result.newBuffer = data.bufferCurrent; // Keep buffer
            }

            results[index] = result;
        }
    }
}