using Reflex.Attributes;
using SoundSystem.Core;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Global.SoundSystem
{
    public class SfxPlayer : MonoBehaviour
    {
        [Inject] protected SfxManager sfxManager;

        public void PlaySound(SoundData soundData)
        {
            sfxManager.PlayVfx(soundData).Forget();
        }
    }
}