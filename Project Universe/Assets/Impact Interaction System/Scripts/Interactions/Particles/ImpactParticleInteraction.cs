using Impact.Utility;
using UnityEngine;

namespace Impact.Interactions.Particles
{
    /// <summary>
    /// Interaction for showing particle effects.
    /// </summary>
    [CreateAssetMenu(fileName = "New Impact Particle Interaction", menuName = "Impact/Particle Interaction", order = 2)]
    public class ImpactParticleInteraction : ImpactInteractionBase
    {
        private const string interactionResultPoolKey = "ParticleInteractionResult";

        [SerializeField]
        private float _minimumVelocity = 2;
        [SerializeField]
        private float _collisionNormalInfluence = 1;
        [SerializeField]
        private ImpactParticlesBase _particlePrefab;
        [SerializeField]
        private bool _isParticleLooped;
        [SerializeField]
        private Range _emissionInterval = new Range(0.2f, 0.3f);
        [SerializeField]
        private InteractionIntervalType _emissionIntervalType = InteractionIntervalType.Time;
        [SerializeField]
        private bool _emitOnCollision = true;
        [SerializeField]
        private bool _emitOnSlide;
        [SerializeField]
        private bool _emitOnRoll;

        /// <summary>
        /// The minimum velocity required to emit particles.
        /// </summary>
        public float MinimumVelocity
        {
            get { return _minimumVelocity; }
            set { _minimumVelocity = value; }
        }

        /// <summary>
        /// How much the collision normal affects the collision intensity.
        /// </summary>
        public float CollisionNormalInfluence
        {
            get { return _collisionNormalInfluence; }
            set { _collisionNormalInfluence = value; }
        }

        /// <summary>
        /// The particle prefab to use.
        /// </summary>
        public ImpactParticlesBase ParticlePrefab
        {
            get { return _particlePrefab; }
            set { _particlePrefab = value; }
        }

        /// <summary>
        /// Is the particle prefab looped?
        /// </summary>
        public bool IsParticleLooped
        {
            get { return _isParticleLooped; }
            set { _isParticleLooped = value; }
        }

        /// <summary>
        /// The interval at which particles should be emitted when sliding or rolling, and the particle is NOT looped.
        /// </summary>
        public Range EmissionInterval
        {
            get { return _emissionInterval; }
            set { _emissionInterval = value; }
        }

        /// <summary>
        /// Whether the emission interval is by time or distance.
        /// </summary>
        public InteractionIntervalType EmissionIntervalType
        {
            get { return _emissionIntervalType; }
            set { _emissionIntervalType = value; }
        }

        /// <summary>
        /// Should particles be emitted for single collisions?
        /// </summary>
        public bool EmitOnCollision
        {
            get { return _emitOnCollision; }
            set { _emitOnCollision = value; }
        }

        /// <summary>
        /// Should particles be emitted when sliding?
        /// </summary>
        public bool EmitOnSlide
        {
            get { return _emitOnSlide; }
            set { _emitOnSlide = value; }
        }

        /// <summary>
        /// Should particles be emitted when rolling?
        /// </summary>
        public bool EmitOnRoll
        {
            get { return _emitOnRoll; }
            set { _emitOnRoll = value; }
        }

        /// <summary>
        /// Creates a new ParticleInteractionResult from the given IInteractionData.
        /// </summary>
        /// <param name="interactionData">The data to use to create the result.</param>
        /// <returns>A new ParticleInteractionResult.</returns>
        public override IInteractionResult GetInteractionResult<T>(T interactionData)
        {
            //Immediately break out if intensity is less than the velocity minimum, since any result would be invalid anyways.
            float intensity = ImpactInteractionUtilities.GetCollisionIntensity(interactionData, CollisionNormalInfluence);
            if (intensity < MinimumVelocity)
                return null;

            long key = 0;
            if (!ImpactInteractionUtilities.GetKeyAndValidate(interactionData, this, out key))
                return null;

            ParticleInteractionResult c;

            if (shouldEmit(interactionData.InteractionType) && ImpactManagerInstance.TryGetInteractionResultFromPool(interactionResultPoolKey, out c))
            {
                c.Key = key;
                c.ParticlesTemplate = ParticlePrefab;
                c.OriginalData = InteractionDataUtilities.ToInteractionData(interactionData);

                c.IsParticleLooped = IsParticleLooped;
                c.EmissionInterval = EmissionInterval;
                c.EmissionIntervalType = EmissionIntervalType;

                return c;
            }

            return null;
        }

        private bool shouldEmit(int collisionParameterType)
        {
            return (collisionParameterType == InteractionData.InteractionTypeCollision && EmitOnCollision) ||
                (collisionParameterType == InteractionData.InteractionTypeSlide && EmitOnSlide) ||
                (collisionParameterType == InteractionData.InteractionTypeRoll && EmitOnRoll);
        }

        /// <summary>
        /// Creates an object pool instance for the particle prefab.
        /// </summary>
        public override void Preload()
        {
            ImpactParticlePool.PreloadPoolForParticle(ParticlePrefab);
            ImpactManagerInstance.CreateInteractionResultPool<Interactions.Particles.ParticleInteractionResult>(interactionResultPoolKey);
        }
    }
}