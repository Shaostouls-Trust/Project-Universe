using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ProjectUniverse.PowerSystem.Nuclear;
using UnityEngine.UI;
using System;

namespace ProjectUniverse.UI
{

    public class FuelRodUIController : MonoBehaviour
    {
        [SerializeField] private TMP_Text mass;
        [SerializeField] private TMP_Text temp;
        [SerializeField] private TMP_Text deltaTemp;
        [SerializeField] private TMP_Text activity;
        [SerializeField] private TMP_Text index;
        [SerializeField] private NuclearCore core;
        private NuclearFuelRod rod;
        private int[] rodIndex = new int[2];
        private int[] bounds = new int[2];//down and right dims
        [SerializeField] private Image icon;
        private float timer = 0.1f;

        private void Start()
        {
            bounds[0] = core.mdnfrRows.Length - 1;
            bounds[1] = core.mdnfrRows[0].fuelRodsCol.Length - 1;
        }

        // Update is called once per frame
        private void OnGUI()
        {
            timer -= Time.deltaTime;
            if (timer <= 0f) 
            {
                timer = 0.1f;
                rod = core.NFRMatrix[rodIndex[0], rodIndex[1]];
                if (rod != null)
                {
                    mass.text = Math.Round(rod.FuelMass / 1000f, 1) + " Kg";
                    temp.text = Math.Round(rod.RodCoreTemp, 1) + " K";
                    deltaTemp.text = Math.Round(rod.DeltaTemp, 2) + " K/c";
                    activity.text = Math.Round(rod.AppliedNetActivity, 2) + " n/s";
                }
                else
                {
                    mass.text = "";
                    temp.text = "";
                    deltaTemp.text = "";
                    activity.text = "";
                }
            } 
        }

        public void ExternalInteractFunc(int i)
        {
            if(i == 0)
            {
                //up
                rodIndex[0] -= 1;
                if(rodIndex[0] < 0)
                {
                    rodIndex[0] = 0;
                }
            }
            else if (i == 1)
            {
                //down
                rodIndex[0] += 1;
                if (rodIndex[0] > bounds[0])
                {
                    rodIndex[0] = bounds[0];
                }
            }
            else if (i == 2)
            {
                //left
                rodIndex[1] -= 1;
                if (rodIndex[1] < 0)
                {
                    rodIndex[1] = 0;
                }
            }
            else if (i == 3)
            {
                //right
                rodIndex[1] += 1;
                if (rodIndex[1] > bounds[1])
                {
                    rodIndex[1] = bounds[1];
                }
            }
            index.text = rodIndex[0] + ":" + rodIndex[1];
            //move icon to index
            float x = 0.0f;//.015
            float y = -0.17f;//-0.18
            x += 0.25f * (rodIndex[1]+1);//per l<>r
            y -= 0.25f * (rodIndex[0]); 
            icon.transform.localPosition = new Vector3(x, y, 0);
        }
    }
}