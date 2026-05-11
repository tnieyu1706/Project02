using UnityEngine;

namespace SoundSystem.Core
{
    public class SfxPlayer : MonoBehaviour
    {
        [SerializeField] protected SfxManager sfxManager;

        public void PlaySound(SoundData soundData)
        {
            sfxManager.PlayVfx(soundData);
        }
    }
}