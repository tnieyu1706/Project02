using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Global
{
    [RequireComponent(typeof(Button))]
    public class KeyBindingButtonRegister : MonoBehaviour
    {
        [Tooltip("Phím tắt để kích hoạt button này")] [SerializeField]
        private KeyCode bindKey = KeyCode.None;

        private Button button;

        // Cache delegate để đảm bảo reference truyền vào lúc Register và Unregister là giống hệt nhau
        private Action onClickAction;

        private void Awake()
        {
            button = GetComponent<Button>();
            onClickAction = button.onClick.Invoke;
        }

        private void OnEnable()
        {
            if (bindKey != KeyCode.None)
            {
                KeyBindingController.Instance.RegisterAction(bindKey, onClickAction);
            }
        }

        private void OnDisable()
        {
            // Kiểm tra HasInstance cực kỳ quan trọng. 
            // Nếu không có, khi tắt game (Quit), Singleton có thể bị gọi lại và tự động sinh ra một GameObject rác
            if (bindKey != KeyCode.None && KeyBindingController.HasInstance)
            {
                KeyBindingController.Instance.UnregisterAction(bindKey, onClickAction);
            }
        }
    }
}