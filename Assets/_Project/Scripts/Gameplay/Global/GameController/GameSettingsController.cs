using TnieYuPackage.DesignPatterns;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Global.GameController
{
    public class GameSettingsController : SingletonBehavior<GameSettingsController>
    {
        private const string MASTER_VOLUME_KEY = "masterVolume";
        private const string SFX_VOLUME_KEY = "sfxVolume";
        private const string HAS_GAME_CREATED_KEY = "hasGameCreated";

        #region Global Properties

        public float masterVolume = 1;
        public float sfxVolume = 1;
        public bool hasGameCreated = false;

        #endregion

        protected override void Awake()
        {
            base.Awake();

            LoadProperties();
        }

        private void LoadProperties()
        {
            if (PlayerPrefs.HasKey(MASTER_VOLUME_KEY))
            {
                masterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY);
            }

            if (PlayerPrefs.HasKey(SFX_VOLUME_KEY))
            {
                sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY);
            }

            if (PlayerPrefs.HasKey(HAS_GAME_CREATED_KEY))
            {
                hasGameCreated = PlayerPrefs.GetInt(HAS_GAME_CREATED_KEY) == 1;
            }
        }

        private void SaveProperties()
        {
            PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, masterVolume);
            PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sfxVolume);
            PlayerPrefs.SetInt(HAS_GAME_CREATED_KEY, hasGameCreated ? 1 : 0);
        }

        private void OnApplicationQuit()
        {
            SaveProperties();
        }
    }
}