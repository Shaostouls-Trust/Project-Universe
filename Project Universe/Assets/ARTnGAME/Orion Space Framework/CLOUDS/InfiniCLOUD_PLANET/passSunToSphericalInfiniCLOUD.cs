using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Artngame.Orion.PlanetaryInfiniCLOUD
{
    [ExecuteInEditMode]
    public class passSunToSphericalInfiniCLOUD : MonoBehaviour
    {

        public Transform Sun;
        public Material oceanMat;

        //scatter params
        public float fog_depth = 0.29f;// 1.5f;
        public float reileigh = 1.3f;//2.0f;
        public float mieCoefficient = 1;//0.1f;
        public float mieDirectionalG = 0.1f;
        public float ExposureBias = 0.11f;//0.15f; 

        const float n = 1.0003f;
        const float N = 2.545E25f;
        const float pn = 0.035f;
        public Vector3 lambda = new Vector3(680E-9f, 550E-9f, 450E-9f);//new Vector3(680E-9f, 550E-9f, 450E-9f);
        public Vector3 K = new Vector3(0.9f, 0.5f, 0.5f);//new Vector3(0.686f, 0.678f, 0.666f);

        // Start is called before the first frame update
        void Start()
        {

        }


        // Update is called once per frame
        void Update()
        {
            if (Sun != null)
            {
                oceanMat.SetVector("sunPosition", -Sun.forward.normalized);
            }

            oceanMat.SetVector("betaR", totalRayleigh(lambda) * reileigh);
            oceanMat.SetVector("betaM", totalMie(lambda, K, fog_depth) * mieCoefficient);
            oceanMat.SetFloat("fog_depth", fog_depth);
            oceanMat.SetFloat("mieCoefficient", mieCoefficient);
            oceanMat.SetFloat("mieDirectionalG", mieDirectionalG);
            oceanMat.SetFloat("ExposureBias", ExposureBias);

        }

        //UPDATE SCATTER
        static Vector3 totalRayleigh(Vector3 lambda)
        {
            Vector3 result = new Vector3((8.0f * Mathf.Pow(Mathf.PI, 3.0f) * Mathf.Pow(Mathf.Pow(n, 2.0f) - 1.0f, 2.0f) * (6.0f + 3.0f * pn)) / (3.0f * N * Mathf.Pow(lambda.x, 4.0f) * (6.0f - 7.0f * pn)),
                (8.0f * Mathf.Pow(Mathf.PI, 3.0f) * Mathf.Pow(Mathf.Pow(n, 2.0f) - 1.0f, 2.0f) * (6.0f + 3.0f * pn)) / (3.0f * N * Mathf.Pow(lambda.y, 4.0f) * (6.0f - 7.0f * pn)),
                (8.0f * Mathf.Pow(Mathf.PI, 3.0f) * Mathf.Pow(Mathf.Pow(n, 2.0f) - 1.0f, 2.0f) * (6.0f + 3.0f * pn)) / (3.0f * N * Mathf.Pow(lambda.z, 4.0f) * (6.0f - 7.0f * pn)));
            return result;
        }

        static Vector3 totalMie(Vector3 lambda, Vector3 K, float T)
        {
            float c = (0.2f * T) * 10E-17f;
            Vector3 result = new Vector3(
                0.434f * c * Mathf.PI * Mathf.Pow((2.0f * Mathf.PI) / lambda.x, 2.0f) * K.x,
                0.434f * c * Mathf.PI * Mathf.Pow((2.0f * Mathf.PI) / lambda.y, 2.0f) * K.y,
                0.434f * c * Mathf.PI * Mathf.Pow((2.0f * Mathf.PI) / lambda.z, 2.0f) * K.z
            );
            return result;
        }
    }
}