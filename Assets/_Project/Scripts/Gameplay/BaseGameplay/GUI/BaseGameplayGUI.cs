using System;
using _Project.Scripts.Gameplay.Global.GameController;
using Gameplay.Global;
using Cysharp.Threading.Tasks;
using EditorAttributes;
using Reflex.Attributes;
using TnieYuPackage.DesignPatterns;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

namespace Game.BaseGameplay
{
    public class BaseGameplayGUI : Singleton<BaseGameplayGUI>
    {
        [Inject] GameplayTransition transition;

        [SerializeField, Required] private Button playButton;

        [SerializeField, Required] private Text baseHealthText;
        [SerializeField, Required] private Text currentWaveText;
        [SerializeField, Required] private Text maxWaveText;
        [SerializeField, Required] private Text moneyText;

        [SerializeField, Required] private GameObject winPanel;
        [SerializeField, Required] private GameObject losePanel;

        private float preOpenMenuTimeScale;
        private int preOpenMenuFrameRate;

        public event Action OnPlayButtonPressed;

        private void OnDestroy()
        {
            OnMenuPanelClosed();
        }

        #region EVENTS

        public void OnEnable()
        {
            playButton.onClick.AddListener(HandlePlayButtonClicked);

            BaseGameplayController.Instance.baseHealth.OnValueChanged += HandleBaseHealthChanged;
            BaseGameplayController.Instance.money.OnValueChanged += HandleMoneyChanged;
            BaseGameplayController.Instance.currentWaveIndex.OnValueChanged += HandleCurrentWaveIndexChanged;
            BaseGameplayController.Instance.maxWaveIndex.OnValueChanged += HandleMaxWaveIndexChanged;

            BaseGameplayController.Instance.OnWaveStarted += HandleWaveStarting;
            BaseGameplayController.Instance.OnWaveEnded += HandleWaveCompleted;

            Debug.Log($"[TdWaveController] OnEnable");
        }

        private void HandlePlayButtonClicked()
        {
            OnPlayButtonPressed?.Invoke();
        }

        private void HandleWaveStarting()
        {
            playButton.interactable = false;
        }

        private void HandleWaveCompleted()
        {
            playButton.interactable = true;
        }

        private void HandleBaseHealthChanged(int changedHealth)
        {
            baseHealthText.text = $"{changedHealth}";
        }

        private void HandleMoneyChanged(int changedMoney)
        {
            moneyText.text = $"{changedMoney}";
        }

        private void HandleCurrentWaveIndexChanged(int changedWave)
        {
            currentWaveText.text = $"{changedWave}";
        }

        private void HandleMaxWaveIndexChanged(int changedMaxWave)
        {
            maxWaveText.text = $"{changedMaxWave}";
        }

        public void OnDisable()
        {
            playButton.onClick?.RemoveListener(HandlePlayButtonClicked);

            if (BaseGameplayController.HasInstance)
            {
                BaseGameplayController.Instance.OnWaveStarted -= HandleWaveStarting;
                BaseGameplayController.Instance.OnWaveEnded -= HandleWaveCompleted;

                BaseGameplayController.Instance.baseHealth.OnValueChanged -= HandleBaseHealthChanged;
                BaseGameplayController.Instance.money.OnValueChanged -= HandleMoneyChanged;
                BaseGameplayController.Instance.currentWaveIndex.OnValueChanged -= HandleCurrentWaveIndexChanged;
                BaseGameplayController.Instance.maxWaveIndex.OnValueChanged -= HandleMaxWaveIndexChanged;
            }

            Debug.Log($"[TdWaveController] OnDisable");
        }

        #endregion

        public void HandleConfirmGameplayButtonClicked()
        {
            transition.LoadBuildingGameplay().Forget();
        }

        public void OpenWinPanel()
        {
            winPanel.SetActive(true);
            OnMenuPanelOpened();
        }

        public void OpenLosePanel()
        {
            losePanel.SetActive(true);
            OnMenuPanelOpened();
        }

        public void OnMenuPanelOpened()
        {
            preOpenMenuTimeScale = GameTimeController.TimeScale;
            preOpenMenuFrameRate = GameTimeController.TargetFrameRate;
            GameTimeController.SetGameStop();
        }

        public void OnMenuPanelClosed()
        {
            if (GameTimeController.HasInstance)
            {
                GameTimeController.SetTimeScale(preOpenMenuTimeScale);
                GameTimeController.SetFrameRate(preOpenMenuFrameRate);
            }
        }
    }
}