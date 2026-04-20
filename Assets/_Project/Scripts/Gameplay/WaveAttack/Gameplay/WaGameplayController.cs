using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using EditorAttributes;
using Game.BaseGameplay;
using Game.Global;
using Gameplay.Global;
using TnieYuPackage.DesignPatterns;
using TnieYuPackage.Utils;
using UnityEngine;

namespace Game.WaveAttack
{
    [DefaultExecutionOrder(-10)]
    public class WaGameplayController : SingletonBehavior<WaGameplayController>
    {
        private const int TOWER_HANDLER_DELAY_MILLISECONDS = 1500;

        [SerializeField] private ArmyStorageSoapDataSo globalArmyStorageSoap;
        [SerializeField] private ArmyStorageSoapDataSo waveArmyStorageSoap;
        private TowerDefenseAI towerDefenseAI;

        [SerializeField, ReadOnly] private WaveSpawn waveSpawn;

        public Dictionary<ArmyType, ObservableValue<int>> GlobalStorage =>
            globalArmyStorageSoap.data.Value.Dictionary;

        public Dictionary<ArmyType, ObservableValue<int>> WaveStorage =>
            waveArmyStorageSoap.data.Value.Dictionary;

        public ObservableValue<int> maxBaseDamageOutput = new(0);
        public ObservableValue<int> maxEntityDeploymentCount = new(0);
        [SerializeField, ReadOnly] private WaGameplayLevel currentLevel;

        [ReadOnly] public ObservableValue<int> currentBaseDamageOutput = new(0);
        [ReadOnly] public ObservableValue<int> currentEntityDeploymentCount = new(0);

        private static List<TowerRuntime> TowerPlaces => TowerRuntimeManager.Instance.towerRuntimeList;
        private static int CurrentWaveIndex => BaseGameplayController.Instance.currentWaveIndex.Value;

        #region EVENTS

        private void OnEnable()
        {
            BaseGameplayController.Instance.OnGameplayBaseDestroyed += HandleWinGame;
            BaseGameplayController.Instance.OnGameplayWaveClosed += HandleLoseGame;
            BaseGameplayController.Instance.OnWaveEnded += RefreshBehaviourAsEndWave;
            BaseGameplayController.Instance.OnBaseTakenDamage += HandlePerBaseTakenDamage;
            BaseGameplayController.Instance.OnCauseBaseDamageValid = ValidateCausingBaseDamage;
            BaseGameplayGUI.Instance.OnPlayButtonPressed += PlayGame;
        }

        private bool ValidateCausingBaseDamage() => currentBaseDamageOutput.Value <= maxBaseDamageOutput.Value;

        private void HandlePerBaseTakenDamage()
        {
            currentBaseDamageOutput.Value++;
        }

        private void HandleWinGame()
        {
            GameplayTransition.DataManager.ActiveEvent.isCompleted = true;

            //open menu win-game
            BaseGameplayGUI.Instance.OpenWinPanel();
        }

        private void HandleLoseGame()
        {
            //open menu lose-game
            BaseGameplayGUI.Instance.OpenLosePanel();
        }

        private void PlayGame()
        {
            // handle wave data
            var dataConfig = ConvertArmyStorageToWaveDataConfig(WaveStorage);

            // generate: data config -> wave & spawn.
            waveSpawn = SmartWaveConverter.Convert(dataConfig);

            BaseGameplayController.Instance.PlayWave(waveSpawn);

            // handle: after
            ResetStorage(WaveStorage);
        }

        private void OnDisable()
        {
            if (BaseGameplayController.Instance != null)
            {
                BaseGameplayController.Instance.OnCauseBaseDamageValid = null;
                BaseGameplayController.Instance.OnBaseTakenDamage -= HandlePerBaseTakenDamage;
                BaseGameplayController.Instance.OnGameplayBaseDestroyed -= HandleWinGame;
                BaseGameplayController.Instance.OnGameplayWaveClosed -= HandleLoseGame;
                BaseGameplayController.Instance.OnWaveEnded -= RefreshBehaviourAsEndWave;
            }

            if (BaseGameplayGUI.Instance != null)
            {
                BaseGameplayGUI.Instance.OnPlayButtonPressed -= PlayGame;
            }
        }

        #endregion

        #region SUPPORT METHODS

        public bool CheckEntityDeploymentValid(int validateQuantity)
        {
            return validateQuantity <= maxEntityDeploymentCount.Value;
        }

        public void AddArmyForWave(ArmyType armyType, int amount)
        {
            // if (WaveStorage[armyType].Value + amount < 0) return;
            // if (!CheckEntityDeploymentValid(currentEntityDeploymentCount.Value + amount)) return;
            // if (GlobalStorage[armyType].Value - amount < 0) return;

            WaveStorage[armyType].Value += amount;
            currentEntityDeploymentCount.Value += amount;
            GlobalStorage[armyType].Value -= amount;
        }

        private void ResetStorage(Dictionary<ArmyType, ObservableValue<int>> storage)
        {
            foreach (var armyKey in storage.Keys)
            {
                storage[armyKey].Value = 0;
            }
        }

        public static void Setup(WaGameplayLevel waLevel,
            Dictionary<ArmyType, int> armyStorageSource,
            int pathCount,
            int towerCount)
        {
            Instance.currentLevel = waLevel;

            Instance.ResetStorage(Instance.WaveStorage);

            Instance.SetupArmyGlobalStorage(armyStorageSource);
            Instance.SetupTowerDefenseAI(pathCount, towerCount);
        }

        private void SetupTowerDefenseAI(int mapPathCount, int towerCount)
        {
            towerDefenseAI = new(
                mapPathCount: mapPathCount,
                towerCount: towerCount
            );

            RefreshBehaviourAsEndWave();
        }

        private void SetupArmyGlobalStorage(Dictionary<ArmyType, int> armyStorageSource)
        {
            foreach (var kvp in armyStorageSource)
            {
                if (GlobalStorage.ContainsKey(kvp.Key))
                {
                    GlobalStorage[kvp.Key].Value = kvp.Value;
                }
            }
        }

        #endregion

        private async void RefreshBehaviourAsEndWave()
        {
            int currentWaveIndex = CurrentWaveIndex;
            if (currentWaveIndex < currentLevel.waveRules.Count)
            {
                var currentWaveRule = currentLevel.waveRules[CurrentWaveIndex];
                maxBaseDamageOutput.Value = currentWaveRule.maxBaseDamageOutput;
                maxEntityDeploymentCount.Value = currentWaveRule.maxEntityDeploymentCount;
            }

            currentBaseDamageOutput.Value = 0;
            currentEntityDeploymentCount.Value = 0;

            if (towerDefenseAI == null) return;

            (TowerRuntime, TowerPresetSo) result =
                towerDefenseAI.GetTower(TowerPlaces);
            while (result.Item1 != null && result.Item2 != null)
            {
                result.Item1.Setup(result.Item2);
                result = towerDefenseAI.GetTower(TowerPlaces);

                await UniTask.Delay(TOWER_HANDLER_DELAY_MILLISECONDS);
            }

            towerDefenseAI.GrowthSlope();
        }

        private static WaveDataConfig ConvertArmyStorageToWaveDataConfig(
            Dictionary<ArmyType, ObservableValue<int>> armyStorage)
        {
            var data = armyStorage.Select(kvp => new KeyValuePair<ArmyType, int>(kvp.Key, kvp.Value.Value))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            return new WaveDataConfig(data);
        }
    }
}