using ProjectUniverse;
using UnityEngine;
using UnityEngine.InputSystem;

public class HolographicViewController : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera hologramCamera;
    public float rotationSpeed = 50f;
    public float panSpeed = 10f;
    public float zoomSpeed = 5f;

    [Header("Bounds")]
    public float minZoom = 5f;
    public float maxZoom = 50f;
    public float maxPanDistance = 20f;
    private NetworkGraphView _graphView;

    [Header("Input")]
    public InputAction lookAction;
    public InputAction clickAction;
    public InputAction rightClickAction;
    public InputAction scrollAction;

    private Vector3 pivotPoint;
    private float currentZoom;
    private Vector2 lookDelta;
    private bool isRotating;
    private bool isPanning;

    void Start()
    {
        if (hologramCamera == null)
            hologramCamera = Camera.main;

        // Set camera to only render hologram layer
        hologramCamera.cullingMask = 1 << LayerMask.NameToLayer("Hologram");

        // Calculate pivot point (center of all rooms)
        CalculatePivotPoint();
        currentZoom = Vector3.Distance(hologramCamera.transform.position, pivotPoint);

        // Subscribe to input events
        if (clickAction != null)
        {
            clickAction.started += OnClickStarted;
            clickAction.canceled += OnClickCanceled;
        }

        if (rightClickAction != null)
        {
            rightClickAction.started += OnRightClickStarted;
            rightClickAction.canceled += OnRightClickCanceled;
        }
    }

    private void Awake()
    {
        // Find the graph view
        _graphView = FindFirstObjectByType<NetworkGraphView>();
    }

    private bool IsMouseOverHolographicView()
    {
        // Use the graph view's boundary check if available
        if (_graphView != null)
        {
            return !_graphView.IsMouseOverGraphView();
        }

        // Fallback to right half check
        Vector2 mousePos = Mouse.current.position.ReadValue();
        return mousePos.x >= Screen.width * 0.5f;
    }

    void OnClickStarted(InputAction.CallbackContext ctx)
    {
        isRotating = true;
    }

    void OnClickCanceled(InputAction.CallbackContext ctx)
    {
        isRotating = false;
    }

    void OnRightClickStarted(InputAction.CallbackContext ctx)
    {
        isPanning = true;
    }

    void OnRightClickCanceled(InputAction.CallbackContext ctx)
    {
        isPanning = false;
    }

    void Update()
    {
        if (!IsMouseOverHolographicView())
            return;

        // Get input values
        if (lookAction != null)
            lookDelta = lookAction.ReadValue<Vector2>();

        HandleRotation();
        HandlePanning();
        HandleZoom();
    }

    void HandleRotation()
    {
        if (isRotating && lookDelta != Vector2.zero)
        {
            // Rotate around pivot
            hologramCamera.transform.RotateAround(pivotPoint, Vector3.up, lookDelta.x * rotationSpeed * Time.deltaTime);
            hologramCamera.transform.RotateAround(pivotPoint, hologramCamera.transform.right, -lookDelta.y * rotationSpeed * Time.deltaTime);

            // Keep camera looking at pivot
            hologramCamera.transform.LookAt(pivotPoint);
        }
    }

    void HandlePanning()
    {
        if (isPanning && lookDelta != Vector2.zero)
        {
            Vector3 pan = hologramCamera.transform.right * -lookDelta.x * panSpeed * Time.deltaTime;
            pan += hologramCamera.transform.up * -lookDelta.y * panSpeed * Time.deltaTime;

            // Apply bounded panning
            Vector3 newPivot = pivotPoint + pan;
            if (newPivot.magnitude < maxPanDistance)
            {
                pivotPoint = newPivot;
                hologramCamera.transform.position += pan;
            }
        }
    }

    void HandleZoom()
    {
        if (scrollAction != null)
        {
            float scroll = scrollAction.ReadValue<Vector2>().y;
            if (scroll != 0)
            {
                currentZoom -= scroll * zoomSpeed * Time.deltaTime;
                currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

                Vector3 direction = (hologramCamera.transform.position - pivotPoint).normalized;
                hologramCamera.transform.position = pivotPoint + direction * currentZoom;
            }
        }
    }

    void CalculatePivotPoint()
    {
        HolographicRoom[] rooms = FindObjectsByType<HolographicRoom>(FindObjectsSortMode.None);
        if (rooms.Length == 0) return;

        Vector3 sum = Vector3.zero;
        int count = 0;

        foreach (var room in rooms)
        {
            if (room.volumeContainer != null)
            {
                BoxCollider[] colliders = room.volumeContainer.GetComponents<BoxCollider>();
                foreach (var collider in colliders)
                {
                    sum += room.volumeContainer.transform.position + collider.center;
                    count++;
                }
            }
        }

        pivotPoint = (count > 0) ? sum / count : Vector3.zero;
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (clickAction != null)
        {
            clickAction.started -= OnClickStarted;
            clickAction.canceled -= OnClickCanceled;
        }

        if (rightClickAction != null)
        {
            rightClickAction.started -= OnRightClickStarted;
            rightClickAction.canceled -= OnRightClickCanceled;
        }
    }
}