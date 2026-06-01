using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Reflex.Attributes;
using SoundSystem.Core;
using TnieYuPackage.DesignPatterns;
using TnieYuPackage.Utils;
using UnityEngine;

namespace Game.BaseGameplay
{
    [DefaultExecutionOrder(-49)]
    public class BaseGameplayController : Singleton<BaseGameplayController>
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

        private int _previousHealth = -1; // Thêm biến để track máu trước đó

        #endregion

        [SerializeField] private WaveSpawn spawnRead;

        [Inject, NonSerialized] public SfxManager SfxManagerInject;

        public void PlayWave(WaveSpawn waveSpawn)
        {
            spawnRead = waveSpawn;
            RunWave(waveSpawn, this.GetCancellationTokenOnDestroy()).Forget();
        }

        private async UniTask RunWave(WaveSpawn waveSpawn, CancellationToken token)
        {
            Debug.Log($"[TdWaveController] Begin wave {currentWaveIndex.Value}...");
            OnWaveStarted?.Invoke();

            await waveSpawn.Spawn(token);

            await UniTask.WhenAny(
                BaseGameplayPrefabSpawnManager.Instance.PoolTrackers[PrefabType.BaseEnemy]
                    .Waiting(),
                UniTask.Delay(TimeSpan.FromSeconds(30), cancellationToken: token)
            );

            Debug.Log($"[TdWaveController] End wave {currentWaveIndex.Value}...");
            currentWaveIndex.Value++;

            OnWaveEnded?.Invoke();
        }

        #region EVENTS

        private void OnEnable()
        {
            baseHealth.OnValueChanged += OnBaseHealthChanged;
            currentWaveIndex.OnValueChanged += OnCurrentWaveIndexChanged;
        }

        private void OnBaseHealthChanged(int changedValue)
        {
            // Kiểm tra xem có thực sự là bị mất máu (nhận sát thương) không
            bool isTakingDamage = _previousHealth != -1 && changedValue < _previousHealth;
            _previousHealth = changedValue; // Cập nhật lại cache

            if (isTakingDamage)
            {
                if (OnCauseBaseDamageValid != null && !OnCauseBaseDamageValid()) return;

                OnBaseTakenDamage?.Invoke();
            }

            // Vẫn giữ nguyên logic kiểm tra thua game
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

        public void DestroyBase()
        {
            OnGameplayBaseDestroyed?.Invoke();
        }

        public void CloseWaves()
        {
            OnGameplayWaveClosed?.Invoke();
        }

        public void Setup(BaseGameplayLevel baseLevel, int maxWaveIndexSource)
        {
            _previousHealth = -1; // Reset flag khi khởi tạo Level mới
            baseHealth.Value = baseLevel.baseMaxHealth;
            money.Value = baseLevel.startMoney;
            maxWaveIndex.Value = maxWaveIndexSource;
            currentWaveIndex.Value = 0;
        }
    }
}