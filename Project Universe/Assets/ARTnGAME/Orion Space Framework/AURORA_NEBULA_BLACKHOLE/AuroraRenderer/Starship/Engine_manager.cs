using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Artngame.Orion.Aurora
{
    public class Engine_manager : MonoBehaviour
    {
        public static float g_ThrustLevel = 0.0f;
        public float g_AfterGlow_1 = 0.5f;
        public float g_AfterGlow_3 = 0.0f;
        public float g_AfterGlow_7 = 0.0f;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            // g_ThrustLevel cut points where we switch between 1-3-7 engines.
            float cut_0 = 0.0f;
            float cut_1 = 0.15f;
            float cut_3 = 0.4f;

            float t1 = 0.0f, t3 = 0.0f, t7 = 0.0f;

            // Apply heat to all engines
            float heatrate = Mathf.Clamp(0.2f * TimeControl.timelapse * Time.deltaTime, 0.0f, 0.5f);
            if (g_ThrustLevel > cut_0) { t1 = 1.0f; g_AfterGlow_1 = heatrate * 1.0f + (1.0f - heatrate) * g_AfterGlow_1; }
            if (g_ThrustLevel > cut_1) { t3 = 1.0f; g_AfterGlow_3 = heatrate * 1.0f + (1.0f - heatrate) * g_AfterGlow_3; }
            if (g_ThrustLevel > cut_3) { t7 = 1.0f; g_AfterGlow_7 = heatrate * 1.0f + (1.0f - heatrate) * g_AfterGlow_7; }

            // Apply (radiative) cooling to all engines
            float coolrate = Mathf.Clamp(0.1f * TimeControl.timelapse * Time.deltaTime, 0.0f, 0.5f);
            g_AfterGlow_1 *= 1.0f - coolrate;
            g_AfterGlow_3 *= 1.0f - coolrate;
            g_AfterGlow_7 *= 1.0f - coolrate;

            Shader.SetGlobalFloat("g_Thrust_1", t1);
            Shader.SetGlobalFloat("g_Thrust_3", t3);
            Shader.SetGlobalFloat("g_Thrust_7", t7);
            Shader.SetGlobalFloat("g_AfterGlow_1", g_AfterGlow_1);
            Shader.SetGlobalFloat("g_AfterGlow_3", g_AfterGlow_3);
            Shader.SetGlobalFloat("g_AfterGlow_7", g_AfterGlow_7);
        }
    }
}
