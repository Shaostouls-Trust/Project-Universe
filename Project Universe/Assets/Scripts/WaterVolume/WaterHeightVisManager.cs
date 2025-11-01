using UnityEngine;

public class WaterHeightVisManager : MonoBehaviour
{
    [SerializeField] private BoxCollider sourceCollider;
    private BoxCollider thisCollider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        thisCollider = GetComponent<BoxCollider>();
        // Set the scale of the water to match the room source volume collider.
        transform.localScale = sourceCollider.size;
        // Set the transform position to the lower bound of the volume
        transform.position = new Vector3(sourceCollider.transform.position.x,
            sourceCollider.transform.position.y - sourceCollider.bounds.extents.y,
            sourceCollider.transform.position.z);
    }

    public void ProxyStart(BoxCollider collider)
    {
        sourceCollider = collider;
        Start();
    }

    // float fillFraction is the decimal percent of the room that is filled.
    public void UpdateWaterLevel(float fillFraction)
    {
        // The gameobject position and box collider y offset (divided by the room y scale) must be set
        // At full fill, the box offset is - half the room height / localScale.y
        // At half fill, the box offset is zero
        // At empty, the box offset is + half the room height / localScale.y
        // This works out to be 0.5f
        thisCollider.center = new Vector3(thisCollider.center.x, Mathf.Lerp(0.5f, -0.5f, fillFraction), thisCollider.center.z);
        // transform position follows oppose logic, unscaled by localScale
        float offset = transform.localScale.y * 0.5f;
        transform.localPosition = new Vector3(transform.localPosition.x, Mathf.Lerp(-offset, offset, fillFraction), transform.localPosition.z);
    }
}
