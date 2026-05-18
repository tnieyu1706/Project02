using Reflex.Extensions;
using SoundSystem.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

// Thay đổi theo namespace thực tế của Reflex nếu cần

namespace Game.BaseGameplay.Configurators
{
    [CreateAssetMenu(fileName = "InteractSfxConfigurator",
        menuName = "Game/BaseGameplay/Configurators/InteractSfxConfigurator")]
    public class InteractSfxConfigurator : BaseObjectConfigurator
    {
        [Header("Sound Settings")] [SerializeField]
        private SoundData interactSoundData;

        public override void Configure(IBaseObjectRuntime runtime)
        {
            // Đăng ký lắng nghe sự kiện tương tác
            runtime.OnInteract += OnInteract;
        }

        public override void UnConfigure(IBaseObjectRuntime runtime)
        {
            // Hủy đăng ký khi gỡ Configurator để tránh memory leak
            runtime.OnInteract -= OnInteract;
        }

        private void OnInteract(IBaseObjectRuntime owner, IObjectInteractable target)
        {
            var container = SceneManager.GetActiveScene().GetSceneContainer();
            var sfxManager = container.Resolve<SfxManager>();
            sfxManager?.PlayVfx(interactSoundData).Forget();
        }
    }
}