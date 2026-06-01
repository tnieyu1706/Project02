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

        [SerializeField] private float delayEndPanelDisplaySeconds = 1f;
        [SerializeField, Required] private GameObject winPanel;
        [SerializeField, Required] private GameObject losePanel;

        [Header("Options")] [SerializeField] private Button gameSpeedButton;
        [SerializeField] private Text gameSpeedText;
        [SerializeField] private int maxTimeScale = 3;

        private bool hasTimeStop;
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

            {
                gameSpeedButton.onClick.AddListener(HandleGameSpeedChangeWithStatic);
                SetTimeScaleWithUI(Mathf.FloorToInt(GameTimeController.TimeScale));
            }

            BaseGameplayController.Instance.baseHealth.OnValueChanged += HandleBaseHealthChanged;
            BaseGameplayController.Instance.money.OnValueChanged += HandleMoneyChanged;
            BaseGameplayController.Instance.currentWaveIndex.OnValueChanged += HandleCurrentWaveIndexChanged;
            BaseGameplayController.Instance.maxWaveIndex.OnValueChanged += HandleMaxWaveIndexChanged;

            BaseGameplayController.Instance.OnWaveStarted += HandleWaveStarting;
            BaseGameplayController.Instance.OnWaveEnded += HandleWaveCompleted;
            
            // HandleBaseHealthChanged(BaseGameplayController.Instance.baseHealth.Value);
            // HandleMoneyChanged(BaseGameplayController.Instance.money.Value);
            // HandleCurrentWaveIndexChanged(BaseGameplayController.Instance.currentWaveIndex.Value);
            // HandleMaxWaveIndexChanged(BaseGameplayController.Instance.maxWaveIndex.Value);

            Debug.Log($"[TdWaveController] OnEnable");
        }

        private void HandleGameSpeedChangeWithStatic()
        {
            int curTimeScale = Mathf.FloorToInt(GameTimeController.TimeScale);
            // TODO: Get next time scale from current and maxTimeScale setup
            var nextTimeScale = (curTimeScale % maxTimeScale) + 1;

            SetTimeScaleWithUI(nextTimeScale);
        }

        private void SetTimeScaleWithUI(int nextTimeScale)
        {
            GameTimeController.SetTimeScale(nextTimeScale);
            gameSpeedText.text = nextTimeScale.ToString();
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
            gameSpeedButton.onClick.RemoveAllListeners();
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

        public async void HandleConfirmGameplayButtonClicked()
        {
            await UniTask.NextFrame(cancellationToken: this.GetCancellationTokenOnDestroy());
            transition.LoadBuildingGameplay().Forget();
        }

        public async void OpenWinPanel()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delayEndPanelDisplaySeconds));

            winPanel.SetActive(true);

            OnMenuPanelOpened();
        }

        public async void OpenLosePanel()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delayEndPanelDisplaySeconds));

            losePanel.SetActive(true);

            OnMenuPanelOpened();
        }

        public void OnMenuPanelOpened()
        {
            preOpenMenuTimeScale = GameTimeController.TimeScale;
            preOpenMenuFrameRate = GameTimeController.TargetFrameRate;
            GameTimeController.SetGameStop();
            hasTimeStop = true;
        }

        public void OnMenuPanelClosed()
        {
            if (GameTimeController.HasInstance && hasTimeStop)
            {
                GameTimeController.SetTimeScale(preOpenMenuTimeScale);
                GameTimeController.SetFrameRate(preOpenMenuFrameRate);
            }
        }
    }
}