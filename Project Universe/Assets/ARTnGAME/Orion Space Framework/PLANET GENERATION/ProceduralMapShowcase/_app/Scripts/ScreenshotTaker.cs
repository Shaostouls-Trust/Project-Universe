using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Artngame.Orion.MapGen2D
{
    public class ScreenshotTaker : MonoBehaviour
    {
        public MapGeneratorA mapGenerator;
        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                ScreenCapture.CaptureScreenshot("ProcgenScreenshots/" + SceneManager.GetActiveScene().name + "_" + mapGenerator.m_seed + ".png");
            }
        }
    }
}