using System;
using _Project.Scripts.TowerDefense.Entity;
using _Project.Scripts.TowerDefense.LevelSystem;
using BackboneLogger;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Splines;

namespace _Project.Scripts.TowerDefense.Gameplay
{
    public class WaveController
    {
        private LevelWave currentLevelWave;
        private int currentWaveIndex;

        public Action<int, int> OnCurrentWaveIndexChanged;

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
            BLogger.Log($"Playing wave {currentWavePlaying.id}", category: "TD");

            PlayWaveRoutine(currentWavePlaying).Forget();

            CurrentWaveIndex++;
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
                    return;
                }

                var keyframe = wave.keyframes[currentKeyframeIndex];

                if (currentTime >= keyframe.time)
                {
                    SpawnKeyframe(keyframe);

                    currentKeyframeIndex++;
                }

                await UniTask.Yield();
            }
        }

        private void SpawnKeyframe(WaveKeyFrame waveKeyFrame)
        {
            foreach (var p in waveKeyFrame.paths)
            {
                Debug.Log($"Spawn path - {p.pathId}");

                p.spawns.ForEach(command => SpawnCommand(command, p).Forget());
            }
        }

        private async UniTaskVoid SpawnCommand(WaveSpawnCommand command, WavePath wavePath)
        {
            if (!EntityDataManager.Instance.data.Dictionary
                    .TryGetValue(command.entityId, out EntityPresetSo entityPreset))
            {
                return;
            }

            if (!TdGameplayController.Instance.Paths
                    .TryGetValue(wavePath.pathId, out SplineContainer pathMover))
            {
                return;
            }

            for (int i = 0; i < command.quantity; i++)
            {
                //spawn entity in path
                Debug.Log($"Spawning command {command.entityId} - time {i}");

                GameObject enemy = TdSpawnManager.Instance.RuntimePools[TdSpawnKey.Enemy].Get();
                //entity setup
                if (enemy.TryGetComponent(out EntityRuntime entityRuntime))
                {
                    entityRuntime.Setup(entityPreset);
                    entityRuntime.OnDead =
                        () => TdSpawnManager.Instance.RuntimePools[TdSpawnKey.Enemy].Release(enemy);
                }

                if (enemy.TryGetComponent(out EnemyMover enemyMover))
                {
                    enemyMover.Setup(pathMover);
                }

                await UniTask.Delay((int)(command.spawnInterval * 1000));
            }
        }
    }
}