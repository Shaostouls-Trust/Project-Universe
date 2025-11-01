using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using ProjectUniverse.Environment.Gas;
using NUnit.Framework;
using static UnityEngine.Rendering.LineRendering;
using ProjectUniverse.Environment.Hazards;
using ProjectUniverse.Environment.Chemistry;

namespace ProjectUniverse.PowerSystem.CollisionDemo
{
    // Simple fire environmental threat
    public class DemoFire : MonoBehaviour
    {
        [Header("Fire Properties")]
        public float damageRadius = 3f;
        public float damagePerSecond = 20f;
        public float burnDuration = 30f;
        public AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

        [Header("Fuel Consumption")]
        public float fuelConsumptionRate = 0.01f; // m³ per second
        public float oxygenConsumptionRate = 0.02f; // m³ per second
        public float minimumOxygenConcentration = 0.15f; // 15% oxygen needed

        [Header("Heat Spread")]
        public float heatSpreadRate = 0.5f;
        public float maxHeatRadius = 5f;
        public float heatOutputPerSecond = 50000f; // Joules

        [Header("Ignition")]
        [SerializeField] private float ignitionTriggerInterval = 2f; // Trigger ignition every 2 seconds
        [SerializeField] private float ignitionDuration = 3f; // Each ignition lasts 3 seconds

        [Header("Implicit Fuel")]
        [SerializeField] private float implicitFuelRemaining = 100f; // Abstract fuel units
        [SerializeField] private float implicitFuelConsumptionRate = 1f; // Units per second
        private bool isUsingImplicitFuel = false;

        [Header("Visual")]
        public ParticleSystem fireParticles;
        public Light fireLight;

        [Header("Water Interaction")]
        private bool isExtinguished = false;

        private EnvironmentalThreatManager envManager;
        private Environment.Volumes.VolumeAtmosphereController atmosphereController;
        private Environment.Chemistry.RoomReactionManager reactionManager;
        private int threatId;
        private float startTime;
        private bool isActive = true;
        private float updateInterval = 0.5f;
        private float timeSinceUpdate = 0f;
        private float timeSinceIgnition = 0f;

        public void Initialize(Environment.Volumes.VolumeAtmosphereController atmosphere)
        {
            atmosphereController = atmosphere;
            if (atmosphere != null)
            {
                reactionManager = atmosphere.GetComponent<Environment.Chemistry.RoomReactionManager>();
            }
        }

        void Start()
        {
            envManager = FindObjectOfType<EnvironmentalThreatManager>();

            // Only check for room if we weren't initialized with one (not spawned by HazardIntegrationManager)
            if (atmosphereController == null)
            {
                var allRooms = FindObjectsOfType<Environment.Volumes.VolumeAtmosphereController>();
                foreach (var room in allRooms)
                {
                    if (IsPointInRoom(transform.position, room))
                    {
                        atmosphereController = room;
                        reactionManager = room.GetComponent<Environment.Chemistry.RoomReactionManager>();
                        break;
                    }
                }

                // If no room found, we're outside - burn implicitly
                if (atmosphereController == null)
                {
                    isUsingImplicitFuel = true;
                }
            }

            if (envManager != null)
            {
                threatId = envManager.RegisterEnvironmentalThreat(this);
                startTime = Time.time;
                StartCoroutine(FireLifecycle());
            }
        }

        private bool IsPointInRoom(Vector3 point, Environment.Volumes.VolumeAtmosphereController room)
        {
            foreach (var section in room.RoomVolumeSections)
            {
                if (section != null && section.bounds.Contains(point))
                    return true;
            }
            return false;
        }

        void Update()
        {
            if (!isActive) return;

            //B
            // Check if submerged in water
            var hazardManager = HazardIntegrationManager.Instance;
            if (hazardManager != null)
            {
                if (hazardManager.IsPointSubmergedInWater(transform.position, out var submergingVolume))
                {
                    ExtinguishByWater();
                    return;
                }
            }

            timeSinceUpdate += Time.deltaTime;
            timeSinceIgnition += Time.deltaTime;

            if (timeSinceUpdate >= updateInterval)
            {
                // Check if we should switch to implicit fuel
                if (!isUsingImplicitFuel && atmosphereController != null)
                {
                    if (!HasAtmosphericFuel())
                    {
                        isUsingImplicitFuel = true;
                    }
                }

                if (isUsingImplicitFuel)
                {
                    ConsumeImplicitFuel(timeSinceUpdate);
                }
                else
                {
                    ConsumeFuelAndOxygen(timeSinceUpdate);
                }

                GenerateHeat(timeSinceUpdate);
                ProduceCombustionProducts(timeSinceUpdate);
                timeSinceUpdate = 0f;
            }

            // Periodically trigger ignition in the room (only if in a room)
            if (atmosphereController != null && timeSinceIgnition >= ignitionTriggerInterval)
            {
                TriggerRoomIgnition();
                timeSinceIgnition = 0f;
            }
        }

