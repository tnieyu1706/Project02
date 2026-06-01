using UnityEngine;
using UnityEngine.Audio;

namespace SoundSystem.Core
{
    [CreateAssetMenu(fileName = "SoundData", menuName = "Game/Global/SoundData")]
    public class SoundData : ScriptableObject
    {
        public AudioResource resource;
        [Range(0f, 1.6f)] public float volume = 1;
        [Range(0f, 3f)] public float pitch = 1;
        public bool loop;
    }
}