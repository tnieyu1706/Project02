using System.Collections.Generic;
using TnieYuPackage.DesignPatterns;
using TnieYuPackage.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Global.TutorialSystem
{
    /// <summary>
    /// Frontend component for rendering tutorial messages and hints in Screen Space Overlay.
    /// </summary>
    public class TutorialUI : MonoBehaviour
    {
        [Header("Static UI References")] [SerializeField]
        private GameObject messagePanel;

        [SerializeField] private Text messageText;
        [SerializeField] private Button messageButton;
        
        [Header("Dynamic UI References")] [SerializeField]
        private RectTransform hintRectTransform;

        private TutorialStepData _currentStep;
        private Transform _targetAnchor;
        private Camera _cachedCamera;

        private void OnEnable()
        {
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.OnStepStarted += HandleStepStarted;
                TutorialManager.Instance.OnTutorialEnded += Hide;
            }

            if (messageButton != null)
            {
                messageButton.onClick.AddListener(OnMessageClicked);
            }

            Hide();

            Debug.Log("[TutorialSystem] Tutorial UI enabled and subscribed to TutorialManager events.");
        }

        private void OnDisable()
        {
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.OnStepStarted -= HandleStepStarted;
                TutorialManager.Instance.OnTutorialEnded -= Hide;
            }

            if (messageButton != null)
            {
                messageButton.onClick.RemoveAllListeners();
            }
        }

        private void Update()
        {
            if (_targetAnchor != null && hintRectTransform != null && hintRectTransform.gameObject.activeSelf)
            {
                UpdateHintPosition();
            }
        }

        private void OnMessageClicked()
        {
            // if (_currentStep != null && _currentStep.DisplayType == TutorialDisplayType.Text)
            if (_currentStep != null)
            {
                TutorialManager.Instance.Next(_currentStep);
            }
        }

        private void HandleStepStarted(TutorialStepData stepData, Transform targetAnchor)
        {
            _currentStep = stepData;
            _targetAnchor = targetAnchor;

            // 1. Static Message Panel
            bool showMessage = stepData.DisplayType == TutorialDisplayType.Text ||
                               stepData.DisplayType == TutorialDisplayType.TextHint;

            if (messagePanel != null) messagePanel.SetActive(showMessage);
            if (showMessage && messageText != null) messageText.text = stepData.Message;

            // 2. Dynamic Hint Element
            bool showHint = (stepData.DisplayType == TutorialDisplayType.Hint ||
                             stepData.DisplayType == TutorialDisplayType.TextHint) && _targetAnchor != null;

            if (hintRectTransform != null)
            {
                hintRectTransform.gameObject.SetActive(showHint);
                if (showHint) UpdateHintPosition();
            }
        }

        private void UpdateHintPosition()
        {
            if (_targetAnchor == null) return;

            if (_cachedCamera == null)
            {
                _cachedCamera = Registry<Camera>.GetFirst();

                if (_cachedCamera == null) _cachedCamera = Camera.main;
            }

            if (_cachedCamera == null) return;

            Vector3 screenPos = _cachedCamera.WorldToScreenPoint(_targetAnchor.position);

            if (screenPos.z < 0)
            {
                hintRectTransform.gameObject.SetActive(false);
                return;
            }

            if (!hintRectTransform.gameObject.activeSelf) hintRectTransform.gameObject.SetActive(true);

            hintRectTransform.position = screenPos;
        }

        /// <summary>Hides all tutorial UI elements.</summary>
        public void Hide()
        {
            _targetAnchor = null;
            if (messagePanel != null) messagePanel.SetActive(false);
            if (hintRectTransform != null) hintRectTransform.gameObject.SetActive(false);
        }
    }
}