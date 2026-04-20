using System;
using EditorAttributes;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Global.GameController
{
    public class SfxPlayer : MonoBehaviour
    {
        [Button]
        public void PlaySound(string soundName)
        {
            GameSoundMaster.TryPlayOnce(soundName).Forget();
        }
    }
}