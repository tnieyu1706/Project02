using System;
using System.Collections.Generic;
using TnieYuPackage.DesignPatterns;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TnieYuPackage.Core
{
    [DefaultExecutionOrder(-50)]
    public class MouseEventManager : SingletonBehavior<MouseEventManager>
    {
        private readonly Queue<Action> events = new();

        void Start()
        {
            Instance.enabled = false;
        }

        private void Update()
        {
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;
            
            while (events.TryDequeue(out var action))
            {
                action?.Invoke();
            }
        }

        public void Registry(Action action)
        {
            events.Enqueue(action);
        }
    }
}