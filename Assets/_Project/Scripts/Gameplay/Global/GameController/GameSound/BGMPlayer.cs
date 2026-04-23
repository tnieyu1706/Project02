using EditorAttributes;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.Audio;

namespace _Project.Scripts.Gameplay.Global.GameController
{
    [RequireComponent(typeof(AudioSource))]
    public class BGMPlayer : MonoBehaviour
    {
        [SerializeField] AudioResource resource;
        [SerializeField] bool isLooping;
        [SerializeField] [Range(0f, 1f)] private float volume = 1f;

        [SerializeField] private bool isPlayOnStart;

        [SerializeField, Self] private AudioSource audioSource;

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

        [Button]
        public void Play()
        {
            if (resource == null || audioSource == null) return;

            audioSource.resource = resource;
            audioSource.loop = isLooping;
            audioSource.volume = GameSoundMaster.GetTotalVolume(volume);

            audioSource.Play();
        }
    }
}