        private bool HasAtmosphericFuel()
        {
            if (atmosphereController == null) return false;

            foreach (var gas in atmosphereController.RoomGassesLegacy)
            {
                if (ChemistryDatabase.IsCombustible(gas.GetIDName()) && gas.GetConcentration() > 0.0001f)
                {
                    return true;
                }
            }
            return false;
        }

        private void ConsumeImplicitFuel(float deltaTime)
        {
            float intensity = GetCurrentIntensity();
            float consumed = implicitFuelConsumptionRate * intensity * deltaTime;
            implicitFuelRemaining = Mathf.Max(0f, implicitFuelRemaining - consumed);

            if (implicitFuelRemaining <= 0f)
            {
                burnDuration = Mathf.Min(burnDuration, (Time.time - startTime) + 2f);
            }

            // Check for oxidizer even with implicit fuel
            if (atmosphereController != null)
            {
                float oxygenConcentration = GetOxygenConcentration();
                if (oxygenConcentration < minimumOxygenConcentration)
                {
                    burnDuration = Mathf.Min(burnDuration, (Time.time - startTime) + 1f);
                    return;
                }

                // Consume oxygen
                var oxygen = atmosphereController.RoomGassesLegacy.Find(g => g.GetIDName() == "Oxygen");
                if (oxygen != null)
                {
                    float oxygenToConsume = consumed * 0.02f; // Implicit fuel needs oxygen too
                    var oxygenToRemove = new IGas("Oxygen", oxygen.GetTemp(),
                        oxygenToConsume, oxygen.GetLocalPressure(), atmosphereController.RoomVolume);
                    atmosphereController.RemoveRoomGas(oxygenToRemove);
                }
            }
        }

        private void TriggerRoomIgnition()
        {
            if (reactionManager == null) return;

            float intensity = GetCurrentIntensity();
            if (intensity > 0.3f) // Only trigger if fire is reasonably intense
            {
                reactionManager.TriggerIgnition(ignitionDuration);
            }
        }

        private void ConsumeFuelAndOxygen(float deltaTime)
        {
            if (atmosphereController == null) return;

            float intensity = GetCurrentIntensity();
            float actualFuelConsumption = fuelConsumptionRate * intensity * deltaTime;
            float actualOxygenConsumption = oxygenConsumptionRate * intensity * deltaTime;

            // Check oxygen availability
            float oxygenConcentration = GetOxygenConcentration();
            if (oxygenConcentration < minimumOxygenConcentration)
            {
                burnDuration = Mathf.Min(burnDuration, (Time.time - startTime) + 1f);
                return;
            }

            // Consume oxygen
            var oxygen = atmosphereController.RoomGassesLegacy.Find(g => g.GetIDName() == "oxygen");
            if (oxygen != null && oxygen.GetConcentration() > 0.0001f)
            {
                float toConsume = Mathf.Min(actualOxygenConsumption, oxygen.GetConcentration());
                var oxygenToRemove = new IGas("oxygen", oxygen.GetTemp(),
                    toConsume, oxygen.GetLocalPressure(), atmosphereController.RoomVolume);
                atmosphereController.RemoveRoomGas(oxygenToRemove);
            }
            else
            {
                burnDuration = Mathf.Min(burnDuration, (Time.time - startTime) + 0.5f);
                return;
            }

            // Consume atmospheric combustible gases using centralized list
            float fuelRemaining = actualFuelConsumption;

            foreach (var gas in atmosphereController.RoomGassesLegacy)
            {
                if (fuelRemaining <= 0f) break;

                if (ChemistryDatabase.IsCombustible(gas.GetIDName()) && gas.GetConcentration() > 0.0001f)
                {
                    float toConsume = Mathf.Min(fuelRemaining, gas.GetConcentration());
                    var fuelToRemove = new IGas(gas.GetIDName(), gas.GetTemp(),
                        toConsume, gas.GetLocalPressure(), atmosphereController.RoomVolume);
                    atmosphereController.RemoveRoomGas(fuelToRemove);
                    fuelRemaining -= toConsume;
                }
            }
        }

