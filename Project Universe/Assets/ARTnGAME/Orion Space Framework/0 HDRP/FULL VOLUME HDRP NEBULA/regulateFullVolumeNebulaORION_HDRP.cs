using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Artngame.SKYMASTER {
    public class regulateFullVolumeNebulaORION_HDRP : MonoBehaviour
    {

        public Transform player;
        public float cloudMiddle = 24000;
        public connectSuntoNebulaCloudsHDRP nebulaClouds;
        //public float startPlayerHeight;

        public bool SmoothLights = false;//lerp lights than enable/disable
        public Light startLight;
        public int optimize_factor = 5;       
        public float max_start_light_intensity = 8;
        public float LightFadeSpeed = 5;
        public float LightSpeed = 100;
        public float LightRipple = 3;
        Vector3 Saved_start_pos; //v1.7

        public float MaxDist = 1000;
        float stop_time;
        public Vector2 Light_delay = new Vector2(0.0f, 0.3f);
        public float multiplier = 1;
        // Start is called before the first frame update
        void Start()
        {
            if (startLight)
            {
                startLight.enabled = false; startLight.gameObject.SetActive(false);
            }
        }

        // Update is called once per frame
        void Update()
        {
            nebulaClouds.startHeight = (player.transform.position.y - cloudMiddle) * multiplier;
            nebulaClouds.planetZeroCoordinate = new Vector3(nebulaClouds.planetZeroCoordinate.x, nebulaClouds.startHeight, nebulaClouds.planetZeroCoordinate.z);

            nebulaClouds.localLight = startLight;

            int get_in = 1;
            get_in = UnityEngine.Random.Range(1, optimize_factor);
            if (startLight)
            {
                if (get_in == 1)// & target != null)
                {
                    if (!SmoothLights)
                    {
                        startLight.enabled = true;
                        startLight.gameObject.SetActive(true);
                        float max_distX = UnityEngine.Random.Range(-MaxDist, MaxDist);
                        float max_distY = UnityEngine.Random.Range(-MaxDist, MaxDist);
                        float max_distZ = UnityEngine.Random.Range(-MaxDist, MaxDist);
                        startLight.transform.position = Camera.main.transform.position + new Vector3(max_distX, max_distY, max_distZ); // Saved_start_pos; // particles[0].position;//v3.4.5b
                    }
                    else
                    {
                        startLight.enabled = true;
                        startLight.gameObject.SetActive(true);
                        startLight.intensity = Mathf.Lerp(startLight.intensity, max_start_light_intensity, Time.deltaTime * LightSpeed);
                        float max_distX = UnityEngine.Random.Range(-MaxDist, MaxDist);
                        float max_distY = UnityEngine.Random.Range(-MaxDist, MaxDist);
                        float max_distZ = UnityEngine.Random.Range(-MaxDist, MaxDist);
                        startLight.transform.position = Camera.main.transform.position + new Vector3(max_distX, max_distY, max_distZ); //Saved_start_pos; //particles[0].position;//v3.4.5b
                    }
                }
                else
                {
                    if (!SmoothLights)
                    {
                        startLight.enabled = false;
                    }
                    else
                    {
                        startLight.intensity = Mathf.Lerp(startLight.intensity, 0, Time.deltaTime * LightFadeSpeed * Random.Range(1, LightRipple));
                    }
                }
            }

            //v1.7
            if (startLight & Time.fixedTime - stop_time > UnityEngine.Random.Range(Light_delay.x, Light_delay.y)) //& target == null
            {
                stop_time = Time.fixedTime;

                if (!SmoothLights)
                {
                    startLight.enabled = false;
                    startLight.gameObject.SetActive(false);
                }
                else
                {
                    startLight.intensity = Mathf.Lerp(startLight.intensity, 0, Time.deltaTime * LightFadeSpeed * Random.Range(1, LightRipple));
                }
            }

        }
    }
}