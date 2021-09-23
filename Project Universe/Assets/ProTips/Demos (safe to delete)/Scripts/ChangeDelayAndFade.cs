using UnityEngine;
using UnityEngine.UI;

namespace ModelShark
{
    /// <summary>
    /// This script shows you how to change the tooltip DELAY TIME and FADE DURATION dynamically through code.
    /// Place this on a game object in the scene, and your slider (or other UI element) calls it for its "Change" event.
    /// </summary>
    public class ChangeDelayAndFade : MonoBehaviour
    {
        public Text delayText;
        public Text fadeText;

        public void UpdateDelayLength(float delayTime)
        {
            TooltipManager.Instance.tooltipDelay = delayTime;

            delayText.text = delayTime.ToString("F1") + "s";
        }

        public void UpdateFadeDuration(float duration)
        {
            TooltipManager.Instance.fadeDuration = duration;

            fadeText.text = duration.ToString("F1") + "s";
        }
    }
}