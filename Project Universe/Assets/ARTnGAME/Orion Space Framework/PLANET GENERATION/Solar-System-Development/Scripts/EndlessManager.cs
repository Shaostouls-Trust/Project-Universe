using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.Orion.ProceduralPlanets
{
    public class EndlessManager : MonoBehaviour
    {

        public float distanceThreshold = 1000;
        List<Transform> physicsObjects;
        Ship ship;
        PlayerControllerSolarS player; //PlayerController player;//v1.0.6c
        Camera playerCamera;

        public event System.Action PostFloatingOriginUpdate;

        void Awake()
        {
            var ship = FindObjectOfType<Ship>();
            var player = FindObjectOfType<PlayerControllerSolarS>();//v1.0.6c
            var bodies = FindObjectsOfType<CelestialBody>();

            physicsObjects = new List<Transform>();
            physicsObjects.Add(ship.transform);
            physicsObjects.Add(player.transform);
            foreach (var c in bodies)
            {
                physicsObjects.Add(c.transform);
            }

            playerCamera = Camera.main;
        }

        void LateUpdate()
        {
            UpdateFloatingOrigin();
            if (PostFloatingOriginUpdate != null)
            {
                PostFloatingOriginUpdate();
            }
        }

        void UpdateFloatingOrigin()
        {
            Vector3 originOffset = playerCamera.transform.position;
            float dstFromOrigin = originOffset.magnitude;

            if (dstFromOrigin > distanceThreshold)
            {
                foreach (Transform t in physicsObjects)
                {
                    t.position -= originOffset;
                }
            }
        }

    }
}