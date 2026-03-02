using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TnieYuPackage.DesignPatterns.Patterns.Singleton;
using UnityEngine;

namespace TnieYuPackage.Core
{
    [DefaultExecutionOrder(-500)]
    public class EventManager : SingletonBehavior<EventManager>
    {
        private readonly Queue<Action> queue = new();

        void Update()
        {
            while (queue.TryDequeue(out var action))
            {
                action?.Invoke();
            }
        }

        public void Registry(Action action)
        {
            queue.Enqueue(action);
        }

        public async void RegistryDelay(Action action, float delay)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delay));

            Registry(action);
        }
    }
}