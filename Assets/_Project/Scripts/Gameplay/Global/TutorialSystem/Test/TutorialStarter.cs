using System;
using Cysharp.Threading.Tasks;
using EditorAttributes;
using UnityEngine;

namespace Game.Global.TutorialSystem.Test
{
    public class TutorialStarter : MonoBehaviour
    {
        [SerializeField] private TutorialData _tutorialData;
        [SerializeField] private bool _forceRestart = false;
        [SerializeField] private float tutorialStartDelay = 0.5f;

        void Start()
        {
            if (TutorialManager.Instance == null)
            {
                Debug.LogWarning(
                    "[TutorialStarter] TutorialManager.Instance is null at Start. Make sure it is initialized in the Bootstrapper or present in the scene.");
                return;
            }

            StartTutorial();
        }

        private async void StartTutorial()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(tutorialStartDelay));
            
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.StartTutorial(_tutorialData, _forceRestart);
            }
        }

        [Button]
        private void ResetProgress()
        {
            if (TutorialManager.Instance != null && _tutorialData != null)
            {
                TutorialManager.Instance.ResetTutorialProgress(_tutorialData.TutorialId);
                Debug.Log($"[TutorialStarter] Reset progress for tutorial: {_tutorialData.TutorialId}");
            }
        }

        [Button]
        private void NextStepImmediately()
        {
            if (TutorialManager.Instance != null && _tutorialData != null && _tutorialData.Steps.Count > 0)
            {
                TutorialManager.Instance.AdvanceToNextStep();
            }
        }
    }
}