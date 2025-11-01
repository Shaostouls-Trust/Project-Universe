using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.Orion.ProceduralPlanets
{
    public class HatchButton : Interactable
    {

        protected override void ShowInteractMessage()
        {
            Ship ship = FindFirstObjectByType<Ship>();
            string action = (ship.HatchOpen) ? "close" : "open";
            string contextualMessage = $"Press F to {action} hatch";
            GameUI.DisplayInteractionInfo(contextualMessage);
        }

        protected override void Interact()
        {
            base.Interact();
            ShowInteractMessage();
        }

        void OnValidate()
        {
            interactMessage = "#set from script#";
        }
    }
}