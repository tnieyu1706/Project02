using SoundSystem.Core;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Gameplay.Global.SoundSystem
{
    [RequireComponent(typeof(Button))]
    public class ButtonSfxPlayer : SfxPlayer
    {
        [SerializeField] private SoundData soundData;
        private Button button;

        void Awake()
        {
            button = GetComponent<Button>();
        }

        void Start()
        {
            button.onClick.AddListener(PlaySfx);
        }

        private void PlaySfx()
        {
            PlaySound(soundData);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(PlaySfx);
        }
    }
}