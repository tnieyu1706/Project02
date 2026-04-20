using System;
using System.Collections.Generic;
using System.Threading;
using TnieYuPackage.DesignPatterns;
using TnieYuPackage.DictionaryUtilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TnieYuPackage.Utils
{
    [Serializable]
    public class GameObjectPoolBuilder : IMyBuilder<PrefabPoolTracker>
    {
        public GameObject prefab;
        public bool collectionCheck = true;
        public int defaultCapacity;
        public int maxCapacity;

        private GameObject OnCreate()
        {
            return Object.Instantiate(prefab);
        }

        private void OnGet(GameObject go)
        {
            go.SetActive(true);
        }

        private void OnRelease(GameObject go)
        {
            go.SetActive(false);
        }

        private void OnDestroy(GameObject go)
        {
            Object.DestroyImmediate(go);
        }

        public PrefabPoolTracker Build()
        {
            // BLogger.Log($"[TdSpawnManager] Creating pool for {prefab.name}", category:"TD");
            return new PrefabPoolTracker(
                OnCreate,
                OnGet,
                OnRelease,
                OnDestroy,
                collectionCheck,
                defaultCapacity,
                maxCapacity
            );
        }
    }

    [DefaultExecutionOrder(-20)]
    public abstract class PrefabSpawnManager<TKey, TSingleton> : SingletonBehavior<TSingleton>
        where TSingleton : PrefabSpawnManager<TKey, TSingleton>
    {
        [SerializeField] private SerializableDictionary<TKey, GameObjectPoolBuilder> pools = new();

        public Dictionary<TKey, PrefabPoolTracker> PoolTrackers { get; } = new();

        protected override void Awake()
        {
            base.Awake();
            InitializePools();
        }

        protected virtual void InitializePools()
        {
            PoolTrackers.Clear();

            foreach (var eKvp in pools.Dictionary)
            {
                PoolTrackers[eKvp.Key] = eKvp.Value.Build();
            }
        }
    }
}