using System;
using System.Collections.Generic;
using TnieYuPackage.DesignPatterns;
using UnityEngine;

namespace TnieYuPackage.Handlers
{
    [DefaultExecutionOrder(-990)]
    public class InputEventManager : GlobalSingleton<InputEventManager>
    {
        private readonly Dictionary<KeyCode, Func<bool>> keyboardEvents = new();

        public event Action<Vector2> OnMouseMove;

        void Start()
        {
            Instance.enabled = false;
        }

        private void Update()
        {
            OnMouseMove?.Invoke(Input.mousePosition);

            if (!Input.anyKeyDown) return;

            foreach (var key in keyboardEvents.Keys)
            {
                if (!Input.GetKeyDown(key)) continue;

                if (keyboardEvents[key].Invoke())
                {
                    // remove when action -> true.
                    keyboardEvents.Remove(key);
                }

                return;
            }
        }

        public void RegistryOnce(KeyCode key, Func<bool> action)
        {
            keyboardEvents[key] = action;
        }

        public void UnRegistryKey(KeyCode key)
        {
            keyboardEvents.Remove(key);
        }
    }
}