using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
namespace Artngame.Orion.ProceduralPlanets
{
    public class CamTest : MonoBehaviour
    {

        public bool orient;
        public bool setTimestep;
        public float physicsStep = 0.2f;
        public Vector3 initial;

        void Start()
        {
            GetComponent<Rigidbody>().linearVelocity = initial;
            if (setTimestep)
            {

            }
        }

        void FixedUpdate()
        {
            Debug.Log(GetComponent<Rigidbody>().linearVelocity);
            GetComponent<Rigidbody>().position += GetComponent<Rigidbody>().linearVelocity * Time.deltaTime;
        }

    }
}