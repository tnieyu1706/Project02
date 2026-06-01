using KBCore.Refs;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.BaseGameplay
{
    /// <summary>
    /// Gắn component này vào GameObject chứa Collider của Tower/Soldier/Enemy.
    /// Nó sẽ tự tìm IBaseObjectInfoProvider trên cùng GameObject để gọi Display.
    /// </summary>
    public class BaseObjectHoverInfoTrigger : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private InterfaceRef<IBaseObjectInfoProvider> infoProvider;

        // private bool isInfoDisplayed;

        private void Awake()
        {
            if (infoProvider == null)
            {
                Debug.LogWarning($"HoverInfoTrigger cần IBaseObjectInfoProvider trên {gameObject.name}");
            }
        }

        // void OnDisable()
        // {
        //     if (isInfoDisplayed)
        //     {
        //         BaseObjectInfoDisplay.Instance.Hide();
        //         isInfoDisplayed = false;
        //     }
        // }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (infoProvider != null && BaseObjectInfoDisplay.HasInstance)
            {
                BaseObjectInfoDisplay.Instance.Display(infoProvider.Value);
                // isInfoDisplayed = true;
            }
        }

        // public void OnPointerUp(PointerEventData eventData)
        // {
        //     if (BaseObjectInfoDisplay.HasInstance)
        //     {
        //         BaseObjectInfoDisplay.Instance.Hide();
        //         isInfoDisplayed = false;
        //     }
        // }
    }
}