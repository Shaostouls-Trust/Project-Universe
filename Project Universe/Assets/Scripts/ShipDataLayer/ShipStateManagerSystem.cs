using UnityEngine;
using System;
using System.Collections.Generic;

// Core data structure for ship state
[Serializable]
public class ShipComponentState
{
    public string id;
    public float health;
    public Vector3 position;
    public bool isActive;
    public Dictionary<string, float> resources = new Dictionary<string, float>();
}

// Main ship state container
[Serializable]
public class ShipState
{
    public Dictionary<string, ShipComponentState> components = new Dictionary<string, ShipComponentState>();
    public Dictionary<string, float> globalResources = new Dictionary<string, float>();
    public HashSet<string> activePlayersInside = new HashSet<string>();
    public HashSet<string> activePlayersOutside = new HashSet<string>();
}


// Singleton manager for ship state
public class ShipStateManager : MonoBehaviour
{
    private static ShipStateManager instance;
    public static ShipStateManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("ShipStateManager");
                instance = go.AddComponent<ShipStateManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private ShipState shipState = new ShipState();
    private Dictionary<string, Action<ShipComponentState>> componentUpdateCallbacks
        = new Dictionary<string, Action<ShipComponentState>>();

    // Event system for state changes
    public event Action<string, ShipComponentState> OnComponentStateChanged;
    public event Action<string, float> OnResourceChanged;
    public event Action<string, bool> OnPlayerLocationChanged;

    // Component registration
    public void RegisterComponent(string id, ShipComponentState initialState, Action<ShipComponentState> onUpdateCallback = null)
    {
        shipState.components[id] = initialState;
        if (onUpdateCallback != null)
        {
            componentUpdateCallbacks[id] = onUpdateCallback;
        }
    }

    // Update component state
    public void UpdateComponentState(string id, Action<ShipComponentState> updateAction)
    {
        if (shipState.components.TryGetValue(id, out ShipComponentState state))
        {
            updateAction(state);
            OnComponentStateChanged?.Invoke(id, state);

            if (componentUpdateCallbacks.TryGetValue(id, out Action<ShipComponentState> callback))
            {
                callback(state);
            }
        }
    }

    // Resource management
    public void UpdateResource(string resourceId, float amount)
    {
        shipState.globalResources[resourceId] = amount;
        OnResourceChanged?.Invoke(resourceId, amount);
    }

    // Player location tracking
    public void UpdatePlayerLocation(string playerId, bool isInside)
    {
        if (isInside)
        {
            shipState.activePlayersOutside.Remove(playerId);
            shipState.activePlayersInside.Add(playerId);
        }
        else
        {
            shipState.activePlayersInside.Remove(playerId);
            shipState.activePlayersOutside.Add(playerId);
        }
        OnPlayerLocationChanged?.Invoke(playerId, isInside);
    }

    // Damage system
    public void ApplyDamage(string componentId, float damage)
    {
        UpdateComponentState(componentId, state =>
        {
            state.health = Mathf.Max(0, state.health - damage);
            if (state.health == 0)
            {
                state.isActive = false;
            }
        });
    }

    // Scene transition handling
    public void PrepareForSceneTransition()
    {
        // Serialize current state if needed
        // This is where you'd implement any cleanup or state preservation logic
    }

    // State serialization for networking
    public string SerializeState()
    {
        return JsonUtility.ToJson(shipState);
    }

    public void DeserializeState(string jsonState)
    {
        shipState = JsonUtility.FromJson<ShipState>(jsonState);
        // Notify all registered callbacks of new state
        foreach (var kvp in shipState.components)
        {
            OnComponentStateChanged?.Invoke(kvp.Key, kvp.Value);
        }
    }
}
