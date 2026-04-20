using System;
using System.Threading;
using BackboneLogger;
using Cysharp.Threading.Tasks;
using TnieYuPackage.DesignPatterns;
using TnieYuPackage.Utils;
using UnityEngine;

namespace Game.BaseGameplay
{
    [DefaultExecutionOrder(-49)]
    public class BaseGameplayController : SingletonBehavior<BaseGameplayController>
    {
        #region GAMEPLAY PROPERTIES

        public ObservableValue<int> baseHealth;
        public ObservableValue<int> money;
        public ObservableValue<int> currentWaveIndex;
        public ObservableValue<int> maxWaveIndex;

        /// <summary>
        /// Need registry least 1 delegate for game execute normal.
        /// </summary>
        public Func<bool> OnCauseBaseDamageValid;

        public event Action OnGameplayBaseDestroyed;
        public event Action OnGameplayWaveClosed;

        public event Action OnWaveStarted;
        public event Action OnWaveEnded;

        public event Action OnBaseTakenDamage;

        #endregion

        [SerializeField] private WaveSpawn spawnRead;

        public void PlayWave(WaveSpawn waveSpawn)
        {
            spawnRead = waveSpawn;
            RunWave(waveSpawn, this.GetCancellationTokenOnDestroy()).Forget();
        }

        private async UniTask RunWave(WaveSpawn waveSpawn, CancellationToken token)
        {
            BLogger.Log($"[TdWaveController] Start wave...", category: "Base");
            OnWaveStarted?.Invoke();

            await waveSpawn.Spawn(token);

            await UniTask.WhenAny(
                BaseGameplayPrefabSpawnManager.Instance.PoolTrackers[PrefabType.BaseEnemy]
                    .Waiting(),
                UniTask.Delay(TimeSpan.FromSeconds(30), cancellationToken: token)
            );

            BLogger.Log($"[TdWaveController] End wave...", category: "Base");
            currentWaveIndex.Value++;

            OnWaveEnded?.Invoke();
        }

        protected override void Awake()
        {
            dontDestroyOnLoad = false;

            base.Awake();
        }

        #region EVENTS

        private void OnEnable()
        {
            baseHealth.OnValueChanged += OnBaseHealthChanged;
            currentWaveIndex.OnValueChanged += OnCurrentWaveIndexChanged;
        }

        private void OnBaseHealthChanged(int changedValue)
        {
            if (OnCauseBaseDamageValid != null && !OnCauseBaseDamageValid()) return;
            
            OnBaseTakenDamage?.Invoke();
            if (changedValue <= 0)
            {
                OnGameplayBaseDestroyed?.Invoke();
            }
        }

        private void OnCurrentWaveIndexChanged(int changedValue)
        {
            if (changedValue >= maxWaveIndex.Value)
            {
                OnGameplayWaveClosed?.Invoke();
            }
        }

        private void OnDisable()
        {
            baseHealth.OnValueChanged -= OnBaseHealthChanged;
            currentWaveIndex.OnValueChanged -= OnCurrentWaveIndexChanged;
        }

        #endregion

        public void Setup(BaseGameplayLevel baseLevel, int maxWaveIndexSource)
        {
            baseHealth.Value = baseLevel.baseMaxHealth;
            money.Value = baseLevel.startMoney;
            maxWaveIndex.Value = maxWaveIndexSource;
            currentWaveIndex.Value = 0;
        }
    }
}