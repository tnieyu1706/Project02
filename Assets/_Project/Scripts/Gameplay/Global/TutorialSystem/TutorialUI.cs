using UnityEngine;
using UnityEngine.UI;

namespace Game.Global.TutorialSystem
{
    public class TutorialUI : MonoBehaviour
    {
        [Header("Static UI References")] [SerializeField]
        private GameObject messagePanel;

        [SerializeField] private Text messageText;

        [Header("Dynamic UI References")] [SerializeField]
        private RectTransform hintRectTransform;

        private void Awake()
        {
            Hide();
        }

        /// <summary>
        /// Hiển thị Tutorial UI. Nhận vào dữ liệu và vị trí Vector3 (World/UI Space) để đặt Hint.
        /// </summary>
        public void ShowStep(TutorialStepData stepData, Vector3 hintPosition)
        {
            // 1. Static Message Panel
            bool showMessage = stepData.DisplayType == TutorialDisplayType.Text ||
                               stepData.DisplayType == TutorialDisplayType.TextHint;

            if (messagePanel != null)
            {
                messagePanel.SetActive(showMessage);
            }

            if (showMessage && messageText != null)
            {
                messageText.text = stepData.Message;
            }

            // 2. Dynamic Hint Element
            bool showHint = stepData.DisplayType == TutorialDisplayType.Hint ||
                            stepData.DisplayType == TutorialDisplayType.TextHint;

            if (hintRectTransform != null)
            {
                hintRectTransform.gameObject.SetActive(showHint);

                if (showHint)
                {
                    // Di chuyển hint trực tiếp đến tọa độ Vector3 được truyền vào
                    hintRectTransform.position = hintPosition;
                }
            }
        }

        public void Hide()
        {
            if (messagePanel != null) messagePanel.SetActive(false);
            if (hintRectTransform != null) hintRectTransform.gameObject.SetActive(false);
        }
    }
}