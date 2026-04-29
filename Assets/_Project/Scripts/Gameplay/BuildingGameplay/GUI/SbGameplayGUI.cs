using System;
using System.Collections.Generic;
using System.Globalization;
using EditorAttributes;
using Game.BaseGameplay;
using Game.Global;
using TnieYuPackage.DesignPatterns;
using TnieYuPackage.DictionaryUtilities;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

namespace Game.BuildingGameplay
{
    public class SbGameplayGUI : SingletonBehavior<SbGameplayGUI>
    {
        [Header("Gameplay")] [SerializeField] private SerializableDictionary<ResourceType, Text> resourceNumberTexts;
        [SerializeField] private SerializableDictionary<LimitResourceType, Text> limitResourceNumberTexts;
        [SerializeField] private Text peopleNumberText;
        [SerializeField] private Text maxPeopleNumberText;
        [SerializeField] private Slider healthSlider;

        [Header("Global Properties UI")] [SerializeField]
        private Text skillPointText;

        [SerializeField] private Text currentBuildingNumberText;
        [SerializeField] private Text maxBuildingNumberText;

        [Header("Buttons")] [SerializeField] private Button buildingListBtn;
        [SerializeField] private Button enemyBaseBtn;
        [SerializeField] private Button skillTreeBtn;
        [SerializeField] private Button gamePropertyBtn;

        [Header("Time")] [SerializeField] private Text timerText;

        [Header("Event")] [SerializeField] private Text eventText;
        [SerializeField] private Image eventIcon;
        [SerializeField] private Button eventButton;

        [SerializeField, Required] private Sprite noneEventIcon;
        [SerializeField, Required] private Sprite raisedEventIcon;

        [Header("Army Storage")] [SerializeField]
        private SerializableDictionary<ArmyType, Text> armyNumberTexts;

        private readonly Dictionary<ArmyType, Action<int>> armyNumberEvents = new();

        [Header("Reading")] [ReadOnly] public EnemyBaseEventDisplayUIToolkit enemyBaseEventDisplay;

        private Dictionary<ResourceType, Text> ResourceNumberTexts => resourceNumberTexts.Dictionary;
        private Dictionary<LimitResourceType, Text> LimitResourceNumberTexts => limitResourceNumberTexts.Dictionary;

        private Dictionary<ResourceType, Action<float>> ResourceNumberEvents { get; } = new();
        private Dictionary<LimitResourceType, Action<int>> LimitResourceNumberEvents { get; } = new();

        #region EVENTS

        private void OnEnable()
        {
            RegistryGameplayProperties();
            SbGameplayController.Instance.currentHealth.OnValueChanged += HandleSliderHealthChanged;

            RegistryArmyStorageHandlers();

            buildingListBtn.onClick.AddListener(HandleBuildingListButtonClicked);
            enemyBaseBtn.onClick.AddListener(HandleEnemyBaseButtonClicked);
            skillTreeBtn.onClick.AddListener(HandleSkillTreeButtonClicked);
            gamePropertyBtn.onClick.AddListener(HandleGamePropertyButtonClicked);

            eventButton.onClick.AddListener(HandleEventButtonClicked);
            SbTimeController.Instance.OnEventStarted += HandleEventStarted;
            SbTimeController.Instance.currentTime.OnValueChanged += HandleTimerChangedValue;
        }

