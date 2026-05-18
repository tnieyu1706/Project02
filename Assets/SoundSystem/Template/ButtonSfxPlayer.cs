using SoundSystem.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SoundSystem.Template
{
    [RequireComponent(typeof(Button))]
    public class ButtonSfxPlayer : SfxPlayer
    {
        [SerializeField] private Button button;
        [SerializeField] private SoundData soundData;

        void Awake()
        {
            button ??= GetComponent<Button>();
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