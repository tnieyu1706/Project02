using _Project.Scripts.Gameplay.Global.GameController;
using SoundSystem.Template;

namespace _Project.Scripts.Gameplay.Global.SoundSystem
{
    public class GameBgmPlayer : BGMPlayer
    {
        public override void Play()
        {
            if (resource == null || audioSource == null) return;

            audioSource.resource = resource;
            audioSource.loop = isLooping;
            audioSource.volume = volume * GameSettingsController.Instance.masterVolume;

            audioSource.Play();
        }
    }
}