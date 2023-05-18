using UnityEditor;
using UnityEngine;
namespace Artngame.Orion.ProceduralPlanets
{
    [InitializeOnLoad]
    public static class SceneCamHelper
    {

        [MenuItem("Edit/Camera/SetPivot")]
        static void SetCam()
        {
            SceneView.lastActiveSceneView.FrameSelected();
            SceneView.lastActiveSceneView.size = 2.5f;
        }

    }
}