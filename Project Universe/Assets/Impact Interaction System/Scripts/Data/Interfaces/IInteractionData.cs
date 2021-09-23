using UnityEngine;

namespace Impact
{
    /// <summary>
    /// Contains data relating to an interaction.
    /// </summary>
    public interface IInteractionData
    {
        /// <summary>
        /// Integer representing the type of interaction such as Collision, Slide, Roll, and Simple.
        /// It is up to interactions to determine how this value affects the interaction.
        /// </summary>
        int InteractionType { get; set; }

        /// <summary>
        /// The Velocity of the interaction.
        /// </summary>
        Vector3 Velocity { get; set; }
        /// <summary>
        /// The contact point at which the interaction occured.
        /// </summary>
        Vector3 Point { get; set; }
        /// <summary>
        /// The normal of the surface where the interaction occured.
        /// </summary>
        Vector3 Normal { get; set; }

        /// <summary>
        /// The tags retrieved from the object we are interacting with, represented as an integer bitmask. 
        /// This can be null if we were not able to get the object being interacted with.
        /// </summary>
        ImpactTagMask? TagMask { get; set; }
        /// <summary>
        /// If using material composition, a 0 to 1 value representing the influence of the material at the contact point.
        /// </summary>
        float CompositionValue { get; set; }

        /// <summary>
        /// If specified, this will override the Priority value of the Impact Object.
        /// </summary>
        int? PriorityOverride { get; set; }

        /// <summary>
        /// The object these parameters are being sent from.
        /// </summary>
        GameObject ThisObject { get; set; }
        /// <summary>
        /// The object we are interacting with.
        /// </summary>
        GameObject OtherObject { get; set; }

        /// <summary>
        /// Create new interaction data with values copied from this interaction data.
        /// </summary>
        IInteractionData Clone();
    }
}