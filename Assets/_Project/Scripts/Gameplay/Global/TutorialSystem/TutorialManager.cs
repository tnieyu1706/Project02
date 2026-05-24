using System;
using UnityEngine;

namespace Game.Global.TutorialSystem
{

    /// <summary>
    /// Manages the runtime state and progression of tutorials. Decoupled from UI via events.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class TutorialManager : MonoBehaviour
    {
        /// <summary>Singleton instance of the TutorialManager.</summary>
        public static TutorialManager Instance { get; private set; }

        /// <summary>Event dispatched when a new tutorial step starts. Passes step data and target anchor transform.</summary>
        public event Action<TutorialStepData, Transform> OnStepStarted;

        /// <summary>Event dispatched when the active tutorial ends or is completed.</summary>
        public event Action OnTutorialEnded;

        private const string PLAYER_PREFS_PREFIX = "Tutorial_Complete_";

        private TutorialData _currentTutorialSequence;
        private int _currentIndex = -1;
        private bool _isTutorialActive = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>Starts a tutorial sequence from the beginning.</summary>
        public void StartTutorial(TutorialData tutorialData, bool forceRestart = false)
        {
            if (tutorialData == null || tutorialData.Steps.Count == 0)
            {
                Debug.LogWarning("[Tutorial] Cannot start tutorial: Data is null or contains no steps.");
                return;
            }

            if (!forceRestart && HasCompletedTutorial(tutorialData.TutorialId))
            {
                Debug.Log($"[Tutorial] Tutorial '{tutorialData.TutorialId}' already completed. Skipping.");
                return;
            }

            _currentTutorialSequence = tutorialData;
            _currentIndex = 0;
            _isTutorialActive = true;

            Debug.Log($"[Tutorial] Starting tutorial: {tutorialData.TutorialId}");
            ShowCurrentStep();
        }

        /// <summary>Advances to the next step if the triggered step matches the current expected step.</summary>
        public void Next(TutorialStepData triggeredStep)
        {
            if (!_isTutorialActive || _currentTutorialSequence == null) return;
            if (_currentIndex < 0 || _currentIndex >= _currentTutorialSequence.Steps.Count) return;

            if (_currentTutorialSequence.Steps[_currentIndex] != triggeredStep) return;

            AdvanceToNextStep();
        }

        /// <summary>Increments the sequence index and updates the tutorial state.</summary>
        public void AdvanceToNextStep()
        {
            _currentIndex++;

            if (_currentIndex >= _currentTutorialSequence.Steps.Count)
            {
                EndTutorial();
                return;
            }

            ShowCurrentStep();
        }

        private void ShowCurrentStep()
        {
            if (_currentTutorialSequence == null || _currentIndex < 0 || _currentIndex >= _currentTutorialSequence.Steps.Count) return;

            TutorialStepData stepData = _currentTutorialSequence.Steps[_currentIndex];
            Transform targetAnchorTransform = null;

            if (stepData.DisplayType == TutorialDisplayType.Hint || stepData.DisplayType == TutorialDisplayType.TextHint)
            {
                TutorialAnchor targetAnchor = TutorialAnchorRegistry.GetAnchor(stepData);
                if (targetAnchor != null)
                {
                    targetAnchorTransform = targetAnchor.transform;
                }
                else
                {
                    Debug.LogWarning($"[Tutorial] Anchor not found for step {stepData.name}.");
                }
            }

            OnStepStarted?.Invoke(stepData, targetAnchorTransform);
        }

        /// <summary>Forcefully ends the current tutorial and hides UI.</summary>
        public void EndTutorial()
        {
            if (_currentTutorialSequence != null)
            {
                PlayerPrefs.SetInt(GetSavePath(_currentTutorialSequence.TutorialId), 1);
                PlayerPrefs.Save();
            }
            
            _isTutorialActive = false;
            _currentTutorialSequence = null;
            _currentIndex = -1;

            OnTutorialEnded?.Invoke();
        }

        /// <summary>Checks if a tutorial has been marked as completed in PlayerPrefs.</summary>
        public bool HasCompletedTutorial(string tutorialId)
        {
            return PlayerPrefs.GetInt(GetSavePath(tutorialId), 0) == 1;
        }

        private static string GetSavePath(string tutorialId)
        {
            return PLAYER_PREFS_PREFIX + tutorialId;
        }

        /// <summary>Clears completion status for a specific tutorial in PlayerPrefs.</summary>
        public void ResetTutorialProgress(string tutorialId)
        {
            PlayerPrefs.DeleteKey(GetSavePath(tutorialId));
            PlayerPrefs.Save();
        }
    }
}