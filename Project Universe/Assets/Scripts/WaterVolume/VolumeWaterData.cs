using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Rendering;
using ProjectUniverse.Environment.Fluids;
using ProjectUniverse.Util;
using ProjectUniverse.Environment.Volumes;
using ProjectUniverse.PowerSystem.CollisionDemo;
using ProjectUniverse.Environment.Hazards;

// Water management component for individual volumes
[System.Serializable]
public class VolumeWaterData : MonoBehaviour
{
    [Header("Water Properties")]
    //private Dictionary<string, IFluid> fluids = new Dictionary<string, IFluid>();
    private IFluid fluid; // Singular fluid for now
    //public float waterLevel = 0f; // Current water height from volume bottom
    private float maxWaterCapacity = 1000f; // Maximum water this volume can hold
    private bool hasWater = false;

    [Header("Volume Properties")]
    private float volumeFloorHeight; // Y position of volume floor
    private Vector3 volumeSize; // Size of the volume
    private float crossSectionalArea; // Area for water level calculations
    [SerializeField] private float minimumFluidLevel = 0.001f;
    [SerializeField] private WaterHeightVisManager waterVisGO;

    [Header("Directly Connected Volumes")]
    public List<VolumeWaterData> connectedVolumes = new List<VolumeWaterData>();

    [Header("Fire Suppression")]
    [SerializeField] private float fireCheckInterval = 0.5f; // Check every 0.5 seconds
    private float timeSinceFireCheck = 0f;
    private VolumeAtmosphereController cachedAtmosphereController;
    private bool atmosphereControllerChecked = false;

    private Volume volumeComponent;
    private BoxCollider colliderComponent;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;

    //B
    private VolumeAtmosphereController AtmosphereController
    {
        get
        {
            if (!atmosphereControllerChecked)
            {
                cachedAtmosphereController = GetComponent<VolumeAtmosphereController>();
                atmosphereControllerChecked = true;
            }
            return cachedAtmosphereController;
        }
    }


    public Vector3 VolumeSize
    {
        get { return volumeSize; }
    }

    public float CrossSectionalArea
    {
        get { return crossSectionalArea; }
    }

    public float MaxWaterCapacity
    {
        get { return maxWaterCapacity; }
    }

    public bool HasWater
    {
        get { return hasWater; }
        set { hasWater = value; }
    }

    public float VolumeFloorHeight
    {
        get { return volumeFloorHeight; }
    }

    public WaterHeightVisManager WaterVisGO
    {
        get { return waterVisGO; }
    }

    //public Dictionary<string, IFluid> Fluids
    //{
    //    get { return fluids; }
    //}
    public IFluid Fluid
    {
        get { return fluid; }
    }

    void Awake()
    {
        volumeComponent = GetComponent<Volume>();
        colliderComponent = GetComponent<BoxCollider>();
        //fluids = new Dictionary<string, IFluid>();
        if (volumeComponent != null)
        {
            CalculateVolumeProperties();
        }
    }

    void CalculateVolumeProperties()
    {
        // Get volume bounds
        Bounds bounds = colliderComponent.bounds;
        volumeSize = bounds.size;
        volumeFloorHeight = bounds.min.y;
        crossSectionalArea = volumeSize.x * volumeSize.z;
        maxWaterCapacity = crossSectionalArea * volumeSize.y;
    }

    //B
    void Update()
    {
        if (hasWater)
        {
            timeSinceFireCheck += Time.deltaTime;
            if (timeSinceFireCheck >= fireCheckInterval)
            {
                CheckAndExtinguishSubmergedHazards();
                timeSinceFireCheck = 0f;
            }
        }
    }
    //B
    private void CheckAndExtinguishSubmergedHazards()
    {
        if (!hasWater) return;

        float currentWaterHeight = GetAbsoluteWaterHeight();
        Bounds bounds = colliderComponent.bounds;

        // Create a bounds for the water-filled portion
        Vector3 waterBoundsCenter = new Vector3(
            bounds.center.x,
            (volumeFloorHeight + currentWaterHeight) * 0.5f,
            bounds.center.z
        );

        Vector3 waterBoundsSize = new Vector3(
            bounds.size.x,
            currentWaterHeight - volumeFloorHeight,
            bounds.size.z
        );

        // Only check if there's significant water
        if (waterBoundsSize.y < 0.1f) return;

        // Find all fires in the water volume
        Collider[] colliders = Physics.OverlapBox(
            waterBoundsCenter,
            waterBoundsSize * 0.5f,
            Quaternion.identity
        );

        foreach (var col in colliders)
        {
            // Check for DemoFire
            var fire = col.GetComponentInParent<ProjectUniverse.PowerSystem.CollisionDemo.DemoFire>();
            if (fire != null && fire.transform.position.y <= currentWaterHeight)
            {
                ExtinguishFire(fire);
            }
        }

        // Check ignition sources through the HazardIntegrationManager
        ExtinguishSubmergedIgnitionSources(currentWaterHeight);
    }
    //B
    private void ExtinguishFire(ProjectUniverse.PowerSystem.CollisionDemo.DemoFire fire)
    {
        if (fire == null) return;

        fire.ExtinguishByWater();

        if (enableDebugLogs)
        {
            Debug.Log($"Water in {gameObject.name} extinguished fire at {fire.transform.position}");
        }
    }
    //B
    private void ExtinguishSubmergedIgnitionSources(float waterHeight)
    {
        var hazardManager = ProjectUniverse.Environment.Hazards.HazardIntegrationManager.Instance;
        if (hazardManager == null) return;

        // Get atmosphere controller to identify our room
        var atmosphere = AtmosphereController;
        if (atmosphere == null) return;

        hazardManager.ExtinguishIgnitionSourcesInVolumeByWater(atmosphere, waterHeight, colliderComponent.bounds);
    }

