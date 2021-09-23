using UnityEngine;
using Object = UnityEngine.Object;

namespace ModelShark
{
    public static class CanvasHelper
    {
        /// <summary>Searches all canvases in the scene and tries to find the root one (isRootCanvas TRUE).</summary>
        /// <remarks>NOTE: This method uses FindObjectsOfType(), which is slow. Do not call this method often.</remarks>
        public static Canvas GetRootCanvas()
        {
            Canvas[] canvases = Object.FindObjectsOfType<Canvas>();
            if (canvases.Length == 0)
            {
                Debug.LogError("No canvas found in scene.");
                return null;
            }
            for (int i = 0; i < canvases.Length; i++)
            {
                if (canvases[i].isRootCanvas)
                    return canvases[i];
            }

            Debug.LogError("No canvas found at the root level of the scene.");
            return null;
        }
    }
}
