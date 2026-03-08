using System;
using BackboneLogger;
using Cysharp.Threading.Tasks;
using EditorAttributes;
using UnityEngine;
using UnityEngine.Splines;
using Random = UnityEngine.Random;

namespace Game.Td
{
    [Serializable]
    public class TdWaveController
    {
        [SerializeField] private Vector2 offsetRandom = new Vector2(-1f, 1f);
        [SerializeField, ReadOnly] private int currentWaveIndex;
        private LevelWave currentLevelWave;

        public Action<int, int> OnCurrentWaveIndexChanged;
        public Action OnWaveCompleted;

        private int CurrentWaveIndex
        {
            get => currentWaveIndex;
            set
            {
                OnCurrentWaveIndexChanged?.Invoke(value, currentLevelWave.waves.Count);
                currentWaveIndex = value;
            }
        }

        public void SetupLevelWave(LevelWave levelWave)
        {
            currentLevelWave = levelWave;
            CurrentWaveIndex = 0;
        }

        public void PlayWave()
        {
            var currentWavePlaying = currentLevelWave.waves[CurrentWaveIndex];
            BLogger.Log($"[TdWaveController] Start wave {currentWavePlaying.id}", category: "TD");

            PlayWaveRoutine(currentWavePlaying).Forget();
        }

        private async UniTaskVoid PlayWaveRoutine(Wave wave)
        {
            if (wave.keyframes.Count <= 0) return;
            int currentKeyframeIndex = 0;
            float currentTime = 0;

            while (true)
            {
                currentTime += Time.deltaTime;
                if (currentTime >= wave.endTime || currentKeyframeIndex >= wave.keyframes.Count)
                {
                    BLogger.Log(
                        $"[TdWaveController] End wave {currentLevelWave.waves[CurrentWaveIndex].id}: {currentTime:F1}",
                        category: "TD");
                    CurrentWaveIndex++;
                    
                    OnWaveCompleted?.Invoke();
                    return;
                }

                var keyframe = wave.keyframes[currentKeyframeIndex];

                if (currentTime >= keyframe.time)
                {
                    BLogger.Log($"[TdWaveController] Spawn keyframe {keyframe.time:F1}: {currentTime:F1}");
                    SpawnKeyframe(keyframe);

                    currentKeyframeIndex++;
                }

                await UniTask.Yield();
            }
        }

        private void SpawnKeyframe(WaveKeyFrame waveKeyFrame)
        {
            foreach (var p in waveKeyFrame.pathWaves)
            {
                p.spawnCommands.ForEach(command => SpawnCommand(command, p).Forget());
            }
        }

        private async UniTaskVoid SpawnCommand(WaveSpawnCommand command, WavePath wavePath)
        {
            if (!EnemyPresetManager.Instance.data.Dictionary
                    .TryGetValue(command.entityId, out EnemyPresetSo entityPreset))
            {
                BLogger.Log($"[TdWaveController] EnemyPreset {command.entityId} not found!", category: "TD", level: LogLevel.Critical);;
                return;
            }

            if (!TdGameplayController.Instance.Paths
                    .TryGetValue(wavePath.pathId, out SplineContainer pathMover))
            {
                BLogger.Log($"[TdWaveController] Path {wavePath.pathId} not found!", category: "TD", level: LogLevel.Critical);;
                return;
            }

            for (int i = 0; i < command.quantity; i++)
            {
                GameObject enemy = TdSpawnManager.Instance.RuntimePools[TdSpawnKey.Enemy].Get();
                
                if (enemy.TryGetComponent(out EnemyRuntime entityRuntime))
                {
                    float offset = Random.Range(offsetRandom.x, offsetRandom.y);
                    entityRuntime.Setup(entityPreset, pathMover, new Vector2(offset, offset));
                }

                await UniTask.Delay((int)(command.spawnInterval * 1000));
            }
        }
    }
}