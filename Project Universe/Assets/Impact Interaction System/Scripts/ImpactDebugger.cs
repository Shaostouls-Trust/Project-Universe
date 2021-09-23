using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Impact
{
    public static class ImpactDebugger
    {
        public static void LogInteraction<T>(T obj) where T : IInteractionData
        {
            Debug.Log($"[IMPACT] {obj}");
            Debug.DrawRay(obj.Point, obj.Normal.normalized, Color.red);
            Debug.DrawRay(obj.Point, -obj.Velocity, Color.white);
        }

        public static string InteractionTypeToString(int interactionType)
        {
            if (interactionType == 0)
                return "Collision";
            else if (interactionType == 1)
                return "Slide";
            else if (interactionType == 2)
                return "Roll";
            else if (interactionType == 3)
                return "Simple";

            return "Other";
        }
    }
}