    public float GetWaterVolume()
    {
        //return waterLevel * crossSectionalArea;
        float total = 0f;
        //foreach (var fluid in fluids.Values)
        //{
        //    total += fluid.GetConcentration();
        //}
        if (fluid != null)
        {
            total += fluid.GetConcentration();
            return total;
        }
        else { return 0f; }
    }

    public float GetWaterLevel()
    {
        return GetWaterVolume() / crossSectionalArea;
    }

    public void AddFluid(IFluid fluidVolumeToAdd)
    {
        string fluidID = fluidVolumeToAdd.GetIDName();
        // Guaranteed to contain only Water
        if (fluid == null)
        {
            IFluid newFluid = new IFluid(fluidVolumeToAdd);
            fluid = newFluid;
        }
        else
        {
            //Cut out MixFluids
            //MixFluids(fluid, fluidVolumeToAdd);
            fluid = Utils.CombineFluids(fluid, fluidVolumeToAdd, 1f);
        }

        /*if (fluids.ContainsKey(fluidID))
        {
            // Mix with existing fluid
            MixFluids(fluids[fluidID], fluidVolumeToAdd);
        }
        else
        {
            // Add new fluid type
            IFluid newFluid = new IFluid(fluidVolumeToAdd);
            newFluid.SetConcentration(fluidVolumeToAdd.GetConcentration());
            fluids[fluidID] = newFluid;
        }*/

        UpdateFluidState();
    }

    public IFluid RemoveFluid(string fluidID, float volume)
    {
        /*if (fluids.ContainsKey(fluidID))
        {
            float currentVolume = fluids[fluidID].GetConcentration();
            float newVolume = Mathf.Max(0f, currentVolume - volume);

            if (newVolume <= minimumFluidLevel)
            {
                fluids.Remove(fluidID);
            }
            else
            {
                fluids[fluidID].SetConcentration(newVolume);
            }
        }
        else
        {
            return null;
        }*/

        if (fluid != null)
        {
            float currentVolume = fluid.GetConcentration();
            float temp = fluid.GetTemp();
            float newVolume = Mathf.Max(0f, currentVolume - volume);

            //if (newVolume <= minimumFluidLevel)
            //{
            //    ClearAllFluids();
            //}
            //else
            //{
                fluid.SetConcentration(newVolume);
            //}

            UpdateFluidState();

            return new IFluid(fluidID, temp, volume);
        }
        else
        {
            return null;
        }
    }

    public void SetWaterVolume(float volume)
    {
        //waterLevel = Mathf.Clamp(volume / crossSectionalArea, 0f, volumeSize.y);
        if (fluid != null)
        {
            fluid.SetConcentration(volume);
        }
        else
        {
            //Preferably we don't do this here, because it assumes the properties of the water
            fluid = new IFluid("Water", 70f, volume);
        }
        UpdateFluidState();
        //hasWater = waterLevel > minimumFluidLevel;
        //if (waterVisGO != null)
        //{
        //    waterVisGO.UpdateWaterLevel(GetWaterLevel() / volumeSize.y);
        //}
    }

    private void MixFluids(IFluid existingFluid, IFluid newFluid)
    {
        float existingVolume = existingFluid.GetConcentration();
        float volToAdd = newFluid.GetConcentration();
        float totalVolume = existingVolume + volToAdd;

        if (totalVolume > 0)
        {
            // Weighted average for temperature
            float mixedTemp = (existingFluid.GetTemp() * existingVolume + newFluid.GetTemp() * volToAdd) / totalVolume;

            // Weighted average for pressure
            float mixedPressure = (existingFluid.GetLocalPressure() * existingVolume + newFluid.GetLocalPressure() * volToAdd) / totalVolume;

            existingFluid.SetTemp(mixedTemp);
            existingFluid.SetLocalPressure(mixedPressure);
            existingFluid.SetConcentration(totalVolume);
        }
    }

    private void UpdateFluidState()
    {
        float fluidVolume = GetWaterVolume();
        hasWater = fluidVolume > minimumFluidLevel;
        //waterLevel = GetWaterLevel();

        if (waterVisGO != null)
        {
            waterVisGO.UpdateWaterLevel(GetWaterLevel() / volumeSize.y);
        }
        //if (!hasWater)
        //{
        //    ClearAllFluids();
        //}
    }

    public WaterHeightVisManager AssignNewWaterVisManager(GameObject visPrefab, BoxCollider VolumeCollider)
    {
        if (waterVisGO == null)
        {
            waterVisGO = Instantiate(visPrefab,transform).GetComponent<WaterHeightVisManager>();
            waterVisGO.ProxyStart(VolumeCollider);
        }
        return waterVisGO;
    }

    public float GetAbsoluteWaterHeight()
    {
        return volumeFloorHeight + GetWaterLevel();
    }

    public bool CanReceiveWater()
    {
        return GetWaterVolume() < maxWaterCapacity;// waterLevel < volumeSize.y;
    }

    //public List<string> GetFluidTypes()
    //{
    //    return new List<string>(fluids.Keys);
    //}

    //public void ClearAllFluids()
    //{
    //    //fluids.Clear();
    //    fluid = null;
    //}
}