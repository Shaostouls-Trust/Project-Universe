using UnityEngine;
using System.Collections;
using ProjectUniverse.Environment.Chemistry;
using ProjectUniverse.Environment.Volumes;
namespace ProjectUniverse.Environment.Hazards
{
    public enum IgnitionSourceType
    {
        Stationary,
        Mobile // NYI
    }

    public class IgnitionSource
    {
        public int ID { get; private set; }
        public IgnitionSourceType Type { get; private set; }
        public Vector3 Position { get; set; }
        public float Temperature { get; private set; } // Current temperature in Celsius
        public float InitialTemperature { get; private set; }
        public float CoolingRate { get; private set; } // Degrees per second
        public float AutocombustionThreshold { get; private set; }
        public float CreationTime { get; private set; }
        public float Duration { get; private set; } // Time until cooled enough to disappear
        public bool IsActive { get; set; }
        public float IgnitionRadius { get; private set; }
        public VolumeAtmosphereController AssignedRoom { get; set; }

        private bool isExtinguished = false;//B

        public IgnitionSource(int id, IgnitionSourceType type, Vector3 position,
            float initialTemp, float coolingRate, float duration, float ignitionRadius = 2f)
        {
            ID = id;
            Type = type;
            Position = position;
            Temperature = initialTemp;
            InitialTemperature = initialTemp;
            CoolingRate = coolingRate;
            Duration = duration;
            AutocombustionThreshold = 450f; // Default autocombustion temp
            CreationTime = Time.time;
            IsActive = true;
            IgnitionRadius = ignitionRadius;
            AssignedRoom = null;
        }

        public void Update(float deltaTime)
        {
            Temperature = Mathf.Max(0f, Temperature - CoolingRate * deltaTime);

            if (Time.time - CreationTime >= Duration || Temperature < AutocombustionThreshold)
            {
                IsActive = false;
            }
        }

        public bool CanIgnite()
        {
            return IsActive && Temperature >= AutocombustionThreshold;
        }
        //B
        public void ExtinguishByWater()
        {
            if (isExtinguished) return;

            isExtinguished = true;
            IsActive = false;
            Temperature = 20f; // Cool to room temperature instantly
        }
        //B
        public bool IsExtinguished()
        {
            return isExtinguished;
        }
    }
}