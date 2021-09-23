using System;
using UnityEngine;

namespace ModelShark
{
    /// <summary>Pops up a tooltip when the scene starts.</summary>
    [RequireComponent(typeof(PopupTooltipRemotely))]
    public class HiThereStartupTooltip : MonoBehaviour
    {
        public GameObject popupOnObject;

        private void Start()
        {
            // Check if player has already seen the startup tooltip. If so, don't show it again.
            bool hasSeenStartupTip = PlayerPrefs.GetInt("HasSeenStartupTip", 0) != 0;
            if (hasSeenStartupTip) return;

            PopupTooltipRemotely popupFunction = gameObject.GetComponent<PopupTooltipRemotely>();
            if (popupFunction == null) return;

            popupFunction.PopupTooltip(popupOnObject, "<b>Hi there!\n</b>\nHover over the items you see here to discover what ProTips can do for your game.\n", "Continue");
            PlayerPrefs.SetInt("HasSeenStartupTip", 1);
        }
    }
}