using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Artngame.Orion.Aurora
{
    public class TimeControl
    {
        public static float timelapse = 1.0f; // seconds of simulated time per second of game time
        public static float ui_timelapse = 1.0f; // user interface-driven time

        // Update is called once per frame
        public static void Update()
        {
            float blend = Mathf.Clamp(Time.deltaTime, 0.01f, 0.8f);
            timelapse = (1.0f - blend) * timelapse + blend * ui_timelapse;
        }
    }
}