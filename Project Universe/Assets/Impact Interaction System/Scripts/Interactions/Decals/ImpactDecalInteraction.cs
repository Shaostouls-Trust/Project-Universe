using Impact.Utility;
using UnityEngine;

namespace Impact.Interactions.Decals
{
    /// <summary>
    /// Interaction for placing decals.
    /// </summary>
    [CreateAssetMenu(fileName = "New Impact Decal Interaction", menuName = "Impact/Decal Interaction", order = 2)]
    public class ImpactDecalInteraction : ImpactInteractionBase
    {
        private const string interactionResultPoolKey = "DecalInteractionResult";

        [SerializeField]
        private float _minimumVelocity = 2;
        [SerializeField]
        private float _collisionNormalInfluence = 1;
        [SerializeField]
        private ImpactDecalBase _decalPrefab;
        [SerializeField]
        private Range _creationInterval = new Range(0.5f, 0.5f);
        [SerializeField]
        private InteractionIntervalType _creationIntervalType = InteractionIntervalType.Distance;
        [SerializeField]
        private bool _createOnCollision = true;
        [SerializeField]
        private bool _createOnSlide;
        [SerializeField]
        private bool _createOnRoll;

        /// <summary>
        /// The minimum velocity required to place a decal.
        /// </summary>
        public float MinimumVelocity
        {
            get { return _minimumVelocity; }
            set { _minimumVelocity = value; }
        }

        /// <summary>
        /// How much the normal should influence the collision intensity.
        /// </summary>
        public float CollisionNormalInfluence
        {
            get { return _collisionNormalInfluence; }
            set { _collisionNormalInfluence = value; }
        }

        /// <summary>
        /// The decal prefab to use.
        /// </summary>
        public ImpactDecalBase DecalPrefab
        {
            get { return _decalPrefab; }
            set { _decalPrefab = value; }
        }

        /// <summary>
        /// The random interval at which to place decals when sliding or rolling.
        /// </summary>
        public Range CreationInterval
        {
            get { return _creationInterval; }
            set { _creationInterval = value; }
        }

        /// <summary>
        /// Whether the creation interval is by time or distance.
        /// </summary>
        public InteractionIntervalType CreationIntervalType
        {
            get { return _creationIntervalType; }
            set { _creationIntervalType = value; }
        }

        /// <summary>
        /// Should decals be created for single collisions?
        /// </summary>
        public bool CreateOnCollision
        {
            get { return _createOnCollision; }
            set { _createOnCollision = value; }
        }

        /// <summary>
        /// Should decals be created when sliding?
        /// </summary>
        public bool CreateOnSlide
        {
            get { return _createOnSlide; }
            set { _createOnSlide = value; }
        }

        /// <summary>
        /// Should decals be created when rolling?
        /// </summary>
        public bool CreateOnRoll
        {
            get { return _createOnRoll; }
            set { _createOnRoll = value; }
        }

        /// <summary>
        /// Creates a new DecalInteractionResult from the given IInteractionData.
        /// </summary>
        /// <param name="interactionData">The data to use to create the result.</param>
        /// <returns>A new DecalInteractionResult.</returns>
        public override IInteractionResult GetInteractionResult<T>(T interactionData)
        {
            //Immediately break out if intensity is less than the velocity minimum, since any result would be invalid anyways.
            float intensity = ImpactInteractionUtilities.GetCollisionIntensity(interactionData, CollisionNormalInfluence);
            if (intensity < MinimumVelocity)
                return null;

            long key = 0;
            if (!ImpactInteractionUtilities.GetKeyAndValidate(interactionData, this, out key))
                return null;

            DecalInteractionResult c;

            if (shouldEmit(interactionData.InteractionType) && ImpactManagerInstance.TryGetInteractionResultFromPool(interactionResultPoolKey, out c))
            {
                c.Key = key;
                c.DecalTemplate = DecalPrefab;
                c.OriginalData = InteractionDataUtilities.ToInteractionData(interactionData);

                c.CreationInterval = CreationInterval;
                c.CreationIntervalType = CreationIntervalType;

                return c;
            }

            return null;
        }

        private bool shouldEmit(int collisionParameterType)
        {
            return (collisionParameterType == InteractionData.InteractionTypeCollision && CreateOnCollision) ||
                (collisionParameterType == InteractionData.InteractionTypeSlide && CreateOnSlide) ||
                (collisionParameterType == InteractionData.InteractionTypeRoll && CreateOnRoll);
        }

        /// <summary>
        /// Creates an object pool instance for the decal prefab.
        /// </summary>
        public override void Preload()
        {
            ImpactDecalPool.PreloadPoolForDecal(DecalPrefab);
            ImpactManagerInstance.CreateInteractionResultPool<Interactions.Decals.DecalInteractionResult>(interactionResultPoolKey);
        }
    }
}