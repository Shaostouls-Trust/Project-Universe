﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Netcode;
using ProjectUniverse.Environment.Radiation;
using ProjectUniverse.Audio;

namespace ProjectUniverse.PowerSystem
{
    public sealed class IGenerator : NetworkBehaviour
    {
        [SerializeField] private int outputMax;//duh
        [SerializeField] private int generatorLevel;
        public string powerGrid;//grid that this generator is part of
        [SerializeField] private float lastOutput;//OutputCurrent resets every update, so we never see it
        private float outputCurrent;//duh
        [SerializeField] private int maxRouters; //level * 4
        [SerializeField] private IRouter[] routers;
        [SerializeField] private bool leaking;
        [SerializeField] private PowerOutputController outputController;
        [SerializeField] private IRadiationZone radZone;
        [SerializeField] private AlarmSoundController leakAlarm;
        private Guid guid;
        private LinkedList<ICable> iCableDLL = new LinkedList<ICable>();
        private float[] requestedRouterPower;
        private IGenerator myGenerator;

        //leg update
        private int legsOut;//calculate based on machines linked
        [SerializeField]
        private int availibleLegsOut;

        //MLAPI
        private NetworkVariable<int> netOutputMax = new NetworkVariable<int>();//new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone }
        private NetworkVariable<int> netLegsOut = new NetworkVariable<int>();

        public int OutputMax
        {
            get { return outputMax; }
            set { outputMax = value; }
        }
        public float LastOutput
        {
            get { return lastOutput; }
            set { lastOutput = value; }
        }
        public bool Leaking
        {
            get { return leaking; }
            set { leaking = value; }
        }

        // Start is called before the first frame update
        void Start()
        {
            NetworkListeners();
            //create GUID
            //guid = Guid.NewGuid();
            myGenerator = this.gameObject.GetComponent<IGenerator>();
            ProxyStart();
        }

        public override void OnNetworkSpawn()
        {
            //set starting values
            netOutputMax.Value = outputMax;
            base.OnNetworkSpawn();
        }

        private void NetworkListeners()
        {
            
            //netLegsOut.Value = routers.Length * 3;
            //set up events
            netOutputMax.OnValueChanged += delegate { outputMax = netOutputMax.Value; };
            netLegsOut.OnValueChanged += delegate { legsOut = netLegsOut.Value; };
        }

        public void ProxyStart()
        {
            if (routers.Length > maxRouters)
            {
                IRouter[] routTemp = new IRouter[maxRouters];
                for (int i = 0; i < maxRouters; i++)
                {
                    routTemp[i] = routers[i];
                }
                routers = routTemp;
            }
            outputCurrent = 0f;
            //look for routers based on max amount (level * 4)
            for (int i = 0; i < routers.Length; i++)
            {
                if (this.routers[i] != null)
                {
                    //create an ICable node to add to the iCableDLL
                    ICable myIcable = new ICable(this, this.routers[i]);
                    //add it to the end of the DLL, if alone, it's first and last.
                    iCableDLL.AddLast(myIcable);
                    Debug.Log("Checking Router connections");
                    routers[i].CheckMachineState(ref myGenerator);
                }
            }
            netLegsOut.Value = routers.Length * 3;
        }

        // Update is called once per frame
        void Update()
        {
            availibleLegsOut = legsOut;//needs to stick to the hard value, not x3
            outputCurrent = 0f;
            //get leg states - this will be for when we have levers that close off indiv legs.
            //NYI

            // if leaking, release radiation
            if (radZone != null)
            {
                if (leaking)
                {
                    float amt = lastOutput / outputMax;
                    radZone.GeneratorLeakMultiplier = amt;
                    if(leakAlarm != null)
                    {
                        if (lastOutput > 0f)
                        {
                            leakAlarm.active = true;
                        }
                        else
                        {
                            leakAlarm.active = false;
                        }
                    }
                }
                else
                {
                    if (leakAlarm != null)
                    {
                        leakAlarm.active = false;
                    }
                    radZone.GeneratorLeakMultiplier = 0f;
                }
            }
        }

        public void RequestPowerFromGenerator(float requestedAmount, IRouter thisRouter)
        {
            float targetMax;
            if (outputController != null)
            {
                if(outputMax < outputController.TargetOutput)
                {
                    targetMax = outputMax;
                }
                else
                {
                    targetMax = outputController.TargetOutput;
                }
            }
            else
            {
                targetMax = outputMax;
            }
            //transfer power
            foreach (ICable cable in iCableDLL)
            {
                if (cable.route == thisRouter)
                {
                    //get subst's leg req
                    int routerLegReq = cable.route.GetLegRequirement();
                    //if something has happened, and we don't have as many legs as we need.
                    if (routerLegReq > availibleLegsOut)
                    {
                        //we will temporarily change the required leg count to what we can provide
                        routerLegReq = availibleLegsOut;
                    }
                    //split power between legs
                    float[] powerAmount = new float[routerLegReq];
                    for (int l = 0; l < routerLegReq; l++)
                    {
                        powerAmount[l] = requestedAmount / routerLegReq;
                    }

                    if (outputCurrent + requestedAmount <= targetMax)//outputMax
                    {
                        //transfer as much power as is needed, up until capacity is met.
                        availibleLegsOut -= routerLegReq;
                        outputCurrent += requestedAmount;
                        cable.TransferIn(routerLegReq, powerAmount, 1);
                    }
                    else
                    {
                        //(outputMax - outputCurrent)/ 3f, (outputMax - outputCurrent) / 3f, (outputMax - outputCurrent) / 3f
                        float[] tempfloat = new float[] { (targetMax - outputCurrent)/ 3f, (targetMax - outputCurrent) / 3f, (targetMax - outputCurrent) / 3f };
                        //Debug.Log("gen out: " + (outputMax - outputCurrent) 
                        //+ " or " + ((outputMax - outputCurrent) / 3f) + "+" + ((outputMax - outputCurrent) / 3f) 
                        //+ "+" + ((outputMax - outputCurrent) / 3f));
                        outputCurrent = targetMax;//outputMax
                        cable.TransferIn(routerLegReq, tempfloat, 1);
                    }
                }
            }
            LastOutput = outputCurrent;
            if(outputController != null)
            {
                outputController.UpdateMachineUI();
            }
            
        }

        public void SetRouters(IRouter[] newRouters)
        {
            routers = newRouters;
        }

        public Boolean CheckConnection()
        {
            return true;
        }

        public Guid GetGUID()
        {
            return guid;
        }
    }
}