using UnityEngine;
using UnityEngine.Audio;

namespace _Project.Scripts.Gameplay.Global.GameController
{
    [CreateAssetMenu(fileName = "SoundData", menuName = "Game/Global/SoundData")]
    public class SoundData : ScriptableObject
    {
        public AudioResource resource;
        [Range(0f, 1f)] public float volume = 1;
        [Range(0f, 3f)] public float pitch = 1;
        public bool loop;
    }
}