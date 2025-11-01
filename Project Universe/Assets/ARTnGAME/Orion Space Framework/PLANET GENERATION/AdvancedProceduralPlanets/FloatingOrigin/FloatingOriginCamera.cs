using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.Orion.PlanetGEN
{
    [RequireComponent(typeof(Camera))]
    public class FloatingOriginCamera : MonoBehaviour
    {
        [SerializeField]
        float tresholdDistance = 1000;

        public static FloatingOriginCamera main
        {
            get
            {
                var c = Camera.main.GetComponent<FloatingOriginCamera>();
                return c;
            }
        }

        public float fieldOfView
        {
            get { return cameraA.fieldOfView; }
            set { cameraA.fieldOfView = value; }
        }

        public Quaternion Rotation
        {
            get { return transform.rotation; }
            set { transform.rotation = value; }
        }
        public Vector3 VisualPosition
        {
            get
            {
                return transform.position;
            }
            set
            {
                transform.position = value;
            }
        }
        public BigPosition BigPosition
        {
            get
            {
                return VisualSceneOrigin + transform.position;
            }
            set
            {
                transform.position = value - VisualSceneOrigin;
            }
        }


        public BigPosition VisualSceneOrigin { get; private set; }


        private List<FloatingOriginTransform> floatingTransforms = new List<FloatingOriginTransform>();


        private Camera cameraA;

        private void Awake()
        {
            cameraA = GetComponent<Camera>();
        }

        public void Add(FloatingOriginTransform f)
        {
            floatingTransforms.Add(f);
        }

        public void Remove(FloatingOriginTransform f)
        {
            floatingTransforms.Remove(f);
        }

        private void FixedUpdate()
        {
            if (this.transform.position.sqrMagnitude > tresholdDistance * tresholdDistance)
            {
                MyProfiler.BeginSample("Floating origin / scene origin changed");

                var worldPosDelta = new BigPosition(transform.position).KeepOnlySectorPos();
                VisualSceneOrigin += worldPosDelta;
                this.transform.position -= worldPosDelta;

                foreach (var i in floatingTransforms)
                    i.SceneOriginChanged(VisualSceneOrigin);

                MyProfiler.EndSample();
            }
        }

    }
}
