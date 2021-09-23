using Impact.Objects;
using Impact.Utility.ObjectPool;
using UnityEngine;

namespace Impact.Interactions.Audio
{
    /// <summary>
    /// The result of an audio interaction.
    /// Handles both single collision sounds and sliding and rolling sounds.
    /// </summary>
    public class AudioInteractionResult : IContinuousInteractionResult, IPoolable
    {
        /// <summary>
        /// The original interaction data this result was created from.
        /// </summary>
        public InteractionData OriginalData { get; set; }

        /// <summary>
        /// The result key for updating sliding and rolling.
        /// </summary>
        public long Key { get; set; }

        /// <summary>
        /// The audio source prefab obtained from the audio interaction.
        /// </summary>
        public ImpactAudioSourceBase AudioSourceTemplate;

        /// <summary>
        /// The audio clip being played. Obtained from the audio interaction.
        /// </summary>
        public AudioClip AudioClip;

        /// <summary>
        /// Should the audio be looped? 
        /// Sliding and rolling will set this to true. 
        /// Regular collisions will set this to false.
        /// </summary>
        public bool LoopAudio;

        /// <summary>
        /// The original volume of the interaction audio.
        /// </summary>
        public float Volume;

        /// <summary>
        /// The original pitch of the interaction audio.
        /// </summary>
        public float Pitch;

        /// <summary>
        /// The interaction that created this result.
        /// Used to update the result for sliding and rolling.
        /// </summary>
        public ImpactAudioInteraction Interaction;

        /// <summary>
        /// Is the initial volume non-zero and is there an audio clip and audio source defined?
        /// </summary>
        public bool IsValid
        {
            get { return Volume > 0.01f && AudioClip != null && AudioSourceTemplate != null; }
        }

        /// <summary>
        /// Is the audio still playing?
        /// </summary>
        public bool IsAlive
        {
            get { return (targetVolume > 0 || currentVolume > 0) && audioSource != null; }
        }

        private float targetVolume, currentVolume;
        private float targetPitch, currentPitch;

        private IImpactObject parent;
        private ImpactAudioSourceBase audioSource;

        private bool isAvailable = true;

        /// <summary>
        /// Play audio using our data.
        /// </summary>
        /// <param name="parent">The Impact Object that created this result.</param>
        public void Process(IImpactObject parent)
        {
            this.parent = parent;
            audioSource = ImpactAudioPool.PlayAudio(this, OriginalData.Point, InteractionResultExtensions.GetPriority(OriginalData.PriorityOverride, parent));

            targetVolume = currentVolume = Volume;
            targetPitch = currentPitch = Pitch;

            //Dispose immediately for Collision interaction types
            if (OriginalData.InteractionType == InteractionData.InteractionTypeCollision)
                Dispose();
        }

        /// <summary>
        /// Smoothly update the volume and pitch for sliding and rolling.
        /// </summary>
        public void FixedUpdate()
        {
            if (audioSource == null)
                return;

            currentVolume = Mathf.MoveTowards(currentVolume, targetVolume, 0.1f);
            currentPitch = Mathf.MoveTowards(currentPitch, targetPitch, 0.1f);

            audioSource.UpdateAudio(currentVolume, currentPitch);

            targetVolume = 0;
        }

        /// <summary>
        /// Updates this result to adjust the volume and pitch and updates the audio source position.
        /// </summary>
        /// <param name="newResult">The new result that will be used to get the new volume and pitch.</param>
        public void KeepAlive(IInteractionResult newResult)
        {
            if (audioSource == null)
                return;

            AudioInteractionResult audioInteractionResult = newResult as AudioInteractionResult;

            targetVolume = audioInteractionResult.Volume;
            targetPitch = Interaction.UpdatePitch(Pitch, audioInteractionResult.OriginalData.Velocity);

            audioSource.transform.position = audioInteractionResult.OriginalData.Point;
        }

        /// <summary>
        /// Stops the audio source associated with this result so it becomes available in the audio source object pool.
        /// </summary>
        public void Dispose()
        {
            if (OriginalData.InteractionType != InteractionData.InteractionTypeCollision && audioSource != null)
                audioSource.StopAudio();

            AudioSourceTemplate = null;
            audioSource = null;
            AudioClip = null;
            Interaction = null;
            parent = null;

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