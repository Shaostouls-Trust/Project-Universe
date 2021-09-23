using System;
using UnityEngine;

namespace ModelShark
{
    public class AddTooltipDynamically : MonoBehaviour
    {
        private void Start()
        {
            ModifyExistingTooltip();
            AddNewTooltip();
        }

        /// <summary>
        /// This is an example of creating a button at runtime from a prefab that already has a tooltip on it, 
        /// and modifying the tooltip to display whatever dynamic message we want.
        /// </summary>
        private void ModifyExistingTooltip()
        {
            // Load the button from Resources.
            GameObject buttonPrefab = Resources.Load<GameObject>("DynamicObjWithTooltip");
            if (buttonPrefab != null)
            {
                // Assuming the prefab exists, instantiate the button and set its parent.
                GameObject button = Instantiate(buttonPrefab);
                button.transform.SetParent(transform, false);

                // Get the TooltipTrigger component on the button.
                TooltipTrigger tooltipTrigger = button.GetComponent<TooltipTrigger>();
                if (tooltipTrigger != null) // Set the tooltip text.
                    tooltipTrigger.SetText("BodyText", String.Format("This object was created at <b><color=#0049CE>runtime</color></b> from a prefab that already had a tooltip on it, and the tooltip message was modified programmatically.\n\nObject created and tooltip text assigned at {0}.", DateTime.Now));
            }
        }

        /// <summary>
        /// This is an example of creating a button at runtime from a prefab that does NOT already have a tooltip, 
        /// programmatically adding a tooltip to it that does not already exist in the scene, and modifying it to 
        /// display whatever dynamic message we want.
        /// </summary>
        private void AddNewTooltip()
        {
            // Load the button from Resources.
            GameObject buttonPrefab = Resources.Load<GameObject>("DynamicObjWithoutTooltip");
            if (buttonPrefab != null)
            {
                // Assuming the prefab exists, instantiate the button and set its parent.
                GameObject button = Instantiate(buttonPrefab);
                button.transform.SetParent(transform, false);

                // Add the TooltipTrigger component to the button.
                TooltipTrigger tooltipTrigger = button.gameObject.AddComponent<TooltipTrigger>();
                TooltipStyle tooltipStyle = Resources.Load<TooltipStyle>("MetroSimple");
                tooltipTrigger.tooltipStyle = tooltipStyle;

                // Set the tooltip text.
                tooltipTrigger.SetText("BodyText", String.Format("This object was created at <b><color=#F3B200>runtime</color></b> from a prefab that <b><color=#F3B200>did not</color></b> already have a tooltip on it. The tooltip was added programmatically and the message and other parameters modified through code. This \"metro\" tooltip style was also added dynamically to the scene.\n\nObject created and tooltip text assigned at {0}.", DateTime.Now));

                // Set some extra style properties on the tooltip
                tooltipTrigger.maxTextWidth = 250;
                tooltipTrigger.backgroundTint = Color.white;
                tooltipTrigger.tipPosition = TipPosition.BottomRightCorner;
            }
        }
    }
}