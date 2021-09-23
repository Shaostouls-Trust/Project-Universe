using Impact.Objects;
using Impact.Utility;
using Impact.Utility.ObjectPool;
using UnityEngine;

namespace Impact.Interactions.Particles
{
    /// <summary>
    /// The result of a particle interaction.
    /// Handles emitting of particles for single collisions and sliding and rolling.
    /// </summary>
    public class ParticleInteractionResult : IContinuousInteractionResult, IPoolable
    {
        /// <summary>
        /// The original interaction data this result was created from.
        /// </summary>
        public InteractionData OriginalData { get; set; }

        /// <summary>
        /// The particles prefab to use.
        /// </summary>
        public ImpactParticlesBase ParticlesTemplate;
        /// <summary>
        /// Are the particles looped?
        /// </summary>
        public bool IsParticleLooped;
        /// <summary>
        /// Random range for emission interval when sliding or rolling.
        /// </summary>
        public Range EmissionInterval;
        /// <summary>
        /// Whether the emission interval is by time or distance.
        /// </summary>
        public InteractionIntervalType EmissionIntervalType;

        /// <summary>
        /// The result key for updating sliding and rolling.
        /// </summary>
        public long Key { get; set; }

        /// <summary>
        /// Does this result have a particles template?
        /// </summary>
        public bool IsValid
        {
            get { return ParticlesTemplate != null; }
        }

        /// <summary>
        /// Has this result been updated within the last FixedUpdate call?
        /// </summary>
        public bool IsAlive { get; private set; }

        private ImpactParticlesBase particlesInstance;

        private float intervalCounter;
        private float currentEmissionIntervalTarget;
        private Vector3 previousEmissionPosition;

        private IImpactObject parent;

        private bool isAvailable = true;

        /// <summary>
        /// Emit particles using our data.
        /// </summary>
        /// <param name="parent">The Impact Object that created this result.</param>
        public void Process(IImpactObject parent)
        {
            this.parent = parent;

            particlesInstance = ImpactParticlePool.EmitParticles(this, OriginalData.Point, OriginalData.Normal, InteractionResultExtensions.GetPriority(OriginalData.PriorityOverride, parent));
            IsAlive = true;

            currentEmissionIntervalTarget = EmissionInterval.RandomInRange();

            //Dispose immediately for Collision interaction types
            if (OriginalData.InteractionType == InteractionData.InteractionTypeCollision)
                Dispose();
        }

        /// <summary>
        /// Update IsAlive.
        /// </summary>
        public void FixedUpdate()
        {
            IsAlive = false;
        }

        /// <summary>
        /// Updates for sliding and rolling and emits new particles if the particles are not looped.
        /// Will stop emitting if the new parameters don't produce valid data.
        /// </summary>
        /// <param name="newResult">The updated result.</param>
        public void KeepAlive(IInteractionResult newResult)
        {
            IsAlive = true;

            ParticleInteractionResult particleInteractionResult = newResult as ParticleInteractionResult;

            if (IsParticleLooped && particlesInstance != null)
            {
                particlesInstance.UpdateTransform(particleInteractionResult.OriginalData.Point, particleInteractionResult.OriginalData.Normal, particleInteractionResult.OriginalData.Velocity);
            }
            else
            {
                if (EmissionIntervalType == InteractionIntervalType.Time)
                    intervalCounter += Time.fixedDeltaTime;
                else
                    intervalCounter = Vector3.Distance(particleInteractionResult.OriginalData.Point, previousEmissionPosition);

                if (intervalCounter >= currentEmissionIntervalTarget)
                {
                    currentEmissionIntervalTarget = EmissionInterval.RandomInRange();
                    particlesInstance = ImpactParticlePool.EmitParticles(this, particleInteractionResult.OriginalData.Point, particleInteractionResult.OriginalData.Normal, parent.Priority);

                    intervalCounter = 0;
                    previousEmissionPosition = particleInteractionResult.OriginalData.Point;
                }
            }
        }

        /// <summary>
        /// Stops the particle systems.
        /// </summary>
        public void Dispose()
        {
            if (particlesInstance != null && OriginalData.InteractionType != InteractionData.InteractionTypeCollision)
                particlesInstance.Stop();

            IsAlive = false;
            ParticlesTemplate = null;

            MakeAvailable();
        }

        public void Retrieve()
        {
            isAvailable = false;
        }

        public void MakeAvailable()
        {
            isAvailable = true;
        }

        public bool IsAvailable()
        {
            return isAvailable;
        }
    }
}