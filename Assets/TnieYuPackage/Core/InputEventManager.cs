using System;
using System.Collections.Generic;
using TnieYuPackage.DesignPatterns;
using UnityEngine;

namespace TnieYuPackage.Core
{
    /// <summary>
    /// Input Event System support for Registry/UnRegistry & relative handle
    /// for Input: Mouse, Keyboard
    /// Once Key select per frame.
    /// Once Key contain 1 Action.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class InputEventManager : SingletonBehavior<InputEventManager>
    {
        private readonly Queue<Action> events = new();

        private Dictionary<KeyCode, Action> keyboardEvents = new();

        public Action EnableEvent;
        public Action DisableEvent;

        void Start()
        {
            Instance.enabled = false;
        }

        private void OnEnable()
        {
            EnableEvent?.Invoke();
        }

        private void OnDisable()
        {
            DisableEvent?.Invoke();
        }

        private KeyCode currentKey;

        private void Update()
        {
            if (!Input.anyKeyDown) return;

            foreach (var key in keyboardEvents.Keys)
            {
                if (!Input.GetKeyDown(key)) continue;

                keyboardEvents[key]?.Invoke();
                keyboardEvents.Remove(key);
                return;
            }
        }

        public void RegistryOnce(KeyCode key, Action action)
        {
            keyboardEvents.Add(key, action);
        }

        public void UnRegistryKey(KeyCode key)
        {
            keyboardEvents.Remove(key);
        }
    }
}