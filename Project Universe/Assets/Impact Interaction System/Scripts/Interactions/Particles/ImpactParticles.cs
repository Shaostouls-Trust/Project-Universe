using Impact.Data;
using Impact.Utility.ObjectPool;
using UnityEngine;

namespace Impact.Interactions.Particles
{
    /// <summary>
    /// Standard implementation of ImpactParticlesBase.
    /// </summary>
    [AddComponentMenu("Impact/Impact Particles")]
    public class ImpactParticles : ImpactParticlesBase
    {
        /// <summary>
        /// Modes for setting how particles should be rotated.
        /// </summary>
        public enum ParticleRotationMode
        {
            /// <summary>
            /// Align the particles with the surface normal.
            /// </summary>
            AlignToNormal = 0,
            /// <summary>
            /// Rotate the particles to match the interaction velocity.
            /// </summary>
            AlignToVelocity = 1,
            /// <summary>
            /// Align the particles with the surface normal but also try to match the interaction velocity.
            /// </summary>
            AlignToNormalAndVelocity = 2,
            /// <summary>
            /// Don't rotate the particles.
            /// </summary>
            NoRotation = 3
        }

        [SerializeField]
        private ParticleRotationMode _rotationMode = ParticleRotationMode.AlignToNormal;
        [SerializeField]
        private AlignmentAxis _axis = AlignmentAxis.ZUp;
        [SerializeField]
        private int _poolSize = 50;
        [SerializeField]
        private ObjectPoolFallbackMode _poolFallbackMode = ObjectPoolFallbackMode.LowerPriority;

        private ParticleSystem[] particles;

        /// <summary>
        /// How the particles should be rotated.
        /// </summary>
        public ParticleRotationMode RotationMode
        {
            get { return _rotationMode; }
            set { _rotationMode = value; }
        }

        /// <summary>
        /// Which axis should be pointed towards the surface.
        /// </summary>
        public AlignmentAxis Axis
        {
            get { return _axis; }
            set { _axis = value; }
        }

        public override int PoolSize
        {
            get { return _poolSize; }
            set { _poolSize = value; }
        }

        public override ObjectPoolFallbackMode PoolFallbackMode
        {
            get { return _poolFallbackMode; }
            set { _poolFallbackMode = value; }
        }

        private void Awake()
        {
            particles = GetComponentsInChildren<ParticleSystem>();
        }

        public override void EmitParticles(ParticleInteractionResult interactionResult, Vector3 point, Vector3 normal)
        {
            UpdateTransform(point, normal, interactionResult.OriginalData.Velocity);

            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                particles[i].Play();
            }
        }

        public override void UpdateTransform(Vector3 point, Vector3 normal, Vector3 velocity)
        {
            transform.position = point;

            if (RotationMode == ParticleRotationMode.AlignToNormal)
            {
                transform.rotation = AlignmentAxisUtilities.GetRotationForAlignment(Axis, normal);
            }
            else if (RotationMode == ParticleRotationMode.AlignToVelocity)
            {
                transform.rotation = AlignmentAxisUtilities.GetRotationForAlignment(Axis, velocity);
            }
            else if (RotationMode == ParticleRotationMode.AlignToNormalAndVelocity)
            {
                transform.rotation = AlignmentAxisUtilities.GetVelocityRotation(Axis, normal, velocity);
            }
        }

        public override void Stop()
        {
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Stop();
            }
        }

        private void Update()
        {
            if (!IsAvailable())
            {
                bool isAlive = false;
                for (int i = 0; i < particles.Length; i++)
                {
                    if (particles[i].IsAlive())
                        isAlive = true;
                }

                if (!isAlive)
                    MakeAvailable();
            }
        }
    }
}
