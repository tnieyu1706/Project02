using UnityEngine;

namespace Game.Global.TutorialSystem
{
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }

        [Header("Configuration")] [SerializeField]
        private TutorialUI tutorialUI;

        [SerializeField] private string playerPrefsPrefix = "Tutorial_Complete_";

        private TutorialData currentTutorialSequence;
        private int currentStepIndex = -1;
        private bool isTutorialActive = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void StartTutorial(TutorialData tutorialData, bool forceRestart = false)
        {
            if (tutorialData == null || tutorialData.Steps.Count == 0) return;

            if (!forceRestart && HasCompletedTutorial(tutorialData.TutorialId)) return;

            currentTutorialSequence = tutorialData;
            currentStepIndex = 0;
            isTutorialActive = true;

            ShowCurrentStep();
        }

        public void Next()
        {
            if (!isTutorialActive) return;
            currentStepIndex++;
            ShowCurrentStep();
        }

        public void Next(string triggeredNextId)
        {
            if (!isTutorialActive || currentTutorialSequence == null ||
                currentStepIndex >= currentTutorialSequence.Steps.Count)
                return;

            TutorialStepData currentStep = currentTutorialSequence.Steps[currentStepIndex];

            if (!string.IsNullOrEmpty(currentStep.ExpectedNextId) && currentStep.ExpectedNextId != triggeredNextId)
            {
                return;
            }

            currentStepIndex++;
            ShowCurrentStep();
        }

        private void ShowCurrentStep()
        {
            if (currentTutorialSequence == null) return;

            if (currentStepIndex >= currentTutorialSequence.Steps.Count)
            {
                CompleteTutorial();
                return;
            }

            TutorialStepData stepData = currentTutorialSequence.Steps[currentStepIndex];
            Vector3 targetPosition = Vector3.zero;

            // Nếu Step yêu cầu hiển thị Hint, ta đi tìm tọa độ Vector3 của Anchor đó
            if ((stepData.DisplayType == TutorialDisplayType.Hint ||
                 stepData.DisplayType == TutorialDisplayType.TextHint)
                && !string.IsNullOrEmpty(stepData.AnchorId))
            {
                TutorialAnchor targetAnchor = TutorialAnchorRegistry.GetAnchor(stepData.AnchorId);

                if (targetAnchor != null)
                {
                    targetPosition = targetAnchor.transform.position;
                }
                else
                {
                    Debug.LogWarning($"[Tutorial] Không tìm thấy Anchor có ID: {stepData.AnchorId} trên Scene.");
                }
            }

            if (tutorialUI != null)
            {
                // Truyền cả dữ liệu và tọa độ Vector3 cho UI
                tutorialUI.ShowStep(stepData, targetPosition);
            }
        }

        private void CompleteTutorial()
        {
            if (currentTutorialSequence != null)
            {
                PlayerPrefs.SetInt(playerPrefsPrefix + currentTutorialSequence.TutorialId, 1);
                PlayerPrefs.Save();
            }

            EndTutorial();
        }

        public void EndTutorial()
        {
            isTutorialActive = false;
            currentTutorialSequence = null;
            currentStepIndex = -1;
            if (tutorialUI != null) tutorialUI.Hide();
        }

        public bool HasCompletedTutorial(string tutorialId)
        {
            return PlayerPrefs.GetInt(playerPrefsPrefix + tutorialId, 0) == 1;
        }

        public void ResetTutorialProgress(string tutorialId)
        {
            PlayerPrefs.DeleteKey(playerPrefsPrefix + tutorialId);
            PlayerPrefs.Save();
        }
    }
}