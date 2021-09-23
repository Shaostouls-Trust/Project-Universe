using UnityEngine;

namespace Impact
{
    /// <summary>
    /// An implementation of the IInteractionData interface.
    /// </summary>
    public struct InteractionData : IInteractionData
    {
        /// <summary>
        /// Interaction type for single collisions.
        /// </summary>
        public const int InteractionTypeCollision = 0;
        /// <summary>
        /// Interaction type for sliding collisions.
        /// </summary>
        public const int InteractionTypeSlide = 1;
        /// <summary>
        /// Interaction type for rolling collisions.
        /// </summary>
        public const int InteractionTypeRoll = 2;
        /// <summary>
        /// Interaction type for simple collisions (i.e. collisions with no velocity, normal, object, or material data).
        /// </summary>
        public const int InteractionTypeSimple = 3;

        /// <summary>
        /// Integer representing the type of interaction such as Collision, Slide, Roll, and Simple.
        /// It is up to interactions to determine how this value affects the interaction.
        /// </summary>
        public int InteractionType { get; set; }

        /// <summary>
        /// The velocity of the interaction.
        /// </summary>
        public Vector3 Velocity { get; set; }
        /// <summary>
        /// The contact point at which the interaction occured.
        /// </summary>
        public Vector3 Point { get; set; }
        /// <summary>
        /// The normal of the surface where the interaction occured.
        /// </summary>
        public Vector3 Normal { get; set; }

        /// <summary>
        /// The tags retrieved from the object we are interacting with, represented as an integer bitmask.  
        /// This can be null if we were not able to get the object  we are interacting with.
        /// </summary>
        public ImpactTagMask? TagMask { get; set; }
        /// <summary>
        /// If using material composition, a 0 to 1 value representing the influence of the material at the contact point.
        /// </summary>
        public float CompositionValue { get; set; }

        /// <summary>
        /// If specified, this will override the Priority value of the Impact Object.
        /// </summary>
        public int? PriorityOverride { get; set; }

        /// <summary>
        /// The object these parameters are being sent from.
        /// </summary>
        public GameObject ThisObject { get; set; }
        /// <summary>
        /// The object we are interacting with.
        /// </summary>
        public GameObject OtherObject { get; set; }

        /// <summary>
        /// Create new interaction data with values copied from this interaction data.
        /// </summary>
        public IInteractionData Clone()
        {
            InteractionData i = new InteractionData();
            i.InteractionType = InteractionType;
            i.Velocity = Velocity;
            i.Point = Point;
            i.Normal = Normal;
            i.TagMask = TagMask;
            i.CompositionValue = CompositionValue;
            i.PriorityOverride = PriorityOverride;
            i.ThisObject = ThisObject;
            i.OtherObject = OtherObject;

            return i;
        }

        public override string ToString()
        {
            return $"{ThisObject.name} >> {OtherObject.name} : {ImpactDebugger.InteractionTypeToString(InteractionType)} : Point - {Point} : Velocity - {Velocity} : Normal - {Normal} : Tag Mask - {TagMask} : Composition Value - {CompositionValue} : Priority - {PriorityOverride}";
        }
    }

    /// <summary>
    /// Utility class for InteractionData.
    /// </summary>
    public static class InteractionDataUtilities
    {
        /// <summary>
        /// Converts the given IInteractionData implementation to InteractionData. 
        /// </summary>
        /// <typeparam name="T">The IInteractionData type.</typeparam>
        /// <param name="original">The original IInteractionData object.</param>
        /// <returns>A new InteractionData object.</returns>
        public static InteractionData ToInteractionData<T>(T original) where T : IInteractionData
        {
            InteractionData i = new InteractionData();
            i.InteractionType = original.InteractionType;
            i.Velocity = original.Velocity;
            i.Point = original.Point;
            i.Normal = original.Normal;
            i.TagMask = original.TagMask;
            i.CompositionValue = original.CompositionValue;
            i.PriorityOverride = original.PriorityOverride;
            i.ThisObject = original.ThisObject;
            i.OtherObject = original.OtherObject;

            return i;
        }
    }
}