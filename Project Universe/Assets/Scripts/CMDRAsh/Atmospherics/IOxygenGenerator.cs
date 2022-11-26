using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using ProjectUniverse.Environment.Volumes;
using ProjectUniverse.PowerSystem;

/// <summary>
/// vars:
/// oxygen rate - rate of oxygen gen per U(nit of time)
/// IGasPipe[] - Array of all pipes being fed by this generator
/// </summary>
namespace ProjectUniverse.Environment.Gas
{
    public sealed class IOxygenGenerator : MonoBehaviour
    {
        //rate in m3 for oxygen generation and pressurization
        [SerializeField] private float oxyGenRate_m3s;
        [SerializeField] private IGasPipe[] connectedPipes;
        [SerializeField] private Volume myVolume;
        //[SerializeField] private LinkedList<IGasPipe>[] outputs;
        private VolumeAtmosphereController genRoom;
        private bool isRunning;
        private bool isPowered;
        //private bool thisRunMachine;
        // Start is called before the first frame update
        void Start()
        {
            //thisRunMachine = GetComponent<IMachine>().RunMachine;
            //Recurse the paths of the ends of the air duct branch... though I don't thing that'd do anything?
            genRoom = myVolume.gameObject.GetComponent<VolumeAtmosphereController>();
        }

        // Update is called once per frame
        void Update()
        {
            if (GetComponent<IMachine>().RunMachine)
            {
                isRunning = true;
            }
            else
            {
                isRunning = false;
            }

            if (isPowered && isRunning)
            {

                if (genRoom.Oxygenation < 1.0f)
                {
                    genRoom.Oxygenation += (oxyGenRate_m3s * Time.deltaTime);
                }
                //transfer air into the linked airvents
                float oxyGenRateThisUpdate = 0.0f;
                for (int i = 0; i < connectedPipes.Length; i++)
                {
                    if (connectedPipes[i].GlobalPressure < 1.0)
                    {
                        oxyGenRateThisUpdate = (oxyGenRate_m3s * Time.deltaTime);
                    }
                    else
                    {
                        float lerpF = Mathf.Lerp(0f, 1.05f, connectedPipes[i].GlobalPressure);//rateAdjuster);
                                                                                                   //Debug.Log("0-1.025 range lerp by global pressure: "+lerpF);
                                                                                                   //Lerp the rate by the pressure
                        oxyGenRateThisUpdate = Mathf.Lerp((oxyGenRate_m3s * Time.deltaTime), 0.0f, lerpF);
                        //Debug.Log("OxyGen Rate - 0 lerp by lerpF: "+oxyGenRateThisUpdate);
                    }
                    ///
                    /// Is creating this gas at these conditions part of the problem? 3.0m3 and 1.0atm in 0.4m3?
                    ///
                    List<IGas> gasList = new List<IGas>();
                    IGas gasTest = new IGas("Oxygen", 60.0f, (float)Math.Round(oxyGenRateThisUpdate, 3), 1.1f, 0.4f);
                    gasTest.CalculateAtmosphericDensity();
                    gasList.Add(gasTest);
                    //Debug.Log(gasTest.ToString());
                    object[] atmoDatas = { gasTest.GetTemp(), gasTest.GetLocalPressure(), gasList };
                    if (connectedPipes[i].GlobalPressure < 1.1)
                    {
                        //Debug.Log("Generating: " + gasTest.ToString());
                        connectedPipes[i].Receive(false, 25f, 1.1f, gasList, gasTest.GetTemp());
                    }
                }
            }
        }

        public void RunMachine(int powerLevel)
        {
            //Debug.Log("Receive message");
            switch (powerLevel)
            {
                case 0:
                    SetPoweredState(true);
                    SetRunningState(true);
                    break;
                case 1:
                    SetPoweredState(true);
                    SetRunningState(true);
                    break;
                case 2:
                    SetPoweredState(true);
                    SetRunningState(true);
                    break;
                case 3:
                    SetPoweredState(true);
                    SetRunningState(true);
                    break;
                case 4:
                    SetPoweredState(false);
                    SetRunningState(false);
                    break;
                case 5:
                    SetPoweredState(true);
                    SetRunningState(false);
                    break;
            }
        }

        public void SetPoweredState(bool value)
        {
            isPowered = value;
        }
        public void SetRunningState(bool value)
        {
            isRunning = value;
        }
    }
}