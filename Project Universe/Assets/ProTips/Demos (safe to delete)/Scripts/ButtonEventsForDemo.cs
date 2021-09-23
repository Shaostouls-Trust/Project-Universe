using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ModelShark
{
    public class ButtonEventsForDemo : MonoBehaviour
    {
        public Text renderModeText;
        public Canvas canvas;
        public RectTransform rectToTilt;
        public ParticleSystem particles;

        public void Start()
        {
            Time.timeScale = 1f; // Unpause time if it was paused in another scene.
            if (TooltipManager.Instance != null)
                canvas = TooltipManager.Instance.GuiCanvas;

            if (renderModeText != null)
            {
                switch (canvas.renderMode)
                {
                    case RenderMode.ScreenSpaceOverlay:
                        renderModeText.text = "Screen Space Overlay";
                        break;
                    case RenderMode.ScreenSpaceCamera:
                        renderModeText.text = "Screen Space Camera";
                        break;
                }
            }
        }

        public void BackToDemo()
        {
            SceneManager.LoadScene(0);
        }

        public void GoToTemplates()
        {
            SceneManager.LoadScene("Tooltip Styles");
        }

        public void GoTo3DWorldObjectDemo()
        {
            SceneManager.LoadScene("3D World Object Demo");
        }

        /// <summary>Pauses time for the current scene by setting Time.timeScale = 0.</summary>
        public void PauseTime(Text displayText)
        {
            displayText.text = displayText.text == "Pause Time" ? "Unpause Time" : "Pause Time";

            Time.timeScale = Time.timeScale < 1f ? 1f : 0f;
        }

        public void ChangeCanvasRenderMode()
        {
            switch (canvas.renderMode)
            {
                case RenderMode.ScreenSpaceOverlay:
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    canvas.worldCamera = TooltipManager.Instance.guiCamera;
                    renderModeText.text = "Screen Space Camera";
                    rectToTilt.localEulerAngles = new Vector3(0, 350, 0);
                    rectToTilt.anchoredPosition = new Vector2(70, 0);
                    break;
                case RenderMode.ScreenSpaceCamera:
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    renderModeText.text = "Screen Space Overlay";
                    rectToTilt.rotation = Quaternion.identity;
                    rectToTilt.anchoredPosition = new Vector2(0, 0);
                    break;
            }
            TooltipManager.Instance.ResetTooltipRotation();
        }

        public void ToggleParticles(Text displayText)
        {
            if (particles.gameObject.activeSelf)
            {
                particles.gameObject.SetActive(false);
                displayText.text = "Start Particles";
            }
            else
            {
                particles.gameObject.SetActive(true);
                displayText.text = "Stop Particles";
            }
        }
    }
}