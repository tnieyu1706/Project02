using KBCore.Refs;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Gameplay.Global.GameController
{
    [RequireComponent(typeof(Button))]
    public class ButtonSfxPlayer : SfxPlayer
    {
        [SerializeField, Self] private Button button;
        [SerializeField] private string sfxName;

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
            PlaySound(sfxName);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(PlaySfx);
        }
    }
}