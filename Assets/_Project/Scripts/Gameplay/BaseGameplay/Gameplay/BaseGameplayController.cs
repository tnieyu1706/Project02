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

        public event Action<int> OnBaseTakenDamage;

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
                UniTask.Delay(TimeSpan.FromSeconds(60), cancellationToken: token)
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

        public void CauseBaseDamage(int damage)
        {
            if (OnCauseBaseDamageValid != null && !OnCauseBaseDamageValid.Invoke()) return;

            baseHealth.Value -= damage;
            OnBaseTakenDamage?.Invoke(damage);
            if (baseHealth.Value <= 0)
            {
                OnGameplayBaseDestroyed?.Invoke();
            }
        }

        #region EVENTS

        private void OnEnable()
        {
            currentWaveIndex.OnValueChanged += OnCurrentWaveIndexChanged;
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