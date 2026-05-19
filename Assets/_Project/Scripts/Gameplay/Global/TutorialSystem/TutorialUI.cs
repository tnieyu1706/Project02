using TnieYuPackage.DesignPatterns;
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

        private void Awake()
        {
            if (messageButton != null)
            {
                messageButton.onClick.AddListener(OnMessageClicked);
            }
            Hide();
        }

        private void OnEnable()
        {
            // VIBRA NOTE: Decoupled via events.
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.OnStepStarted += HandleStepStarted;
                TutorialManager.Instance.OnTutorialEnded += Hide;
            }
            
            Debug.Log("[TutorialSystem] Tutorial UI enabled and subscribed to TutorialManager events.");
        }

        private void OnDisable()
        {
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.OnStepStarted -= HandleStepStarted;
                TutorialManager.Instance.OnTutorialEnded -= Hide;
            }
        }

        private void Update()
        {
            // VIBRA NOTE: Update hint position every frame to follow potential moving anchors in Screen Space.
            if (_targetAnchor != null && hintRectTransform != null && hintRectTransform.gameObject.activeSelf)
            {
                UpdateHintPosition();
            }
        }

        private void OnMessageClicked()
        {
            // VIBRA NOTE: If the step is Text-only (no anchor/hint), clicking the message advances the tutorial.
            if (_currentStep != null && _currentStep.DisplayType == TutorialDisplayType.Text)
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

            // VIBRA NOTE: Fetching camera via Registry<Camera> as requested.
            if (_cachedCamera == null)
            {
                _cachedCamera = Registry<Camera>.GetFirst();

                // Fallback to Main Camera if registry is empty
                if (_cachedCamera == null) _cachedCamera = Camera.main;
            }

            if (_cachedCamera == null) return;

            Vector3 screenPos = _cachedCamera.WorldToScreenPoint(_targetAnchor.position);

            // If the anchor is behind the camera, hide the hint
            if (screenPos.z < 0)
            {
                hintRectTransform.gameObject.SetActive(false);
                return;
            }

            // Ensure hint is active if it was hidden by being behind camera
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