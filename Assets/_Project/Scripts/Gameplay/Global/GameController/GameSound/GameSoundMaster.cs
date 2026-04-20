using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EditorAttributes;
using TnieYuPackage.DesignPatterns;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;

namespace _Project.Scripts.Gameplay.Global.GameController
{
    /// <summary>
    /// Master for playing sound for game.
    /// Using object pool to manage audio source for better performance.
    /// Get/Release Pool when you need control AudioSource, likes BGM.
    /// Call PlayOnce when you just want to play sound with controller by GameSoundMaster, likes SFX, short.
    /// </summary>
    public class GameSoundMaster : SingletonBehavior<GameSoundMaster>
    {
        // current load all sound
        private const string SOUND_LABEL_NAME = "Sound";
        private const int WAIT_TIME_MILLISECONDS = 100;

        [SerializeField, Required] private Transform container;
        [SerializeField] private GameObject sfxPrefab;
        public ObjectPool<AudioSource> Pool { get; set; }

        public readonly Dictionary<string, SoundData> Sounds = new();

        public static float GetTotalVolume(float clipVolume) =>
            clipVolume * GameSettingsController.Instance.sfxVolume * GameSettingsController.Instance.masterVolume;

        protected override void Awake()
        {
            base.Awake();

            Pool = new(
                OnCreateAudioPrefab,
                OnGetAudioPrefab,
                OnReleaseAudioPrefab,
                OnDestroyAudioPrefab,
                true,
                5,
                20
            );

            LoadSfxSounds();
        }

        private AudioSource OnCreateAudioPrefab()
        {
            var gObj = Instantiate(sfxPrefab, container);

            return gObj.GetComponent<AudioSource>();
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

        private async void LoadSfxSounds()
        {
            var resourcesTask = Addressables.LoadResourceLocationsAsync(SOUND_LABEL_NAME).ToUniTask();
            var resources = await resourcesTask;

            Sounds.Clear();
            foreach (var re in resources)
            {
                var data = await Addressables.LoadAssetAsync<SoundData>(re).ToUniTask();
                Sounds.Add(re.PrimaryKey, data);
            }
        }
        public static async UniTask PlayOnce(SoundData soundData)
        {
            var audioSource = Instance.Pool.Get();

            // setup
            audioSource.clip = soundData.audioClip;
            audioSource.volume = GetTotalVolume(soundData.volume) * Random.Range(0.9f, 1.1f);
            audioSource.pitch = Random.Range(0.8f, 1.2f);
            // audioSource.time = Random.Range(0, soundData.audioClip.length);
            audioSource.loop = false; //default vfx play one-time

            audioSource.Play();

            while (audioSource.isPlaying && !Instance.GetCancellationTokenOnDestroy().IsCancellationRequested)
            {
                await UniTask.Delay(WAIT_TIME_MILLISECONDS);
            }

            audioSource.Stop(); // ensure stop before release
            Instance.Pool.Release(audioSource);
        }

        public static async UniTaskVoid TryPlayOnce(string soundName)
        {
            if (Instance.Sounds.TryGetValue(soundName, out var soundData))
            {
                await PlayOnce(soundData);
            }
        }

        //testing

        private AudioSource audioSourceTemp;

        [Button]
        private void PlaySoundManual(string soundName)
        {
            TryPlayOnce(soundName).Forget();
        }
    }
}