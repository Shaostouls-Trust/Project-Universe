using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectUniverse.PowerSystem {
    public class PowerOutputController : MonoBehaviour
    {
        [SerializeField] private TMP_Text machName;
        [SerializeField] private TMP_Text maxOut;
        [SerializeField] private TMP_Text currentOut;
        [SerializeField] private TMP_Text targetOut;
        [SerializeField] private MachineType type;
        [SerializeField] private IGenerator generator;
        [SerializeField] private IRouter router;
        [SerializeField] private IRoutingSubstation sub;
        [SerializeField] private float increment;
        private float targetOutput = 1024f;
        private float maxOutput = 1024f;
        private float updateTime = 1f;

        public enum MachineType { Generator, Router, Substation }

        public string MachineName
        {
            get { return machName.text; }
            set { machName.text = value; }
        }
        public string CurrentOutput
        {
            get { return currentOut.text; }
            set { currentOut.text = value; }
        }
        public float MaxOutput
        {
            get { return maxOutput; }
            set { maxOutput = value; }
        }
        public float TargetOutput
        {
            get { return targetOutput; }
            set { targetOutput = value; }
        }

        private void Start()
        {
            maxOutput = generator.OutputMax;
            maxOut.text = ""+maxOutput;
            TargetOutput = maxOutput;
            targetOut.text = ""+TargetOutput;
        }

        public void UpdateMachineUI()
        {
            if(updateTime <= 0f)
            {
                updateTime = 1f;
                if (type == MachineType.Generator)
                {
                    if (generator != null)
                    {
                        CurrentOutput = "" + Mathf.Round(generator.LastOutput);
                    }
                }
                else if (type == MachineType.Router)
                {
                    if (router != null)
                    {

                    }
                }
                else
                {
                    if (sub != null)
                    {

                    }
                }
            }
            else
            {
                updateTime -= Time.deltaTime;
            }
        }

        public void LowerTargetOutput()
        {
            if(TargetOutput > 0f)
            {
                TargetOutput -= increment;
                targetOut.text = "" + TargetOutput;
            }
            
        }
        public void RaiseTargetOutput()
        {
            if (TargetOutput < MaxOutput)
            {
                TargetOutput += increment;
                targetOut.text = "" + TargetOutput;
            }
        }

        public void ExternalInteractFunc(int param)
        {
            if(param == 0)
            {
                RaiseTargetOutput();
            }
            else
            {
                LowerTargetOutput();
            }
        }
    }
}