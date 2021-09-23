using Impact.Utility.ObjectPool;
using UnityEngine;

namespace Impact.Interactions.Audio
{
    /// <summary>
    /// Component for playing audio interactions.
    /// </summary>
    [AddComponentMenu("Impact/Impact Audio Source")]
    public class ImpactAudioSource : ImpactAudioSourceBase
    {
        [SerializeField]
        private AudioSource _audioSource;

        [SerializeField]
        private int _poolSize = 20;
        [SerializeField]
        private ObjectPoolFallbackMode _poolFallbackMode = ObjectPoolFallbackMode.LowerPriority;

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

        private float baseVolume, basePitch;

        private void Reset()
        {
            _audioSource = GetComponentInChildren<AudioSource>();
        }

        private void Awake()
        {
            baseVolume = _audioSource.volume;
            basePitch = _audioSource.pitch;
        }

        public override void PlayAudio(AudioInteractionResult interactionResult, Vector3 point)
        {
            transform.position = point;

            _audioSource.loop = interactionResult.LoopAudio;
            _audioSource.clip = interactionResult.AudioClip;
            _audioSource.volume = baseVolume * interactionResult.Volume;
            _audioSource.pitch = basePitch * interactionResult.Pitch;

            _audioSource.Play();
        }

        public override void UpdateAudio(float volume, float pitch)
        {
            _audioSource.volume = volume;
            _audioSource.pitch = pitch;
        }

        public override void StopAudio()
        {
            _audioSource.Stop();
        }

        private void Update()
        {
            if (!IsAvailable() && !_audioSource.isPlaying)
            {
                MakeAvailable();
            }
        }
    }
}
