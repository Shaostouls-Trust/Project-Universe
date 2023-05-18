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
        [Space]
        [SerializeField] private GameObject machineStateButtonPrefab;
        [SerializeField] private GameObject substationStateButtonPrefab;
        [SerializeField] private GameObject machinePanel;
        [SerializeField] private GameObject breakerPanel;
        [SerializeField] private GameObject substationPanel;
        [SerializeField] private TMP_Text buffer;
        [SerializeField] private TMP_Text bufferMax;
        private List<GameObject> machineStateButtons = new List<GameObject>();
        private List<GameObject> breakerStateButtons = new List<GameObject>();
        private List<GameObject> substationStateButtons = new List<GameObject>();
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
            if(generator != null)
            {
                maxOutput = generator.OutputMax;
                maxOut.text = "" + maxOutput;
                TargetOutput = maxOutput;
                targetOut.text = "" + TargetOutput;
            }
        }
        
        public void ClearMachineButtons()
        {
            foreach (GameObject button in machineStateButtons)
            {
                Destroy(button);
            }
            machineStateButtons.Clear();
        }
        public void ClearBreakerButtons()
        {
            foreach (GameObject button in breakerStateButtons)
            {
                Destroy(button);
            }
            breakerStateButtons.Clear();
        }
        public void ClearRouterButtons()
        {
            foreach (GameObject button in substationStateButtons)
            {
                Destroy(button);
            }
            substationStateButtons.Clear();
        }

        public void CreateButton(IMachine mach)
        {
            GameObject button = Instantiate(machineStateButtonPrefab, machinePanel.transform);
            button.GetComponent<MachineStateButtonController>().SetData(mach);
            machineStateButtons.Add(button);
        }
        
        public void CreateButton(IBreakerBox box)
        {
            GameObject button = Instantiate(machineStateButtonPrefab, breakerPanel.transform);
            button.GetComponent<MachineStateButtonController>().SetData(box);
            breakerStateButtons.Add(button);
        }

        public void CreateButton(IRoutingSubstation router)
        {
            GameObject button = Instantiate(substationStateButtonPrefab, substationPanel.transform);
            button.GetComponent<SubstationStateButtonController>().SetData(router);
            substationStateButtons.Add(button);
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
                        buffer.text = "" + Math.Round(router.BufferCurrent, 2);
                        bufferMax.text = "" + Math.Round(sub.BufferMax, 2);
                    }
                }
                else
                {
                    if (sub != null)
                    {
                        buffer.text = "" + Math.Round(sub.BufferCurrent,2);
                        bufferMax.text = "" + Math.Round(sub.BufferMax, 2);
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

        /// <summary>
        /// Generator throttling
        /// </summary>
        /// <param name="param"></param>
        public void ExternalInteractFunc(int param)
        {
            if(param == 0)
            {
                RaiseTargetOutput();
            }
            else if(param == 1)
            {
                LowerTargetOutput();
            }
        }
    }
}