using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TnieYuPackage.DesignPatterns;
using UnityEngine;

namespace TnieYuPackage.Core
{
    [DefaultExecutionOrder(-100)]
    public class EventManager : SingletonBehavior<EventManager>
    {
        private readonly Queue<Action> queue = new();

        protected override void Awake()
        {
            dontDestroyOnLoad = true;
            base.Awake();
        }

        void Update()
        {
            while (queue.TryDequeue(out var action))
            {
                action?.Invoke();
            }
        }

        private void OnDestroy()
        {
            queue.Clear();
        }

        public void Registry(Action action)
        {
            queue.Enqueue(action);
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public async void RegistryDelay(Action action, float delay, CancellationToken token = default)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: token);

            Registry(action);
        }
    }
}