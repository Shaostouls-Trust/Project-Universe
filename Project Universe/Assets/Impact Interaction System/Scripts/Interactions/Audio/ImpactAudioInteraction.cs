using Impact.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace Impact.Interactions.Audio
{
    /// <summary>
    /// Interaction for playing audio.
    /// </summary>
    [CreateAssetMenu(fileName = "New Impact Audio Interaction", menuName = "Impact/Audio Interaction", order = 2)]
    public class ImpactAudioInteraction : ImpactInteractionBase
    {
        private const string interactionResultPoolKey = "AudioInteractionResult";

        /// <summary>
        /// The mode for selecting collision audio clips.
        /// </summary>
        public enum CollisionAudioClipSelectionMode
        {
            /// <summary>
            /// Use the collision velocity to select the audio clip.
            /// </summary>
            Velocity,
            /// <summary>
            /// Randomly select the audio clip.
            /// </summary>
            Random
        }

        [SerializeField]
        private Range _velocityRange = new Range(2, 9);
        [SerializeField]
        private Range _randomPitchRange = new Range(0.9f, 1.1f);
        [SerializeField]
        private Range _randomVolumeRange = new Range(0.9f, 1.1f);

        [SerializeField]
        private bool _scaleVolumeWithVelocity = true;
        [SerializeField]
        private AnimationCurve _velocityVolumeScaleCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [SerializeField]
        private float _collisionNormalInfluence = 1;
        [SerializeField]
        private float _slideVelocityPitchMultiplier = 0.025f;

        [SerializeField]
        private ImpactAudioSourceBase _audioSourceTemplate;

        [SerializeField]
        private CollisionAudioClipSelectionMode _collisionAudioSelectionMode;
        [SerializeField]
        private List<AudioClip> _collisionAudioClips = new List<AudioClip>();

        [SerializeField]
        private AudioClip _slideAudioClip;
        [SerializeField]
        private AudioClip _rollAudioClip;

        /// <summary>
        /// The range input velocities will be compared to when calculating the collision intensity.
        /// </summary>
        public Range VelocityRange
        {
            get { return _velocityRange; }
            set { _velocityRange = value; }
        }

        /// <summary>
        /// Random range for multiplying the pitch.
        /// </summary>
        public Range RandomPitchRange
        {
            get { return _randomPitchRange; }
            set { _randomPitchRange = value; }
        }

        /// <summary>
        /// Random range for multiplying the volume.
        /// </summary>
        public Range RandomVolumeRange
        {
            get { return _randomVolumeRange; }
            set { _randomVolumeRange = value; }
        }

        /// <summary>
        /// Should the output volume be scaled relative to the velocity?
        /// </summary>
        public bool ScaleVolumeWithVelocity
        {
            get { return _scaleVolumeWithVelocity; }
            set { _scaleVolumeWithVelocity = value; }
        }

        /// <summary>
        /// The curve used to get the scalar value for the volume if ScaleVolumeWithVelocity is true.
        /// </summary>
        public AnimationCurve VelocityVolumeScaleCurve
        {
            get { return _velocityVolumeScaleCurve; }
            set { _velocityVolumeScaleCurve = value; }
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
        /// A value to increase the sliding and rolling pitch as the velocity increases.
        /// </summary>
        public float SlideVelocityPitchMultiplier
        {
            get { return _slideVelocityPitchMultiplier; }
            set { _slideVelocityPitchMultiplier = value; }
        }

        /// <summary>
        /// How collision audio clips should be selected.
        /// </summary>
        public CollisionAudioClipSelectionMode CollisionAudioSelectionMode
        {
            get { return _collisionAudioSelectionMode; }
            set { _collisionAudioSelectionMode = value; }
        }

        /// <summary>
        /// The audio source prefab to use values from when playing the audio interaction.
        /// </summary>
        public ImpactAudioSourceBase AudioSourceTemplate
        {
            get { return _audioSourceTemplate; }
            set { _audioSourceTemplate = value; }
        }

        /// <summary>
        /// The list of collision audio clips.
        /// </summary>
        public List<AudioClip> CollisionAudioClips
        {
            get { return _collisionAudioClips; }
        }

        /// <summary>
        /// The audio clip to play when sliding.
        /// </summary>
        public AudioClip SlideAudioClip
        {
            get { return _slideAudioClip; }
            set { _slideAudioClip = value; }
        }

        /// <summary>
        /// The audio clip to play when rolling.
        /// </summary>
        public AudioClip RollAudioClip
        {
            get { return _rollAudioClip; }
            set { _rollAudioClip = value; }
        }

        /// <summary>
        /// Creates a new AudioInteractionResult from the given IInteractionData.
        /// </summary>
        /// <param name="interactionData">The data to use to create the result.</param>
        /// <returns>A new AudioInteractionResult.</returns>
        public override IInteractionResult GetInteractionResult<T>(T interactionData)
        {
            //Immediately break out if intensity is less than the velocity range minimum, since any result would be invalid anyways.
            float intensity = ImpactInteractionUtilities.GetCollisionIntensity(interactionData, CollisionNormalInfluence);
            if (intensity < VelocityRange.Min)
                return null;

            long key = 0;
            if (!ImpactInteractionUtilities.GetKeyAndValidate(interactionData, this, out key))
                return null;

            AudioInteractionResult c;

            if (ImpactManagerInstance.TryGetInteractionResultFromPool(interactionResultPoolKey, out c))
            {
                c.OriginalData = InteractionDataUtilities.ToInteractionData(interactionData);
                c.LoopAudio = interactionData.InteractionType != InteractionData.InteractionTypeCollision;
                c.Interaction = this;
                c.AudioSourceTemplate = AudioSourceTemplate;

                if (interactionData.InteractionType == InteractionData.InteractionTypeSimple)
                {
                    c.AudioClip = getAudioClip(interactionData.InteractionType, 0);
                    c.Volume = RandomVolumeRange.RandomInRange();
                }
                else
                {
                    float normalizedIntensity = VelocityRange.Normalize(intensity);

                    c.AudioClip = getAudioClip(interactionData.InteractionType, normalizedIntensity);
                    c.Volume = getVolume(normalizedIntensity) * interactionData.CompositionValue;
                    c.Key = key;
                }

                c.Pitch = RandomPitchRange.RandomInRange();

                return c;
            }

            return null;
        }

        /// <summary>
        /// Gives an updated pitch based on an initial pitch and velocity.
        /// </summary>
        /// <param name="originalPitch">The pitch of the original result.</param>
        /// <param name="velocity">The current velocity.</param>
        /// <returns>The updated pitch based on the original pitch and velocity.</returns>
        public float UpdatePitch(float originalPitch, Vector3 velocity)
        {
            return originalPitch + velocity.magnitude * SlideVelocityPitchMultiplier;
        }

        private float getVolume(float normalizedIntensity)
        {
            if (ScaleVolumeWithVelocity)
            {
                return VelocityVolumeScaleCurve.Evaluate(normalizedIntensity) * RandomVolumeRange.RandomInRange();
            }
            else
            {
                return RandomVolumeRange.RandomInRange();
            }
        }

        private AudioClip getAudioClip(int interactionType, float normalizedIntensity)
        {
            if (interactionType == InteractionData.InteractionTypeSimple)
                return getRandomCollisionAudioClip();
            else if (interactionType == InteractionData.InteractionTypeCollision)
                return getCollisionAudioClip(normalizedIntensity);
            else if (interactionType == InteractionData.InteractionTypeSlide)
                return SlideAudioClip;
            else if (interactionType == InteractionData.InteractionTypeRoll)
                return RollAudioClip;

            return null;
        }

        private AudioClip getCollisionAudioClip(float normalizedIntensity)
        {
            if (CollisionAudioClips.Count == 0)
                return null;

            if (_collisionAudioSelectionMode == CollisionAudioClipSelectionMode.Random)
            {
                return getRandomCollisionAudioClip();
            }
            else
            {
                int index = (int)(Mathf.Clamp01(normalizedIntensity) * (CollisionAudioClips.Count - 1));
                return CollisionAudioClips[index];
            }
        }

        private AudioClip getRandomCollisionAudioClip()
        {
            if (CollisionAudioClips.Count == 0)
                return null;

            int index = UnityEngine.Random.Range(0, CollisionAudioClips.Count);
            return CollisionAudioClips[index];
        }

        /// <summary>
        /// Creates an instance of the ImpactAudioPool.
        /// </summary>
        public override void Preload()
        {
            ImpactAudioPool.PreloadPoolForAudioSource(AudioSourceTemplate);
            ImpactManagerInstance.CreateInteractionResultPool<AudioInteractionResult>(interactionResultPoolKey);
        }
    }
}