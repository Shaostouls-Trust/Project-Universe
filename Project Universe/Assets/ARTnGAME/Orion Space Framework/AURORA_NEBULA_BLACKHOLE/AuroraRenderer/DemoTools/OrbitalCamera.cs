using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Artngame.Orion.Aurora
{
    public class OrbitalCamera : MonoBehaviour
    {
        Vector3 P = new Vector3(); // position, in m
        Vector3 V = new Vector3(); // velocity, in m / second

        float km = 1000.0f; // meters per kilometer
        float Re = 6378000.0f; // radius of Earth, in meters

        // Start is called before the first frame update
        void Start()
        {
            P.Set(-0.09f, 0.85f, -0.55f); // earth radii
            P = P * Re; // scale up to meters
            V.Set(-7.9f, 0.0f, 0.0f); // km/sec
            V = V * km; // scale up to meters
        }

        // Update is called once per frame
        void Update()
        {
            // Adapted from: http://3dcognition.com/unity-flight-simulator-phase-2/
            //   and http://wiki.unity3d.com/index.php/SmoothMouseLook


            float altitude = transform.position.magnitude;

            float rotateSpeed = 30.0f; // degrees/second
            float speed = 0.5f * (altitude - 0.99f); // WASD movement, earth radius/second
            float mouseSpeed = 140.0f; // degrees rotation per pixel of mouse movement / second

            float transAmount = speed * Time.deltaTime;
            float rotateAmount = rotateSpeed * Time.deltaTime;

            // Update time zooming
            if (Input.GetKeyDown(".")) TimeControl.ui_timelapse *= 4.0f;
            if (Input.GetKeyDown(",")) TimeControl.ui_timelapse /= 4.0f;
            if (Input.GetKeyDown("/")) TimeControl.ui_timelapse = 1.0f;

            float zoom = Input.GetAxis("Time Zoom");
            if (zoom > 0.0f)
            {
                TimeControl.ui_timelapse = 16.0f * Mathf.Pow(4.0f, 1.0f + 2.0f * zoom);
            }
            TimeControl.Update();

            float rotX = 0.0f;
            float rotY = 0.0f;
            if (Input.GetMouseButton(0))
            {
                rotX += Input.GetAxis("Mouse X") * mouseSpeed * Time.deltaTime;
                rotY -= Input.GetAxis("Mouse Y") * mouseSpeed * Time.deltaTime;
            }
            if (Input.GetKey("up"))
            {
                rotY += rotateAmount;
            }
            if (Input.GetKey("down"))
            {
                rotY -= rotateAmount;
            }
            if (Input.GetKey("left"))
            {
                rotX -= rotateAmount;
            }
            if (Input.GetKey("right"))
            {
                rotX += rotateAmount;
            }

            Vector3 rocket = new Vector3(0.0f, 0.0f, 0.0f);
            float rocketAccel = 50.0f; // m/s^2 acceleration in vacuum
            if (Input.GetKey("a")) { rocket.x = -rocketAccel; }
            if (Input.GetKey("d")) { rocket.x = +rocketAccel; }

            if (Input.GetKey("z")) { rocket.y = -rocketAccel; }
            if (Input.GetKey("q")) { rocket.y = +rocketAccel; }

            if (Input.GetKey("s")) { rocket.z = -rocketAccel; }
            if (Input.GetKey("w")) { rocket.z = +rocketAccel; }

            // Rotate keyboard rocket thrust to match local motion frame
            rocket = rocket.x * transform.right + rocket.y * transform.up + rocket.z * transform.forward;

            float thrust = Input.GetAxis("Rocket Thrust");
            if (thrust > 0.0f)
            {
                rocket.z += thrust; // FIXME: rotate to match controller orientation
            }


            float me = 5.972e24f; // mass of Earth, in kilograms
            float G = 6.67408e-11f; // gravitational constant, MKS units
            float r = P.magnitude; // distance to spacecraft, in meters
            float accel = -G * me / (r * r); // scalar acceleration due to gravity (m/s^2)
            Vector3 A = P * (accel / P.magnitude); // vector acceleration due to gravity
            A += rocket; // acceleration due to rocket
            float dt = Time.deltaTime * TimeControl.timelapse;
            V = V + dt * A; // Euler update for velocity
            P = P + dt * V; // Euler update for position

            float height = (P.magnitude - Re) / (km); // kilometers altitude
            if (height < 60.0f)
            {
                float air_density = Mathf.Exp(-height / 8.0f);
                float dragfactor = 0.1f;
                float dragloss = (1.0f - dragfactor * air_density * dt);
                if (dragloss < 0.5f) dragloss = 0.5f;
                V = V * dragloss;
            }

            float min_altitude = 1.00001f * Re; // stay outside of the planet
            if (P.magnitude < min_altitude)
            {
                P = P * (min_altitude / P.magnitude);
            }
            float max_altitude = 100.0f * Re; // stay fairly near the planet
            if (P.magnitude > max_altitude)
            {
                P = P * (max_altitude / P.magnitude);
                V = V * 0.001f;
            }

            transform.position = P * (1.0f / Re);// copy out simulated position to GUI position (in Earth radii)

            transform.Rotate(rotY, rotX, 0);


            if (Input.GetKey("x") || Input.GetKey("escape"))
            {
                Application.Quit();
            }

        }
    }
}