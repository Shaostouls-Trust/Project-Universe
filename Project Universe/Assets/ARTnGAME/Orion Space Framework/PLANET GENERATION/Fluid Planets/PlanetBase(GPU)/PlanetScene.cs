using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Artngame.Orion.FluidPlanetGenerator
{
    [ExecuteInEditMode]
    public class PlanetScene : MonoBehaviour
    {

        public ComputeShader ComputeShader;
        public Shader VisualShader;

        public int Resolution;
        public Material PlanetMaterial;
        public Material PlanetMaterialA;

        public Texture2D DyeTexture;
        public Texture2D Velocity;

        FluidSimulation FluidSimulation;

        // Use this for initialization
        void Start()
        {
            if (Application.isPlaying)
            {
                FluidSimulation = new FluidSimulation();
                FluidSimulation.ComputeShader = ComputeShader;

                FluidSimulation.setResolution(Resolution);

                FluidSimulation.SetDyeTexture(DyeTexture);
                FluidSimulation.setVelocity(Velocity);

                PlanetMaterial.SetTexture("_MainTex", FluidSimulation._colorRT1);

                Vector4[] aoKernel = new Vector4[16];
                for (int i = 0; i < 16; i++)
                {

                    aoKernel[i] = new Vector4(Random.Range(0, 2.0f) - 1, Random.Range(0, 2.0f) - 1, 0, 0);

                }

                //PlanetMaterial.SetVectorArray("_AOKernel",aoKernel);
            }

        }

        public bool saveTextureToDisk = false;
#if UNITY_EDITOR
        public static void SaveTextureAsPNG(Texture2D _texture, string name)
        {
            var dirPath = Application.streamingAssetsPath + "/SavedImages/"+ name + ".png";
            //if (!Directory.Exists(dirPath))
            //{
            //    Directory.CreateDirectory(dirPath);
            //}
            byte[] _bytes = _texture.EncodeToPNG();
            System.IO.File.WriteAllBytes(dirPath, _bytes);
            Debug.Log(_bytes.Length / 1024 + "Kb was saved as: " + dirPath);
        }
        Texture2D toTexture2D(RenderTexture rTex)
        {
            Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
            // ReadPixels looks at the active RenderTexture.
            RenderTexture.active = rTex;
            tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
            tex.Apply();
            return tex;
        }
#endif
        // Update is called once per frame
        void Update()
        {

#if UNITY_EDITOR
            if (saveTextureToDisk)
            {
                SaveTextureAsPNG(toTexture2D(FluidSimulation._colorRT1), "FluidPlanetA");
                saveTextureToDisk = false;
            }
#endif

            if (Application.isPlaying)
            {
                FluidSimulation.SimStep(Time.deltaTime);

                // Not sure if this is really necessary
                PlanetMaterial.SetTexture("_MainTex", FluidSimulation._colorRT1);
                PlanetMaterial.SetTexture("_Velo", FluidSimulation._velRT2);

                PlanetMaterialA.SetTexture("_MainTex", FluidSimulation._velRT2);

                // PlanetRotation
                //transform.localEulerAngles+= new Vector3(0,1,0)*Time.deltaTime*30;
            }

        }


    }
}