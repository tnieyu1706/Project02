using EditorAttributes;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Global.GameController
{
    public class BGMPlayer : MonoBehaviour
    {
        public string bgmName;
        [SerializeField] private bool isPlayOnStart;

        private AudioSource audioSource;

        private void OnEnable()
        {
            audioSource = GameSoundMaster.Instance.Pool.Get();
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
            if (GameSoundMaster.Instance != null)
            {
                GameSoundMaster.Instance.Pool.Release(audioSource);
            }
        }
        
        [Button]
        public void Play()
        {
            if (!GameSoundMaster.Instance.Sounds.TryGetValue(bgmName, out var soundData)) return;

            audioSource.clip = soundData.audioClip;
            audioSource.loop = soundData.loop;
            audioSource.volume = GameSoundMaster.GetTotalVolume(soundData.volume);

            audioSource.Play();
        }
    }
}