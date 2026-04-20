using UnityEngine;

namespace _Project.Scripts.Gameplay.Global.GameController
{
    [CreateAssetMenu(fileName = "SoundData", menuName = "Game/Global/SoundData")]
    public class SoundData : ScriptableObject
    {
        public AudioClip audioClip;
        public float volume;
        public bool loop;
    }
}