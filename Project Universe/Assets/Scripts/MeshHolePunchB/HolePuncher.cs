using System.Collections.Generic;
using System.Linq;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using UnityEngine;
namespace ProjectUniverse.Environment.Destruction
{
    public class HolePuncher : MonoBehaviour
    {
        [SerializeField] private float holeRadius = 0.5f;
        [SerializeField] private GameObject holePrefab; // Prefab with Hole component
        [SerializeField] private float thicknessThreshold = 0.1f; // Minimum thickness to consider "thick"

        [Header("Penetration Settings")]
        [SerializeField] private float minPenetrationVelocity = 5f; // Minimum velocity to penetrate anything (m/s)
        [SerializeField] private float penetrationModifier = 1f; // Modifier for required velocity
        [SerializeField] private AnimationCurve penetrationCurve; // Maps velocity to max penetration depth
        [SerializeField] private float maxPenetrationDepth = 100f; // Maximum penetration in cm
        [SerializeField] private bool enablePenetrationDebug = false;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = false;

        private WaterFlowSystem waterFlowSystem;
        private Rigidbody rb;
        private Vector3 lastVelocity; // Store velocity before collision

        void Start()
        {
            waterFlowSystem = FindFirstObjectByType<WaterFlowSystem>();
            rb = GetComponent<Rigidbody>();

            // Create default hole prefab if none assigned
            if (holePrefab == null)
            {
                holePrefab = new GameObject("HolePrefab");
                holePrefab.AddComponent<Hole>();
                holePrefab.SetActive(false);
            }

            // Setup default penetration curve if none assigned
            if (penetrationCurve == null || penetrationCurve.keys.Length == 0)
            {
                SetupDefaultPenetrationCurve();
            }
        }

        void SetupDefaultPenetrationCurve()
        {
            penetrationCurve = new AnimationCurve();

            // Mimics realistic penetration curve - logarithmic growth
            // X = velocity (m/s), Y = penetration depth (normalized 0-1)
            penetrationCurve.AddKey(0f, 0f);           // No velocity = no penetration
            penetrationCurve.AddKey(5f, 0.05f);        // Minimum velocity threshold
            penetrationCurve.AddKey(10f, 0.15f);       // Low velocity
            penetrationCurve.AddKey(20f, 0.35f);       // Medium velocity
            penetrationCurve.AddKey(50f, 0.6f);        // High velocity
            penetrationCurve.AddKey(100f, 0.8f);       // Very high velocity
            penetrationCurve.AddKey(200f, 0.95f);      // Extreme velocity
            penetrationCurve.AddKey(500f, 1f);         // Maximum penetration

            // Set curve tangents for smooth logarithmic shape
            for (int i = 0; i < penetrationCurve.keys.Length; i++)
            {
                penetrationCurve.SmoothTangents(i, 0.5f);
            }
        }

