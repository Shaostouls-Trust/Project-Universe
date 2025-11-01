using NUnit.Framework;
using ProjectUniverse.Environment.Gas;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Fan : MonoBehaviour, IGasContainer
{
    [SerializeField] private FanPort inlet;
    [SerializeField] private FanPort outlet;

    [SerializeField] private List<IGas> gases = new();
    [SerializeField] private float pressure = 1f;
    [SerializeField] private float basePressureBoost = 0.5f;
    [SerializeField] private float baseFlowRate = 10f;

    [SerializeField] private GameObject[] externalAffectors = new GameObject[0];
    private IGasEffect[] cachedEffects;

    private bool isRunning;
    private bool effectsNeedRefresh = true;

    #region Properties

    public List<IGas> Gases => gases;
    public float Pressure
    {
        get => pressure;
        set => pressure = Mathf.Max(0, value); // Prevent negative pressure
    }
    public string ContainerName => gameObject.name;

    #endregion

    private void Start()
    {
        inlet.FindConnection();
        outlet.FindConnection();
        RefreshEffects();
    }

    private void OnValidate()
    {
        // Refresh effects when changed in inspector
        if (Application.isPlaying)
            effectsNeedRefresh = true;
    }


    public void SetRunning(bool running) => isRunning = running;

    private void FixedUpdate()
    {
        if (effectsNeedRefresh)
            RefreshEffects();

        if (!isRunning || !inlet.IsConnected() || !outlet.IsConnected())
            return;

        PumpGas();
    }

    /// <summary>
    /// Cache and initialize all effects from externalAffectors
    /// </summary>
    private void RefreshEffects()
    {
        // Cleanup old effects
        if (cachedEffects != null)
        {
            foreach (var effect in cachedEffects)
            {
                effect?.Cleanup();
            }
        }

        // Initialize new effects
        List<IGasEffect> activeEffects = new();

        foreach (var affectorGO in externalAffectors)
        {
            if (affectorGO == null)
                continue;

            var effect = affectorGO.GetComponent<IGasEffect>();
            if (effect == null)
            {
                Debug.LogWarning(
                    $"GameObject '{affectorGO.name}' does not have an IGasEffect component",
                    affectorGO
                );
                continue;
            }

            try
            {
                effect.Initialize(this);
                activeEffects.Add(effect);
                Debug.Log($"Initialized effect: {effect.GetEffectName()}", affectorGO);
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"Failed to initialize effect {effect.GetEffectName()}: {ex.Message}",
                    affectorGO
                );
            }
        }

        cachedEffects = activeEffects.ToArray();
        effectsNeedRefresh = false;
    }

    public void AddAffector(GameObject affectorGO)
    {
        System.Array.Resize(ref externalAffectors, externalAffectors.Length + 1);
        externalAffectors[externalAffectors.Length - 1] = affectorGO;
        effectsNeedRefresh = true;
    }

    public void RemoveAffector(GameObject affectorGO)
    {
        externalAffectors = externalAffectors.Where(g => g != affectorGO).ToArray();
        effectsNeedRefresh = true;
    }


    private void PumpGas()
    {
        var inletContainer = inlet.GetConnectedContainer();
        var outletContainer = outlet.GetConnectedContainer();

        float pressureDiff = (pressure + basePressureBoost) - outletContainer.Pressure;

        if (pressureDiff <= 0)
        {
            HandleBackflow(inletContainer, outletContainer);
            return;
        }

        TransferGas(inletContainer, this, Time.fixedDeltaTime);
        TransferGas(this, outletContainer, Time.fixedDeltaTime);
    }


    private void HandleBackflow(IGasContainer inletContainer, IGasContainer outletContainer)
    {
        float backflowThreshold = inletContainer.Pressure - outletContainer.Pressure;

        if (backflowThreshold > 0.1f)
        {
            TransferGas(outletContainer, inletContainer, Time.fixedDeltaTime);
        }
    }

    /// <summary>
    /// Transfer gas with all effects applied
    /// </summary>
    private void TransferGas(IGasContainer source, IGasContainer destination, float deltaTime)
    {
        if (source.Gases.Count == 0)
            return;

        // Create transfer batches
        var gasBatches = new List<GasTransferBatch>();
        float totalAmount = 0;

        foreach (var gas in source.Gases)
        {
            var batch = new GasTransferBatch(gas.GetIDName(), gas.GetConcentration());
            gasBatches.Add(batch);
            totalAmount += gas.GetConcentration();
        }

        // Create context for effects
        var context = new TransferContext(
            source,
            destination,
            gasBatches,
            deltaTime,
            baseFlowRate
        );

        // Apply pre-transfer effects
        try
        {
            foreach (var effect in cachedEffects)
            {
                if (!effect.IsActive())
                    continue;

                effect.OnPreTransfer(context);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in pre-transfer effect: {ex.Message}\n{ex.StackTrace}");
        }

        // Calculate actual transfer amount
        float amountToTransfer = context.BaseFlowRate * deltaTime;

        // Apply post-transfer effects (can modify amounts)
        try
        {
            foreach (var effect in cachedEffects)
            {
                if (!effect.IsActive())
                    continue;

                effect.OnPostTransfer(context);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in post-transfer effect: {ex.Message}\n{ex.StackTrace}");
        }

        // Perform actual transfer with modified batches
        float actualTransferred = 0;

        foreach (var batch in context.GasBatches)
        {
            if (batch.Amount <= 0)
                continue;

            float toTransfer = Mathf.Min(batch.Amount, amountToTransfer - actualTransferred);

            // Remove from source
            var sourceGas = source.Gases.FirstOrDefault(g => g.GetIDName() == batch.GasType);
            if (sourceGas != null)
            {
                sourceGas.SetConcentration(sourceGas.GetConcentration() - toTransfer);
                if (sourceGas.GetConcentration() <= 0)
                    source.Gases.Remove(sourceGas);
            }

            // Add to destination
            var destGas = destination.Gases.FirstOrDefault(g => g.GetIDName() == batch.GasType);
            if (destGas != null)
            {
                destGas.SetConcentration(destGas.GetConcentration() - toTransfer);
            }
            else
            {
                destination.Gases.Add(new IGas(batch.GasType,sourceGas.GetTemp(), toTransfer));
            }

            actualTransferred += toTransfer;
        }

        // Update pressures
        source.Pressure -= actualTransferred * 0.01f;
        destination.Pressure += actualTransferred * 0.01f;
    }

    private void OnDestroy()
    {
        if (cachedEffects != null)
        {
            foreach (var effect in cachedEffects)
            {
                effect?.Cleanup();
            }
        }
    }


}
