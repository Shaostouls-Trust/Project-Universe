using System;
using UnityEngine;

namespace Artngame.Orion.MiniGames
{
    public class PlayerController : MonoBehaviour
    {
        public float speed = 1.0f;
        public float tilt = 4.0f;
        public Boundary boundary;
        public GameObject shotPrefab;
        public Transform shotSpawn;
        public float fireDelay = 0.25f;

        private Rigidbody rb;
        private AudioSource audioSource;
        private float nextFire;

        private void Update()
        {
            if (Input.GetButton("Fire1") && (Time.time > nextFire))
            {
                Instantiate(shotPrefab, shotSpawn.position, shotSpawn.rotation);
                nextFire = Time.time + fireDelay;
                audioSource.Play();
            }
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            audioSource = GetComponent<AudioSource>();
        }

        private void FixedUpdate()
        {
            float moveHorizontal = Input.GetAxis("Horizontal");
            float moveVertical = Input.GetAxis("Vertical");
            var velocity = new Vector3(moveHorizontal, 0.0f, moveVertical);
            rb.velocity = speed*velocity;
            rb.position = boundary.Clamp(rb.position);
            rb.rotation = Quaternion.Euler(0.0f, 0.0f, rb.velocity.x*-tilt);
        }
    }

    [Serializable]
    public class Boundary
    {
        public float xMin, xMax, zMin, zMax;

        public Vector3 Clamp(Vector3 vector)
        {
            var vector3 = new Vector3(
                Mathf.Clamp(vector.x, xMin, xMax),
                vector.y,
                Mathf.Clamp(vector.z, zMin, zMax)
                );
            return vector3;
        }
    }
}