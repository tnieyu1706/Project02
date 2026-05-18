using UnityEngine;

namespace Game.Global.TutorialSystem
{
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private TutorialUI tutorialUI;
        [SerializeField] private string playerPrefsPrefix = "Tutorial_Complete_";

        private TutorialData currentTutorialSequence;
        private TutorialStepData currentStepData;
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
            currentStepData = tutorialData.StartStep != null ? tutorialData.StartStep : tutorialData.Steps[0];
                
            if (currentStepData == null) return;

            isTutorialActive = true;
            ShowCurrentStep();
        }

        public void Next()
        {
            if (!isTutorialActive || currentStepData == null) return;
            AdvanceToNextStep();
        }

        /// <summary>
        /// Hệ thống Tracker mới: Nhận vào trực tiếp ScriptableObject.
        /// </summary>
        public void Next(TutorialStepData triggeredStep)
        {
             if (!isTutorialActive || currentTutorialSequence == null || currentStepData == null) return;

             // Kiểm tra xem sự kiện gửi lên có đúng với Step đang chờ hiện tại hay không
             if (currentStepData != triggeredStep) return;

             AdvanceToNextStep();
        }

        private void AdvanceToNextStep()
        {
            if (currentStepData.NextStep == null)
            {
                CompleteTutorial();
                return;
            }

            currentStepData = currentStepData.NextStep;
            ShowCurrentStep();
        }

        private void ShowCurrentStep()
        {
            if (currentTutorialSequence == null || currentStepData == null) return;

            Vector3 targetPosition = Vector3.zero;

            if (currentStepData.DisplayType == TutorialDisplayType.Hint || currentStepData.DisplayType == TutorialDisplayType.TextHint)
            {
                // Dùng chính currentStepData để tìm Anchor
                TutorialAnchor targetAnchor = TutorialAnchorRegistry.GetAnchor(currentStepData);
                
                if (targetAnchor != null)
                {
                    targetPosition = targetAnchor.transform.position;
                }
                else
                {
                    Debug.LogWarning($"[Tutorial] Không tìm thấy Anchor nào chứa targetStep {currentStepData.name} trên Scene.");
                }
            }

            if (tutorialUI != null) tutorialUI.ShowStep(currentStepData, targetPosition);
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
            currentStepData = null;
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