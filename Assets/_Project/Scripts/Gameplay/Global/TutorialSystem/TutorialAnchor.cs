using UnityEngine;

namespace Game.Global.TutorialSystem
{
    /// <summary>
    /// Gắn script này vào các UI Element (hoặc GameObject) mà bạn muốn vòng sáng chỉ vào.
    /// </summary>
    [DisallowMultipleComponent]
    public class TutorialAnchor : MonoBehaviour
    {
        [Tooltip("Kéo trực tiếp file TutorialStepData (Sub-asset) tương ứng vào đây thay vì gõ ID.")]
        public TutorialStepData TargetStep;

        private void OnEnable()
        {
            TutorialAnchorRegistry.RegisterAnchor(this);
        }

        private void OnDisable()
        {
            TutorialAnchorRegistry.UnregisterAnchor(this);
        }

        /// <summary>
        /// Gọi hàm này từ Event Trigger hoặc OnClick của Button để hoàn thành Step này.
        /// </summary>
        public void TriggerNextStep()
        {
            if (TargetStep != null)
            {
                TutorialManager.Instance.Next(TargetStep);
            }
        }
    }
}