        private void RegistryGameplayProperties()
        {
            // 1. Đăng ký và hiển thị Resource
            foreach (var resourceNumberKvp in ResourceNumberTexts)
            {
                Action<float> onResourceDataChanged =
                    changedValue => resourceNumberKvp.Value.text = changedValue.ToString("F1");

                var observableResource = SbGameplayController.GetObservableResource(resourceNumberKvp.Key);
                observableResource.OnValueChanged += onResourceDataChanged;
                ResourceNumberEvents[resourceNumberKvp.Key] = onResourceDataChanged;

                // Cập nhật Text UI lần đầu tiên ngay lúc đăng ký
                resourceNumberKvp.Value.text = observableResource.Value.ToString("F1");
            }

            // 2. Đăng ký và hiển thị Limit Resource
            foreach (var limitResourceNumberKvp in LimitResourceNumberTexts)
            {
                Action<int> onLimitResourceDataChanged =
                    changedValue => limitResourceNumberKvp.Value.text = changedValue.ToString();

                var observableLimitResource =
                    SbGameplayController.Instance.LimitResourceStorage[limitResourceNumberKvp.Key];
                observableLimitResource.OnValueChanged += onLimitResourceDataChanged;
                LimitResourceNumberEvents[limitResourceNumberKvp.Key] = onLimitResourceDataChanged;

                // Cập nhật Text UI lần đầu tiên ngay lúc đăng ký
                limitResourceNumberKvp.Value.text = observableLimitResource.Value.ToString();
            }

            // 3. Đăng ký Villagers
            SbGameplayController.Instance.VillagerData.CurrentVillagers.OnValueChanged += OnCurrentVillagerDataChanged;
            SbGameplayController.Instance.VillagerData.MaxVillagers.OnValueChanged += OnMaxVillagerDataChanged;

            // Cập nhật UI Dân làng lần đầu tiên
            OnCurrentVillagerDataChanged(SbGameplayController.Instance.VillagerData.CurrentVillagers.Value);
            OnMaxVillagerDataChanged(SbGameplayController.Instance.VillagerData.MaxVillagers.Value);

            // 4. Đăng ký Global Properties (Skill Points, Building Numbers)
            if (GamePropertiesRuntime.Instance != null)
            {
                GamePropertiesRuntime.Instance.SkillPoints.OnValueChanged += OnSkillPointChanged;
                GamePropertiesRuntime.Instance.CurrentBuildingNumber.OnValueChanged += OnCurrentBuildingNumberChanged;
                GamePropertiesRuntime.Instance.MaxBuildingNumber.OnValueChanged += OnMaxBuildingNumberChanged;

                // Cập nhật UI lần đầu
                OnSkillPointChanged(GamePropertiesRuntime.Instance.SkillPoints.Value);
                OnCurrentBuildingNumberChanged(GamePropertiesRuntime.Instance.CurrentBuildingNumber.Value);
                OnMaxBuildingNumberChanged(GamePropertiesRuntime.Instance.MaxBuildingNumber.Value);
            }
        }

        private void UnRegistryGameplayProperties()
        {
            foreach (var resourceEventKvp in ResourceNumberEvents)
            {
                SbGameplayController.GetObservableResource(resourceEventKvp.Key).OnValueChanged -=
                    resourceEventKvp.Value;
            }

            foreach (var limitResourceEventKvp in LimitResourceNumberEvents)
            {
                SbGameplayController.Instance.LimitResourceStorage[limitResourceEventKvp.Key].OnValueChanged -=
                    limitResourceEventKvp.Value;
            }

            SbGameplayController.Instance.VillagerData.CurrentVillagers.OnValueChanged -= OnCurrentVillagerDataChanged;
            SbGameplayController.Instance.VillagerData.MaxVillagers.OnValueChanged -= OnMaxVillagerDataChanged;

            if (GamePropertiesRuntime.Instance != null)
            {
                GamePropertiesRuntime.Instance.SkillPoints.OnValueChanged -= OnSkillPointChanged;
                GamePropertiesRuntime.Instance.CurrentBuildingNumber.OnValueChanged -= OnCurrentBuildingNumberChanged;
                GamePropertiesRuntime.Instance.MaxBuildingNumber.OnValueChanged -= OnMaxBuildingNumberChanged;
            }
        }

