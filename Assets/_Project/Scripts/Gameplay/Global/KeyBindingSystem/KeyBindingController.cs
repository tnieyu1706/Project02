using System;
using System.Collections.Generic;
using UnityEngine;
using TnieYuPackage.DesignPatterns;

namespace Game.Global
{
    public class KeyBindingController : Singleton<KeyBindingController>
    {
        private Dictionary<KeyCode, Action> keyBindings = new Dictionary<KeyCode, Action>();
        
        // Cache list để tránh cấp phát bộ nhớ mỗi frame và tránh lỗi "Collection was modified" khi đang lặp
        private List<Action> actionsToInvoke = new List<Action>(); 

        public void RegisterAction(KeyCode key, Action action)
        {
            if (keyBindings.ContainsKey(key))
            {
                keyBindings[key] += action;
            }
            else
            {
                keyBindings.Add(key, action);
            }
        }

        public void UnregisterAction(KeyCode key, Action action)
        {
            if (keyBindings.ContainsKey(key))
            {
                keyBindings[key] -= action;
                
                // Quan trọng: Dọn dẹp key nếu không còn action nào để tránh rác Dictionary
                if (keyBindings[key] == null)
                {
                    keyBindings.Remove(key);
                }
            }
        }

        private void Update()
        {
            actionsToInvoke.Clear();

            // 1. Quét qua Dictionary để tìm các phím đang được bấm
            foreach (var kvp in keyBindings)
            {
                // Dùng GetKeyDown để mô phỏng chính xác hành vi "Click" 1 lần của Button
                if (Input.GetKeyDown(kvp.Key)) 
                {
                    if (kvp.Value != null)
                    {
                        actionsToInvoke.Add(kvp.Value);
                    }
                }
            }

            // 2. Kích hoạt các Action bên ngoài vòng lặp foreach của Dictionary
            foreach (var action in actionsToInvoke)
            {
                action.Invoke();
            }
        }

#if UNITY_EDITOR
        // Hàm hỗ trợ cho Editor script để đọc dữ liệu (chỉ view)
        public IReadOnlyDictionary<KeyCode, Action> GetKeyBindingsForEditor()
        {
            return keyBindings;
        }
#endif
    }
}