        void FixedUpdate()
        {
            // Store velocity before any collision occurs
            if (rb != null)
            {
                lastVelocity = rb.linearVelocity;
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            MeshHolePunch fragmenter = collision.gameObject.GetComponent<MeshHolePunch>();

            if (fragmenter != null)
            {
                // Get impact point and direction
                ContactPoint contact = collision.contacts[0];
                Vector3 impactPoint = contact.point;
                Vector3 impactRotation = transform.rotation.eulerAngles;

                // Use velocity as impact direction
                Vector3 impactDirection = lastVelocity;
                Vector3 impactVelocity;

                // First try using the relative velocity (velocity difference at impact)
                if (collision.relativeVelocity.magnitude > 0.1f)
                {
                    // Negative so that the directions in the thickness method work with all cases here
                    impactDirection = -collision.relativeVelocity.normalized;
                    impactVelocity = collision.relativeVelocity;
                    Debug.Log(impactDirection);
                }
                // Then try the stored last velocity
                else if (lastVelocity.magnitude > 0.1f)
                {
                    impactDirection = -lastVelocity.normalized;
                    impactVelocity = lastVelocity;
                    Debug.Log(impactDirection);
                }
                // Last resort: use negative contact normal
                else
                {
                    impactVelocity = Vector3.zero;
                    impactDirection = -contact.normal;
                    if (enableDebugLogs)
                        Debug.Log("Using contact normal as fallback for impact direction:"+impactDirection);
                }

                float velocityMagnitude = impactVelocity.magnitude;
                if (velocityMagnitude < minPenetrationVelocity)
                {
                    if (enableDebugLogs)
                        Debug.Log($"Below minimum penetration velocity: {velocityMagnitude:F2} < {minPenetrationVelocity:F2} m/s");
                    return;
                }

                // Calculate angle factor (1 = perpendicular, 0 = parallel)
                float angleFactor = Mathf.Abs(Vector3.Dot(impactDirection, contact.normal));

                // Oblique angles reduce penetration capability
                if (angleFactor < 0.2f) // Less than ~11 degrees from parallel
                {
                    if (enableDebugLogs)
                        Debug.Log($"Impact angle too shallow: {Mathf.Acos(angleFactor) * Mathf.Rad2Deg:F1} degrees");
                    return;
                }

                // Calculate penetration capability
                float basePenetration = penetrationCurve.Evaluate(velocityMagnitude) * maxPenetrationDepth;
                float effectivePenetration = basePenetration * angleFactor * penetrationModifier;

                // Calculate thickness
                float thickness = CalculateThickness(impactPoint, impactDirection, collision.gameObject);
                float thicknessMeters = thickness / 100f; // Convert cm to meters

                // Check if projectile can penetrate this thickness
                if (thicknessMeters > effectivePenetration)
                {
                    if (enableDebugLogs)
                        Debug.Log($"Insufficient penetration: {effectivePenetration:F3}m < {thicknessMeters:F3}m thick " +
                                $"(velocity: {velocityMagnitude:F1} m/s, angle factor: {angleFactor:F2})");
                    return;
                }

                // Calculate entry and exit points
                Vector3 entryPoint = impactPoint;
                Vector3 exitPoint = impactPoint + (impactDirection.normalized * thickness / 100f); // Convert cm to m

                // Punch hole through the mesh
                fragmenter.PunchHole(holeRadius, impactPoint, impactRotation);

                // Create hole opening for water flow
                CreateHoleOpening(entryPoint, exitPoint, thickness, collision.gameObject);
            }
        }
        private float CalculateThickness(Vector3 impactPoint, Vector3 direction, GameObject target)
        {
            // Raycast from 10f along the line of impact towards the impact point.
            Vector3 source = impactPoint + direction.normalized * 10f;
            // Raycast from source to target
            RaycastHit[] forwardHits = Physics.RaycastAll(source, -direction.normalized, 11f);
            Debug.Log("From source: " + -direction.normalized);
            // Raycast from target to source to get exit points
            // Move the impact point back a hair so that the raycast hits the surface
            impactPoint -= direction.normalized * .1f;
            RaycastHit[] backwardHits = Physics.RaycastAll(impactPoint, direction.normalized, 11f);
            // Combine both forward and backward hits
            RaycastHit[] allHits = new RaycastHit[forwardHits.Length + backwardHits.Length];
            forwardHits.CopyTo(allHits, 0);
            backwardHits.CopyTo(allHits, forwardHits.Length);
            List<RaycastHit> allHitsList = allHits.ToList();

            // Filter to only hits on the target object
            allHitsList = allHitsList.Where(hit => hit.collider.gameObject == target).ToList();

            if (allHitsList.Count < 2)
            {
                Debug.LogWarning($"Could not determine thickness - only {allHitsList.Count} hits on target");
                return 0f;
            }

            allHitsList.Sort((a, b) =>
            {
                float distA = Vector3.Dot(a.point - source, direction.normalized);
                float distB = Vector3.Dot(b.point - source, direction.normalized);
                return distA.CompareTo(distB);
            });
            // Sort hits by distance from the source
            // Get only the first hit (the one furthest from source).
            // This will correspond to the first and last indicies of the list (furthest to fwr, closest to back)

            //for (int i = 0; i < allHitsList.Count; i++)
            //{
                //Debug.Log($"hit {i}: {allHitsList[i].point} on {allHitsList[i].collider.gameObject.name}");
            //}

            // Get the first and last hits to determine thickness
            Vector3 entryHit = allHitsList[0].point;
            Vector3 exitHit = allHitsList[allHitsList.Count - 1].point;

            // Convert thickness from meters to centimeters
            return Mathf.Abs(Vector3.Distance(entryHit, exitHit)) * 100f;
        }

        private void CreateHoleOpening(Vector3 entryPoint, Vector3 exitPoint, float thickness, GameObject targetObject)
        {
            // Create hole game object
            GameObject holeGO = Instantiate(holePrefab);
            holeGO.SetActive(true);
            holeGO.transform.position = (entryPoint + exitPoint) / 2f; // Center position
            holeGO.name = $"Hole_{System.DateTime.Now.Ticks}";

            Hole hole = holeGO.GetComponent<Hole>();

            // Set hole properties
            hole.width = holeRadius * 2f;
            hole.height = holeRadius * 2f;
            hole.flowCoefficient = 0.6f; // Typical orifice coefficient

            // Determine if this is a thick hole
            bool isThick = thickness > thicknessThreshold * 100f; // Convert threshold to cm

            if (isThick)
            {
                hole.SetThickHoleProperties(entryPoint, exitPoint, thickness);
            }

            // Find connected volumes
            FindAndAssignConnectedVolumes(hole, entryPoint, exitPoint);

            // Register with water flow system
            if (waterFlowSystem != null)
            {
                waterFlowSystem.RegisterNewOpening(hole);
            }
        }

        private void FindAndAssignConnectedVolumes(Hole hole, Vector3 entryPoint, Vector3 exitPoint)
        {
            // Find all water volumes
            VolumeWaterData[] allVolumes = FindObjectsOfType<VolumeWaterData>();

            VolumeWaterData volume1 = null;
            VolumeWaterData volume2 = null;

            Debug.Log($"{entryPoint} to {exitPoint}");
            // Check which volumes contain the entry and exit points
            foreach (var volume in allVolumes)
            {
                BoxCollider collider = volume.GetComponent<BoxCollider>();
                if (collider != null)
                {
                    Bounds bounds = collider.bounds;

                    // Slightly expand bounds for edge detection
                    //bounds.Expand(0.01f);

                    if (bounds.Contains(entryPoint))
                    {
                        volume1 = volume;
                    }

                    if (bounds.Contains(exitPoint))
                    {
                        volume2 = volume;
                    }
                }
            }

            // If we didn't find two different volumes, check more carefully
            if (volume2 == null && volume1 != null)
            {
                // The exit point might be just outside the bounds, check nearest volume
                float minDistance = float.MaxValue;
                foreach (var volume in allVolumes)
                {
                    if (volume != volume1)
                    {
                        BoxCollider collider = volume.GetComponent<BoxCollider>();
                        if (collider != null)
                        {
                            Vector3 closestPoint = collider.ClosestPoint(exitPoint);
                            float distance = Vector3.Distance(closestPoint, exitPoint);
                            if (distance < minDistance && distance < 0.5f) // Within 50cm
                            {
                                minDistance = distance;
                                volume2 = volume;
                            }
                        }
                    }
                }
            }

            // Assign volumes to hole
            hole.connectedVolume1 = volume1;
            hole.connectedVolume2 = volume2;

            if (volume1 != null && volume2 != null)
            {
                Debug.Log($"Hole created connecting {volume1.name} to {volume2.name}, thickness: {hole.thickness}cm");
            }
            // If only one volume found, it might be a wall hole
            else if (volume1 == volume2 && volume1 != null)
            {
                // This is a hole within the same volume, might need special handling
                Debug.Log($"Hole created within same volume: {volume1.name}");
            }
            else
            {
                Debug.LogWarning("Hole created but no volumes found");
            }
        }
    }
}