        private void RegistryArmyStorageHandlers()
        {
            armyNumberEvents.Clear();
            foreach (var armyKvp in armyNumberTexts.Dictionary)
            {
                Action<int> eventHandler = newValue => armyKvp.Value.text = $"{newValue}";
                var armyObserver = SbGameplayController.GetObservableArmy(armyKvp.Key);
                armyObserver.OnValueChanged += eventHandler;
                armyNumberEvents[armyKvp.Key] = eventHandler;

                armyKvp.Value.text = $"{armyObserver.Value}";
            }
        }

        private void UnRegistryArmyStorageHandlers()
        {
            foreach (var armyKvp in armyNumberTexts.Dictionary)
            {
                SbGameplayController.GetObservableArmy(armyKvp.Key).OnValueChanged -= armyNumberEvents[armyKvp.Key];
            }

            armyNumberEvents.Clear();
        }

        private void HandleSliderHealthChanged(int health)
        {
            healthSlider.value = health;
        }

        private void HandleEventButtonClicked()
        {
            // display next event if not exist.
            var eventData = SbTimeController.Instance.CurrentEventConfig;
            EventInfoUIToolkit.Instance.Display(eventData);
        }

        private void HandleTimerChangedValue(float value)
        {
            timerText.text = TimeSpan.FromSeconds(value).ToString(@"mm\:ss");
        }

        private void HandleEventStarted(EventData eventConfig)
        {
            eventIcon.sprite = raisedEventIcon;
            eventText.text = TimeSpan.FromSeconds(SbTimeController.Instance.nextRaisedEventTime).ToString(@"mm\:ss");
        }

        private void HandleEnemyBaseButtonClicked()
        {
            enemyBaseEventDisplay.Open();
        }

        private void HandleBuildingListButtonClicked()
        {
            BuildingTypeListUIToolkit.Instance.Show();
        }

        private void HandleSkillTreeButtonClicked()
        {
            if (GameSkillTreeDisplayController.Instance != null)
            {
                GameSkillTreeDisplayController.Instance.Open();
            }
        }

        private void HandleGamePropertyButtonClicked()
        {
            if (PropertyRuntimeDisplayUIToolkit.Instance != null)
            {
                PropertyRuntimeDisplayUIToolkit.Instance.Open();
            }
        }

        private void OnCurrentVillagerDataChanged(int changedPeopleNumber)
        {
            peopleNumberText.text = $"{changedPeopleNumber}";
        }

        private void OnMaxVillagerDataChanged(int changedMaxPeopleNumber)
        {
            maxPeopleNumberText.text = $"{changedMaxPeopleNumber}";
        }

        private void OnSkillPointChanged(int value)
        {
            if (skillPointText != null) skillPointText.text = $"{value}";
        }

        private void OnCurrentBuildingNumberChanged(int value)
        {
            if (currentBuildingNumberText != null) currentBuildingNumberText.text = $"{value}";
        }

        private void OnMaxBuildingNumberChanged(int value)
        {
            if (maxBuildingNumberText != null) maxBuildingNumberText.text = $"{value}";
        }

        private void OnDisable()
        {
            buildingListBtn.onClick.RemoveListener(HandleBuildingListButtonClicked);
            enemyBaseBtn.onClick.RemoveListener(HandleEnemyBaseButtonClicked);
            skillTreeBtn.onClick.RemoveListener(HandleSkillTreeButtonClicked);
            gamePropertyBtn.onClick.RemoveListener(HandleGamePropertyButtonClicked);
            eventButton.onClick.RemoveListener(HandleEventButtonClicked);

            if (SbTimeController.Instance != null)
            {
                SbTimeController.Instance.OnEventStarted -= HandleEventStarted;
                SbTimeController.Instance.currentTime.OnValueChanged -= HandleTimerChangedValue;
            }

            if (SbGameplayController.Instance != null)
            {
                SbGameplayController.Instance.currentHealth.OnValueChanged -= HandleSliderHealthChanged;
                UnRegistryGameplayProperties();
                UnRegistryArmyStorageHandlers();
            }
        }

        #endregion
    }
}