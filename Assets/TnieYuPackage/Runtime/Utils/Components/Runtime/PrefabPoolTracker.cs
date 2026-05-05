    using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

namespace TnieYuPackage.Utils
{
    public abstract class ObjectPoolTracker<T> : IObjectPool<T>
        where T : class
    {
        protected ObjectPool<T> Pool { get; }
        public List<T> ActiveObjects { get; } = new List<T>();

        private Action<T> OnGet { get; }
        private Action<T> OnRelease { get; }

        private UniTaskCompletionSource tcs;

        public ObjectPoolTracker(
            Func<T> onCreate,
            Action<T> onGet,
            Action<T> onRelease,
            Action<T> onDestroy,
            bool collectionCheck,
            int defaultCapacity,
            int maxCapacity
        )
        {
            OnGet = onGet;
            OnRelease = onRelease;

            Pool = new ObjectPool<T>(
                onCreate,
                GetProcedure,
                ReleaseProcedure,
                onDestroy,
                collectionCheck,
                defaultCapacity,
                maxCapacity
            );
        }

        protected virtual void ReleaseProcedure(T obj)
        {
            OnRelease?.Invoke(obj);
            ActiveObjects.Remove(obj);
            OnObjectInactive();
        }

        protected virtual void GetProcedure(T obj)
        {
            OnGet?.Invoke(obj);
            ActiveObjects.Add(obj);
        }

        protected virtual void OnObjectInactive()
        {
            if (ActiveObjects.Count != 0 || tcs == null) return;

            tcs.TrySetResult();
            tcs = null;
        }

        /// <summary>
        /// A task wait for all objects will be released. If there is no active object, the task will complete immediately.
        /// </summary>
        /// <returns></returns>
        public virtual UniTask Waiting()
        {
            if (ActiveObjects.Count == 0)
                return UniTask.CompletedTask;

            tcs = new UniTaskCompletionSource();
            return tcs.Task;
        }

        public void ReleaseAll() => ActiveObjects.ForEach(Release);

        public T Get() => Pool.Get();

        public PooledObject<T> Get(out T v) => Pool.Get(out v);

        public void Release(T element) => Pool.Release(element);

        public void Clear() => Pool.Clear();

        public int CountInactive => Pool.CountInactive;
    }

    public class PrefabPoolTracker : ObjectPoolTracker<GameObject>
    {
        public PrefabPoolTracker(
            Func<GameObject> onCreate,
            Action<GameObject> onGet,
            Action<GameObject> onRelease,
            Action<GameObject> onDestroy,
            bool collectionCheck,
            int defaultCapacity,
            int maxCapacity
        ) : base(onCreate, onGet, onRelease, onDestroy, collectionCheck, defaultCapacity, maxCapacity)
        {
        }
    }
}