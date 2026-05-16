using UnityEngine;

namespace Game.Global.TutorialSystem
{
    /// <summary>
    /// Gắn script này vào các UI Element (hoặc GameObject) mà bạn muốn vòng sáng chỉ vào.
    /// </summary>
    [DisallowMultipleComponent]
    public class TutorialAnchor : MonoBehaviour
    {
        [Tooltip("ID duy nhất để hệ thống tìm thấy điểm neo này (VD: Btn_Play, Inventory_Slot_1).")]
        public string AnchorId;

        private void OnEnable()
        {
            TutorialAnchorRegistry.RegisterAnchor(this);
        }

        private void OnDisable()
        {
            TutorialAnchorRegistry.UnregisterAnchor(this);
        }
    }
}