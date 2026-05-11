using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

namespace SoundSystem.Core
{
    [DefaultExecutionOrder(-100)]
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
            audioSource?.gameObject.SetActive(true);
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

        public async UniTaskVoid PlayVfx(SoundData soundData)
        {
            var audioSource = Pool.Get();

            audioSource.resource = soundData.resource;
            audioSource.volume = GetTotalVfxVolume(soundData.volume);
            audioSource.pitch = soundData.pitch;
            audioSource.loop = false;

            audioSource.Play();

            try
            {
                await UniTask.WaitUntil(
                    () => !audioSource.isPlaying,
                    cancellationToken: this.GetCancellationTokenOnDestroy()
                );
            }
            catch
            {
                // scene unload => cancel
            }
            finally
            {
                if (audioSource != null && Pool != null)
                {
                    Pool.Release(audioSource);
                }
            }
        }
    }
}