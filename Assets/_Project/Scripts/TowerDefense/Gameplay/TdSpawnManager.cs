using System;
using System.Collections.Generic;
using TnieYuPackage.DictionaryUtilities;
using EditorAttributes;
using TnieYuPackage.DesignPatterns;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace Game.Td
{
    public enum TdSpawnKey
    {
        Enemy,
        Soldier,
        Projectile
    }

    [Serializable]
    public class GameObjectPoolBuilder : IMyBuilder<ObjectPool<GameObject>>
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

        public ObjectPool<GameObject> Build()
        {
            return new ObjectPool<GameObject>(
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
    public class TdSpawnManager : SingletonBehavior<TdSpawnManager>
    {
        [SerializeField] private SerializableDictionary<TdSpawnKey, GameObjectPoolBuilder> pools = new();

        public readonly Dictionary<TdSpawnKey, ObjectPool<GameObject>> RuntimePools = new();

        protected override void Awake()
        {
            base.Awake();
            
            InitializePools();
        }

        private void InitializePools()
        {
            RuntimePools.Clear();

            foreach (var eKvp in pools.Dictionary)
            {
                RuntimePools[eKvp.Key] = eKvp.Value.Build();
            }
        }
        
        //Test
        [Button]
        public void Get(TdSpawnKey key)
        {
            RuntimePools[key].Get();
        }

        public GameObject objectRelease;

        [Button]
        public void Release(TdSpawnKey key)
        {
            RuntimePools[key].Release(objectRelease);
        }
    }
}