using System;
using UnityEngine;

namespace ModelShark
{
    /// <summary>
    /// This is an example of how to pop up a tooltip on any object from an event. This tooltip will stay open and block all other tooltips
    /// until the use clicks the Close button. It illustrates how you could make a system of tutorial tooltips that teach a player how to
    /// play your game, and also shows how you can have interactable tooltips.
    /// </summary>
    public class PopupTooltipRemotely : MonoBehaviour
    {
        /// <summary>Triggers a tooltip immediately on the game object specified.</summary>
        /// <param name="onObject">The game object to pop a tooltip over.</param>
        public void PopupTooltip(GameObject onObject, string bodyText, string buttonText)
        {
            // Add the TooltipTrigger component to the object we want to pop a tooltip up for.
            TooltipTrigger tooltipTrigger = onObject.GetComponent<TooltipTrigger>();

            if (tooltipTrigger == null)
                tooltipTrigger = onObject.AddComponent<TooltipTrigger>();

            TooltipStyle tooltipStyle = Resources.Load<TooltipStyle>("CleanSimpleCloseButton");
            tooltipTrigger.tooltipStyle = tooltipStyle;

            // Set the tooltip text and properties.
            tooltipTrigger.SetText("BodyText", bodyText);
            tooltipTrigger.SetText("ButtonText", String.IsNullOrEmpty(buttonText) ? "Continue" : buttonText);
            tooltipTrigger.tipPosition = TipPosition.TopRightCorner;
            tooltipTrigger.maxTextWidth = 300;
            tooltipTrigger.staysOpen = true; // make this a tooltip that stays open...
            tooltipTrigger.isBlocking = true; // ...and is blocking (no other tooltips allowed while this one is active).

            // Popup the tooltip and give it the object that triggered it (the Canvas in this case).
            tooltipTrigger.Popup(8f, gameObject);
        }

        public void PopupTooltip(GameObject onObject)
        {
            PopupTooltip(onObject, "This tooltip was triggered from an event. A tooltip like this is useful for in-game tutorials and NPC \"barks.\"", "Continue");
        }
    }
}