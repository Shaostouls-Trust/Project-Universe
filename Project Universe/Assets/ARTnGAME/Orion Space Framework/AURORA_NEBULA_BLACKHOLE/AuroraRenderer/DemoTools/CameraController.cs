using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Artngame.Orion.Aurora
{

    public class CameraController : MonoBehaviour
    {

        // Start is called before the first frame update
        void Start()
        {
            TimeControl.ui_timelapse = 20.0f;
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

            // Update time zooming
            if (Input.GetKeyDown(".")) TimeControl.ui_timelapse *= 4.0f;
            if (Input.GetKeyDown(",")) TimeControl.ui_timelapse /= 4.0f;
            if (Input.GetKeyDown("/")) TimeControl.ui_timelapse = 1.0f;
            TimeControl.Update();

            float transAmount = speed * Time.deltaTime;
            float rotateAmount = rotateSpeed * Time.deltaTime;

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
            transform.Rotate(rotY, rotX, 0);

            if (Input.GetKey("a"))
            {
                transform.Translate(-transAmount, 0, 0);
            }
            if (Input.GetKey("d"))
            {
                transform.Translate(transAmount, 0, 0);
            }

            if (Input.GetKey("z"))
            {
                transform.Translate(0, -transAmount, 0);
            }
            if (Input.GetKey("q"))
            {
                transform.Translate(0, transAmount, 0);
            }

            if (Input.GetKey("s"))
            {
                transform.Translate(0, 0, -transAmount);
            }
            if (Input.GetKey("w"))
            {
                transform.Translate(0, 0, transAmount);
            }

            float min_altitude = 1.00001f; // keep outside of the planet
            if (transform.position.magnitude < min_altitude)
            {
                transform.position = transform.position * (min_altitude / transform.position.magnitude);
            }


            if (Input.GetKey("x") || Input.GetKey("escape"))
            {
                Application.Quit();
            }

        }
    }
}