        //B
        public void ExtinguishByWater()
        {
            if (isExtinguished) return;

            isExtinguished = true;
            isActive = false;

            // Produce steam when water hits fire
            if (atmosphereController != null)
            {
                float intensity = GetCurrentIntensity();
                float steamVolume = intensity * 0.5f; // Water turning to steam

                var steam = new IGas("steam", 100f, steamVolume,
                    atmosphereController.Pressure, atmosphereController.RoomVolume);
                atmosphereController.AddRoomGas(steam);

                // Add some heat from the extinguishing process
                atmosphereController.AddRoomHeat(heatOutputPerSecond * intensity * 0.1f, true);
            }

            // Unregister from environmental manager
            if (envManager != null)
            {
                envManager.UnregisterThreat(threatId);
            }

            // Stop particles immediately
            if (fireParticles)
            {
                fireParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            // Fade out light quickly
            if (fireLight)
            {
                StartCoroutine(QuickFadeLight());
            }
            else
            {
                Destroy(gameObject, 0.1f);
            }
        }
        //B
        private IEnumerator QuickFadeLight()
        {
            if (fireLight == null)
            {
                Destroy(gameObject);
                yield break;
            }

            float originalIntensity = fireLight.intensity;
            float fadeTime = 0.1f; // Quick fade for water extinguishing
            float elapsed = 0f;

            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                fireLight.intensity = originalIntensity * (1f - elapsed / fadeTime);
                yield return null;
            }

            Destroy(gameObject);
        }

        private float GetOxygenConcentration()
        {
            if (atmosphereController == null) return 0.21f; // Assume normal air outside

            float totalGasVolume = 0f;
            float oxygenVolume = 0f;

            foreach (var gas in atmosphereController.RoomGassesLegacy)
            {
                float conc = gas.GetConcentration();
                totalGasVolume += conc;
                if (gas.GetIDName() == "Oxygen")
                    oxygenVolume = conc;
            }

            return totalGasVolume > 0f ? oxygenVolume / totalGasVolume : 0f;
        }

        private void GenerateHeat(float deltaTime)
        {
            if (atmosphereController == null) return;

            float intensity = GetCurrentIntensity();
            float heatGenerated = heatOutputPerSecond * intensity * deltaTime;
            atmosphereController.AddRoomHeat(heatGenerated, true);
        }

        private void ProduceCombustionProducts(float deltaTime)
        {
            if (atmosphereController == null) return;

            float intensity = GetCurrentIntensity();
            float fuelConsumed = isUsingImplicitFuel ?
                implicitFuelConsumptionRate * intensity * deltaTime :
                fuelConsumptionRate * intensity * deltaTime;

            // Standard combustion products
            float co2Volume = fuelConsumed * 0.8f;
            float h2oVolume = fuelConsumed * 0.6f;

            var co2 = new IGas("CO2", atmosphereController.Temperature, co2Volume,
                atmosphereController.Pressure, atmosphereController.RoomVolume);
            atmosphereController.AddRoomGas(co2);

            var h2o = new IGas("H2O", atmosphereController.Temperature, h2oVolume,
                atmosphereController.Pressure, atmosphereController.RoomVolume);
            atmosphereController.AddRoomGas(h2o);

            // Standard soot production (dirty by definition)
            float sootAmount = fuelConsumed * 5f;
            atmosphereController.AddParticulate("soot", sootAmount);

            // Metal oxides if burning metals
            if (isUsingImplicitFuel)
            {
                atmosphereController.AddParticulate("metal_oxide", sootAmount * 0.3f);
            }
        }

        IEnumerator FireLifecycle()
        {
            yield return new WaitForSeconds(burnDuration);

            isActive = false;

            if (envManager != null)
            {
                envManager.UnregisterThreat(threatId);
            }

            if (fireParticles) fireParticles.Stop();
            if (fireLight)
            {
                float originalIntensity = fireLight.intensity;
                float fadeTime = 2f;
                float elapsed = 0f;

                while (elapsed < fadeTime)
                {
                    elapsed += Time.deltaTime;
                    fireLight.intensity = originalIntensity * (1f - elapsed / fadeTime);
                    yield return null;
                }
            }

            Destroy(gameObject);
        }

        public float GetCurrentIntensity()
        {
            if (!isActive) return 0f;

            float normalizedTime = (Time.time - startTime) / burnDuration;
            return intensityCurve.Evaluate(normalizedTime);
        }

        public float GetCurrentDamageRadius()
        {
            return damageRadius * GetCurrentIntensity();
        }

        public float GetCurrentHeatRadius()
        {
            return maxHeatRadius * GetCurrentIntensity();
        }

        public float GetCurrentDamagePerSecond()
        {
            return damagePerSecond * GetCurrentIntensity();
        }

        void OnDestroy()
        {
            if (envManager != null && isActive)//&& !isExtinguished
            {
                envManager.UnregisterThreat(threatId);
            }

            // Decrement room fire count
            if (atmosphereController != null)
            {
                var hazardManager = FindObjectOfType<HazardIntegrationManager>();
                if (hazardManager != null)
                {
                    hazardManager.DecrementRoomFireCount(atmosphereController);
                }
            }
        }

        void OnDrawGizmos()
        {
            if (!isActive) return;

            // Draw damage radius
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, GetCurrentDamageRadius());

            // Draw heat radius
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, GetCurrentHeatRadius());
        }
    }
}