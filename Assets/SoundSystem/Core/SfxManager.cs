using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

namespace SoundSystem.Core
{
    [DefaultExecutionOrder(-20)]
    public class SfxManager : MonoBehaviour
    {
        private const int WAIT_TIME_MILLISECONDS = 100;

        [SerializeField] private AudioSource audioSourcePrefab;
        [SerializeField] private Vector2 volumeVariant = Vector2.zero;
        [SerializeField] private Vector2 pitchVariant = Vector2.zero;

        protected ObjectPool<AudioSource> Pool { get; set; }

        public virtual float GetTotalVfxVolume(float volume)
        {
            return volume;
        }

        protected void Awake()
        {
            Pool = new(
                OnCreateAudioPrefab,
                OnGetAudioPrefab,
                OnReleaseAudioPrefab,
                OnDestroyAudioPrefab,
                true,
                5,
                20
            );
        }

        private AudioSource OnCreateAudioPrefab()
        {
            return Instantiate(audioSourcePrefab);
        }

        private void OnGetAudioPrefab(AudioSource audioSource)
        {
            audioSource.gameObject.SetActive(true);
        }

        private void OnReleaseAudioPrefab(AudioSource audioSource)
        {
            audioSource.Stop();
            audioSource.gameObject.SetActive(false);
        }

        private void OnDestroyAudioPrefab(AudioSource audioSource)
        {
            DestroyImmediate(audioSource.gameObject);
        }

        public async void PlayVfx(SoundData soundData)
        {
            var audioSource = Pool.Get();
            // setup
            audioSource.resource = soundData.resource;
            audioSource.volume = GetTotalVfxVolume(soundData.volume) + Random.Range(volumeVariant.x, volumeVariant.y);
            audioSource.pitch = soundData.pitch + Random.Range(pitchVariant.x, pitchVariant.y);
            // audioSource.time = Random.Range(0, soundData.audioClip.length);
            audioSource.loop = false;

            audioSource.Play();

            while (audioSource.isPlaying && !destroyCancellationToken.IsCancellationRequested)
            {
                await Task.Delay(WAIT_TIME_MILLISECONDS, cancellationToken: destroyCancellationToken);
            }

            audioSource.Stop(); // ensure stop before release
            Pool.Release(audioSource);
        }
    }
}