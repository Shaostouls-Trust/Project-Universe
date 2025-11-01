using UnityEngine;

// Example component behavior
public class ShipComponent : MonoBehaviour
{
    [SerializeField] private string componentId;
    [SerializeField] private float maxHealth = 100f;

    private void Start()
    {
        // Register with state manager
        var initialState = new ShipComponentState
        {
            id = componentId,
            health = maxHealth,
            position = transform.position,
            isActive = true
        };

        ShipStateManager.Instance.RegisterComponent(componentId, initialState, OnStateUpdated);
    }

    private void OnStateUpdated(ShipComponentState newState)
    {
        // Update visual representation based on state
        if (!newState.isActive)
        {
            // Handle disabled state
        }

        // Update any visual damage indicators
        float healthPercentage = newState.health / maxHealth;
        // Update materials, effects, etc.
    }
}