using System;
using Cysharp.Threading.Tasks;
using Gameplay.Global;
using Reflex.Attributes;
using TnieYuPackage.DesignPatterns;
using UnityEngine;

namespace Game.BuildingGameplay
{
    [DefaultExecutionOrder(-8)]
    public class BuildingGameMenuGUI : Singleton<BuildingGameMenuGUI>
    {
        [Header("Panels")] [SerializeField] private GameObject winPanel;
        [SerializeField] private GameObject losePanel;

        [SerializeField] private float delayDisplaySeconds = 2f;

        [Inject] private GameplayTransition transition;

        private void OnEnable()
        {
            SbGameplayController.OnWinGame += HandleOnGameWin;
            SbGameplayController.OnLoseGame += HandleOnGameLose;
        }

        private void OnDisable()
        {
            SbGameplayController.OnWinGame -= HandleOnGameWin;
            SbGameplayController.OnLoseGame -= HandleOnGameLose;
        }

        private async void HandleOnGameWin()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delayDisplaySeconds));
            winPanel.SetActive(true);
        }

        private async void HandleOnGameLose()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delayDisplaySeconds));
            losePanel.SetActive(true);
        }

        public void HandleConfirmButton()
        {
            Instance.transition.LoadWorldMapGame().Forget();
        }
    }
}