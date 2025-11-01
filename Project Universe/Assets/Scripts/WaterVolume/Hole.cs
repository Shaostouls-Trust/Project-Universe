using UnityEngine;

// Hole implementation
public class Hole : Opening
{
    [Header("Hole Specific")]
    public bool isBlocked = false;
    public bool isThick = false;

    [Header("Thick Hole Properties")]
    public Vector3 holePosition1;      // One end of the hole
    public Vector3 holePosition2;      // Other end of the hole
    public float thickness = 0f;       // Thickness of the wall/floor
    public bool isHorizontal = false;  // True if hole goes through a wall

    private void Start()
    {
        // Add holes created in editor
        // Register with water flow system
        WaterFlowSystem waterFlowSystem = FindFirstObjectByType<WaterFlowSystem>();
        if (waterFlowSystem != null)
        {
            waterFlowSystem.RegisterNewOpening(this);
        }
    }

    public override bool CanWaterFlow()
    {
        return !isBlocked;
    }

    public void BlockHole()
    {
        isBlocked = true;
    }

    public void UnblockHole()
    {
        isBlocked = false;
    }

    public void SetThickHoleProperties(Vector3 entryPoint, Vector3 exitPoint, float wallThickness)
    {
        isThick = true;
        holePosition1 = entryPoint;
        holePosition2 = exitPoint;
        thickness = wallThickness;

        // Determine if this is a vertical or horizontal hole
        float verticalDiff = Mathf.Abs(exitPoint.y - entryPoint.y);
        float horizontalDiff = Mathf.Max(Mathf.Abs(exitPoint.x - entryPoint.x), Mathf.Abs(exitPoint.z - entryPoint.z));

        isHorizontal = horizontalDiff > verticalDiff;

        // Set bottom height to the lower of the two positions
        float minY = Mathf.Min(entryPoint.y, exitPoint.y);
        bottomHeight = minY - transform.position.y;

        // For horizontal holes, height is the actual opening dimension
        // For vertical holes, height is the vertical distance
        if (isHorizontal)
        {
            height = width; // Circular or square hole
        }
        else
        {
            height = Mathf.Abs(exitPoint.y - entryPoint.y);
        }
    }

    public override float GetBottomElevation()
    {
        if (isThick)
        {
            return Mathf.Min(holePosition1.y, holePosition2.y);
        }
        return base.GetBottomElevation();
    }

    public float GetTopElevation()
    {
        if (isThick)
        {
            return Mathf.Max(holePosition1.y, holePosition2.y);
        }
        return GetBottomElevation() + height;
    }
}
