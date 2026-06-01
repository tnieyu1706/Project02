using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EditorAttributes;
using Game.BaseGameplay;
using Game.Global;
using KBCore.Refs;
using Reflex.Attributes;
using TnieYuPackage.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.WaveAttack
{
    [RequireComponent(typeof(UIDocument))]
    public class WaGameplayPanelUIToolkit : SingletonDisplayUI<WaGameplayPanelUIToolkit>
    {
        [SerializeField, Self] private UIDocument uiDocument;
        [SerializeField] private StyleSheet styleSheet;

        private VisualElement root;

        // UI Elements - Left Side (Army Setup)
        private ScrollView armyListContainer;
        private Dictionary<ArmyType, TextField> armyInputFields = new();
        private Dictionary<ArmyType, Label> globalCountLabels = new();

        // UI Elements - Right Side (Rules)
        private Label waveTitleLabel;
        private Label maxBaseDamageLabel;
        private Label maxEntityLabel;
        private Label currentBaseDamageLabel;
        private Label currentEntityLabel;

        // Action trackers for cleanup
        private readonly List<Action> cleanupActions = new();

        private async void Start()
        {
            await UniTask.Yield();
            root = uiDocument.rootVisualElement;
            InitializeUI();

            // Đăng ký event sau khi UI đã khởi tạo xong
            RegisterEvents();

            // Mặc định ẩn hoặc hiện tùy logic game của bạn (Ở đây tôi gọi Hide mặc định)
            Hide();
        }

        private void InitializeUI()
        {
            root.Clear();
            if (styleSheet != null) root.styleSheets.Add(styleSheet);
            root.pickingMode = PickingMode.Ignore;

            var rootContainer = new VisualElement();
            rootContainer.AddToClassList("root-container");
            root.Add(rootContainer);
            rootContainer.pickingMode = PickingMode.Ignore;

            var panelContainer = new VisualElement();
            panelContainer.AddToClassList("panel-container");
            rootContainer.Add(panelContainer);

            // Nút đóng Panel
            var closeBtn = new Button { text = "X" };
            closeBtn.AddToClassList("close-btn");
            closeBtn.clicked += Hide;
            panelContainer.Add(closeBtn);

            // ================= LEFT PANEL (70%) =================
            var leftPanel = new VisualElement();
            leftPanel.AddToClassList("left-panel");

            var leftTitle = new Label("ARMY DEPLOYMENT SETUP");
            leftTitle.AddToClassList("panel-title");
            leftPanel.Add(leftTitle);

            armyListContainer = new ScrollView();
            armyListContainer.AddToClassList("army-list");
            leftPanel.Add(armyListContainer);

            // Render Army Rows
            foreach (ArmyType armyType in Enum.GetValues(typeof(ArmyType)))
            {
                var row = CreateArmyRow(armyType);
                armyListContainer.Add(row);
            }

            // ================= RIGHT PANEL (30%) =================
            var rightPanel = new VisualElement();
            rightPanel.AddToClassList("right-panel");

            waveTitleLabel = new Label("WAVE RULES");
            waveTitleLabel.AddToClassList("panel-title");
            waveTitleLabel.AddToClassList("rule-title");
            rightPanel.Add(waveTitleLabel);

            var ruleContainer = new VisualElement();
            ruleContainer.AddToClassList("rule-container");

            maxBaseDamageLabel = CreateRuleRow(ruleContainer, "Max Base Damage:", "0");
            currentBaseDamageLabel = CreateRuleRow(ruleContainer, "Current Damage:", "0");

            var separator = new VisualElement();
            separator.AddToClassList("separator");
            ruleContainer.Add(separator);

            maxEntityLabel = CreateRuleRow(ruleContainer, "Max Deployment:", "0");
            currentEntityLabel = CreateRuleRow(ruleContainer, "Current Deployed:", "0");

            rightPanel.Add(ruleContainer);

            // Add panels to container
            panelContainer.Add(leftPanel);
            panelContainer.Add(rightPanel);

            rootContainer.pickingMode = PickingMode.Position;
        }

        private VisualElement CreateArmyRow(ArmyType armyType)
        {
            var row = new VisualElement();
            row.AddToClassList("army-row");

            var nameContainer = new VisualElement();
            nameContainer.AddToClassList("army-name-container");

            var nameLabel = new Label(armyType.ToString());
            nameLabel.AddToClassList("army-name-lbl");

            var globalLabel = new Label("Global: 0");
            globalLabel.AddToClassList("army-global-lbl");
            globalCountLabels[armyType] = globalLabel;

            nameContainer.Add(nameLabel);
            nameContainer.Add(globalLabel);

            var controls = new VisualElement();
            controls.AddToClassList("army-controls");

            var minusBtn = new Button { text = "-" };
            var countInput = new TextField { value = "0" };
            countInput.AddToClassList("army-count-input");
            armyInputFields[armyType] = countInput;

            var plusBtn = new Button { text = "+" };
            var maxBtn = new Button { text = "Max" };

            // Logic tính toán an toàn dựa vào rule
            minusBtn.clicked += () => ModifyArmyCount(armyType, -1);
            plusBtn.clicked += () => ModifyArmyCount(armyType, 1);
            maxBtn.clicked += () => ApplyMaxArmy(armyType);
            countInput.RegisterValueChangedCallback(evt => OnInputValueChanged(armyType, evt.newValue, countInput));

            controls.Add(minusBtn);
            controls.Add(countInput);
            controls.Add(plusBtn);
            controls.Add(maxBtn);

            row.Add(nameContainer);
            row.Add(controls);

            return row;
        }

        private Label CreateRuleRow(VisualElement parent, string title, string initialValue)
        {
            var row = new VisualElement();
            row.AddToClassList("rule-row");

            var titleLbl = new Label(title);
            titleLbl.AddToClassList("rule-label-title");

            var valueLbl = new Label(initialValue);
            valueLbl.AddToClassList("rule-label-value");

            row.Add(titleLbl);
            row.Add(valueLbl);
            parent.Add(row);

            return valueLbl;
        }

        #region EVENT REGISTRATIONS

        private void RegisterEvents()
        {
            if (!WaGameplayController.HasInstance || !BaseGameplayController.HasInstance) return;

            var waCtrl = WaGameplayController.Instance;
            var baseCtrl = BaseGameplayController.Instance;

            // Đăng ký Wave Rules
            Action<int> maxDmgChange = val => maxBaseDamageLabel.text = val.ToString();
            Action<int> curDmgChange = val => currentBaseDamageLabel.text = val.ToString();
            Action<int> maxEntChange = val => maxEntityLabel.text = val.ToString();
            Action<int> curEntChange = val => currentEntityLabel.text = val.ToString();
            Action<int> waveChange = val => waveTitleLabel.text = $"WAVE {val + 1} RULES";

            waCtrl.maxBaseDamageOutput.OnValueChanged += maxDmgChange;
            waCtrl.currentBaseDamageOutput.OnValueChanged += curDmgChange;
            waCtrl.maxEntityDeploymentCount.OnValueChanged += maxEntChange;
            waCtrl.currentEntityDeploymentCount.OnValueChanged += curEntChange;
            baseCtrl.currentWaveIndex.OnValueChanged += waveChange;

            // Khởi tạo text hiện tại
            maxDmgChange(waCtrl.maxBaseDamageOutput.Value);
            curDmgChange(waCtrl.currentBaseDamageOutput.Value);
            maxEntChange(waCtrl.maxEntityDeploymentCount.Value);
            curEntChange(waCtrl.currentEntityDeploymentCount.Value);
            waveChange(baseCtrl.currentWaveIndex.Value);

            cleanupActions.Add(() =>
            {
                if (WaGameplayController.HasInstance)
                {
                    waCtrl.maxBaseDamageOutput.OnValueChanged -= maxDmgChange;
                    waCtrl.currentBaseDamageOutput.OnValueChanged -= curDmgChange;
                    waCtrl.maxEntityDeploymentCount.OnValueChanged -= maxEntChange;
                    waCtrl.currentEntityDeploymentCount.OnValueChanged -= curEntChange;
                }

                if (BaseGameplayController.HasInstance)
                {
                    baseCtrl.currentWaveIndex.OnValueChanged -= waveChange;
                }
            });

            // Đăng ký Army Observables
            foreach (ArmyType armyType in Enum.GetValues(typeof(ArmyType)))
            {
                var localType = armyType; // Cache cho closure

                // Track Global Army
                Action<int> globalChange = val => globalCountLabels[localType].text = $"Global: {val}";
                waCtrl.GlobalStorage[localType].OnValueChanged += globalChange;
                globalChange(waCtrl.GlobalStorage[localType].Value);

                // Track Wave Army
                Action<int> waveChangeAction = val => armyInputFields[localType].SetValueWithoutNotify(val.ToString());
                waCtrl.WaveStorage[localType].OnValueChanged += waveChangeAction;
                waveChangeAction(waCtrl.WaveStorage[localType].Value);

                cleanupActions.Add(() =>
                {
                    if (WaGameplayController.HasInstance)
                    {
                        waCtrl.GlobalStorage[localType].OnValueChanged -= globalChange;
                        waCtrl.WaveStorage[localType].OnValueChanged -= waveChangeAction;
                    }
                });
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            foreach (var cleanup in cleanupActions)
            {
                cleanup?.Invoke();
            }

            cleanupActions.Clear();
        }

        #endregion

        #region LOGIC HANDLERS

        private void OnInputValueChanged(ArmyType type, string newValue, TextField inputField)
        {
            if (int.TryParse(newValue, out int parsedValue))
            {
                int currentInWave = WaGameplayController.Instance.WaveStorage[type].Value;
                int diff = parsedValue - currentInWave;
                SafeAddArmyToWave(type, diff);
            }

            // Dù đúng hay sai format, cuối cùng vẫn ép UI hiển thị lại giá trị chuẩn từ Logic
            inputField.SetValueWithoutNotify(WaGameplayController.Instance.WaveStorage[type].Value.ToString());
        }

        private void ModifyArmyCount(ArmyType type, int change)
        {
            SafeAddArmyToWave(type, change);
        }

        private void ApplyMaxArmy(ArmyType type)
        {
            var waCtrl = WaGameplayController.Instance;
            int globalAvail = waCtrl.GlobalStorage[type].Value;
            int currentDeployed = waCtrl.currentEntityDeploymentCount.Value;
            int maxDeployed = waCtrl.maxEntityDeploymentCount.Value;

            // Tính toán khoảng trống tối đa còn có thể nhét thêm vào wave
            int spaceLeft = Mathf.Max(0, maxDeployed - currentDeployed);

            // Số lượng an toàn = Min(Có sẵn trong Global, Khoảng trống Wave Rule)
            int safeAmountToAdd = Mathf.Min(globalAvail, spaceLeft);

            if (safeAmountToAdd > 0)
            {
                waCtrl.AddArmyForWave(type, safeAmountToAdd);
            }
        }

        private void SafeAddArmyToWave(ArmyType type, int amountDiff)
        {
            if (amountDiff == 0) return;

            var waCtrl = WaGameplayController.Instance;

            if (amountDiff > 0)
            {
                int globalAvail = waCtrl.GlobalStorage[type].Value;
                int currentDeployed = waCtrl.currentEntityDeploymentCount.Value;
                int maxDeployed = waCtrl.maxEntityDeploymentCount.Value;
                int spaceLeft = Mathf.Max(0, maxDeployed - currentDeployed);

                // Giới hạn số lượng muốn Add không được vượt qua rule và số lượng global
                amountDiff = Mathf.Min(amountDiff, globalAvail);
                amountDiff = Mathf.Min(amountDiff, spaceLeft);
            }
            else
            {
                // Nếu là trừ quân (rút khỏi wave)
                int currentInWave = waCtrl.WaveStorage[type].Value;
                // Không được trừ lố số lượng đang có trong wave
                amountDiff = Mathf.Max(amountDiff, -currentInWave);
            }

            if (amountDiff != 0)
            {
                waCtrl.AddArmyForWave(type, amountDiff);
            }
        }

        #endregion

        [Button]
        public void ShowPanel()
        {
            root.style.display = DisplayStyle.Flex;
            BlurBackground.Show();
        }

        [Button]
        public override void Hide()
        {
            root.style.display = DisplayStyle.None;
        }
    }
}