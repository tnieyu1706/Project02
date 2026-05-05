using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TnieYuPackage.DesignPatterns;
using UnityEngine;

namespace TnieYuPackage.Handlers
{
    [DefaultExecutionOrder(-1000)]
    public class EventManager : GlobalSingleton<EventManager>
    {
        private readonly Queue<Action> queue = new();

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

        public async UniTask RegistryDelay(Action action, float delay, CancellationToken token = default)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: token);
            Registry(action);
        }
    }
}