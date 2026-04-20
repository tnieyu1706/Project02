using System;
using System.Collections.Generic;
using System.Threading;
using BackboneLogger;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Splines;
using Random = UnityEngine.Random;

namespace Game.BaseGameplay
{
    internal interface ISpawnCommand<in T>
    {
        UniTask Spawn(CancellationToken token, T data);
    }

    [Serializable]
    public class EntitySpawn : ISpawnCommand<PathSpawn>
    {
        private static Vector2 offsetSpawn = new(-0.6f, 0.6f);

        public string entityId;
        public int quantity;
        public float spawnInterval;

        public async UniTask Spawn(CancellationToken token, PathSpawn data)
        {
            var presetOperation = Addressables.LoadAssetAsync<EnemyPresetSo>(entityId);
            var entityPreset = await presetOperation.Task;

            if (entityPreset == null)
            {
                BLogger.Log($"[EntityWave] EnemyPreset {entityId} not found!",
                    category: "Base",
                    level: LogLevel.Critical);
                return;
            }

            if (!PathManager.Instance.Paths
                    .TryGetValue(data.pathId, out SplineContainer pathMover))
            {
                BLogger.Log($"[EntityWave] Path {data.pathId} not found!",
                    category: "Base",
                    level: LogLevel.Critical);
                return;
            }

            for (int i = 0; i < quantity; i++)
            {
                GameObject enemy = BaseGameplayPrefabSpawnManager.Instance.PoolTrackers[PrefabType.BaseEnemy].Get();

                if (enemy.TryGetComponent(out EnemyRuntime entityRuntime))
                {
                    float offset = Random.Range(offsetSpawn.x, offsetSpawn.y);
                    entityRuntime.Setup(entityPreset, pathMover, new Vector2(offset, offset));
                }

                await UniTask.WaitForSeconds(spawnInterval, cancellationToken: token);
            }
        }
    }

    [Serializable]
    public class PathSpawn : ISpawnCommand<KeyframeSpawn>
    {
        public string pathId;
        public List<EntitySpawn> entitySpawns;

        public UniTask Spawn(CancellationToken token, KeyframeSpawn data)
        {
            List<UniTask> tasks = new List<UniTask>(entitySpawns.Count);
            foreach (var entitySpawn in entitySpawns)
            {
                tasks.Add(entitySpawn.Spawn(token, this));
            }

            return UniTask.WhenAll(tasks);
        }
    }

    [Serializable]
    public class KeyframeSpawn : ISpawnCommand<WaveSpawn>
    {
        public float time;
        public List<PathSpawn> pathWaves;

        public UniTask Spawn(CancellationToken token, WaveSpawn data)
        {
            List<UniTask> tasks = new(pathWaves.Count);
            foreach (var pathSpawn in pathWaves)
            {
                tasks.Add(pathSpawn.Spawn(token, this));
            }

            return UniTask.WhenAll(tasks);
        }
    }

    [Serializable]
    public class WaveSpawn : ISpawnCommand<WaveSpawn>
    {
        public List<KeyframeSpawn> keyframes = new();

        public async UniTask Spawn(CancellationToken token, WaveSpawn _ = null)
        {
            if (keyframes.Count <= 0) return;

            for (int i = 0; i < keyframes.Count && !token.IsCancellationRequested; i++)
            {
                var kf = keyframes[i];
                var nextTime = kf.time;
                if (i + 1 < keyframes.Count)
                {
                    nextTime = keyframes[i + 1].time;
                }

                BLogger.Log($"[WaveSpawn] Spawn keyframe {i}-{kf.time:F1}", category: "Base");

                await kf.Spawn(token, this);
                float deltaTime = nextTime - kf.time;

                if (deltaTime <= 0) continue;

                await UniTask.Delay(TimeSpan.FromSeconds(deltaTime), cancellationToken: token);
            }
        }
    }
}