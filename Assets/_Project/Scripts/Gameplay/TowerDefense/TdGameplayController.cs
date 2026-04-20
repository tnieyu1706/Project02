using System.Collections.Generic;
using System.Linq;
using Game.BaseGameplay;
using Gameplay.Global;
using TnieYuPackage.DesignPatterns;

namespace Game.TowerDefense
{
    public class TdGameplayController : SingletonBehavior<TdGameplayController>
    {
        public List<WaveDataConfig> Waves = new();

        protected override void Awake()
        {
            dontDestroyOnLoad = false;
            base.Awake();
        }

        #region EVENTS

        private void OnEnable()
        {
            BaseGameplayGUI.Instance.OnPlayButtonPressed += PlayWave;
            BaseGameplayController.Instance.OnGameplayBaseDestroyed += OnGameplayBaseDestroyed;
            BaseGameplayController.Instance.OnGameplayWaveClosed += OnGameplayWaveClosed;
            BaseGameplayController.Instance.OnCauseBaseDamageValid = ValidateCausingBaseDamage;
        }

        private bool ValidateCausingBaseDamage() => true;

        private void PlayWave()
        {
            // validate
            var currentIndex = BaseGameplayController.Instance.currentWaveIndex.Value;

            var waveDataConfig = Waves[currentIndex];

            //translate: dataConfig -> spawn
            var waveSpawn = SmartWaveConverter.Convert(waveDataConfig);

            BaseGameplayController.Instance.PlayWave(waveSpawn);
        }

        private void OnGameplayBaseDestroyed()
        {
            //handle: lose game
            BaseGameplayGUI.Instance.OpenLosePanel();
        }

        private void OnGameplayWaveClosed()
        {
            //handle: win game
            GameplayTransition.DataManager.ActiveEvent.isCompleted = true;

            BaseGameplayGUI.Instance.OpenWinPanel();
        }

        private void OnDisable()
        {
            if (BaseGameplayController.Instance != null)
            {
                BaseGameplayController.Instance.OnGameplayBaseDestroyed -= OnGameplayBaseDestroyed;
                BaseGameplayController.Instance.OnGameplayWaveClosed -= OnGameplayWaveClosed;
                BaseGameplayController.Instance.OnCauseBaseDamageValid = null;
            }

            if (BaseGameplayGUI.Instance != null)
            {
                BaseGameplayGUI.Instance.OnPlayButtonPressed -= PlayWave;
            }
        }

        #endregion

        public void Setup(TdGameplayLevel tdGameplayLevel)
        {
            Waves = tdGameplayLevel.waveConfigs.Select(dto => new WaveDataConfig(dto.MilitaryClone))
                .ToList();
        }
    }
}