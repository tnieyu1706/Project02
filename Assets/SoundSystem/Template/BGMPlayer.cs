using UnityEngine;
using UnityEngine.Audio;

namespace SoundSystem.Template
{
    [RequireComponent(typeof(AudioSource))]
    public class BGMPlayer : MonoBehaviour
    {
        [SerializeField] protected AudioResource resource;
        [SerializeField] protected bool isLooping;
        [SerializeField] [Range(0f, 1f)] protected float volume = 1f;

        [SerializeField] private bool isPlayOnStart;

        [SerializeField] protected AudioSource audioSource;

        void Awake()
        {
            audioSource ??= GetComponent<AudioSource>();
        }

        void Start()
        {
            if (isPlayOnStart)
            {
                Play();
            }
        }

        private void OnDisable()
        {
            audioSource?.Stop();
        }

        public virtual void Play()
        {
            if (resource == null || audioSource == null) return;

            audioSource.resource = resource;
            audioSource.loop = isLooping;
            audioSource.volume = volume;

            audioSource.Play();
        }
    }
}