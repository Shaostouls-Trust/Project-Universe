using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Artngame.Orion.Aurora
{
    public class EarthSpinScript : MonoBehaviour
    {
        bool spinning = true;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            float spinrate = -360.0f / (24 * 24 * 60); // degrees per second
            float spin = spinrate * Time.deltaTime * TimeControl.timelapse;

            if (Input.GetKeyDown("space"))
                spinning = !spinning; // toggle spin with spacebar

            if (spinning)
                transform.Rotate(Vector3.up * spin);
        }
    }
}