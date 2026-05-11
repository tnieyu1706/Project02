using _Project.Scripts.Gameplay.Global.GameController;
using SoundSystem.Core;

namespace _Project.Scripts.Gameplay.Global.SoundSystem
{
    public class GameSfxManager : SfxManager
    {
        public override float GetTotalVfxVolume(float volume)
        {
            return base.GetTotalVfxVolume(volume)
                   * GameSettingsController.Instance.masterVolume
                   * GameSettingsController.Instance.sfxVolume;
        }
    }
}