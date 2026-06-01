using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Global.GameController;
using Cysharp.Threading.Tasks;
using EditorAttributes;
using Game.BaseGameplay;
using Game.Global;
using Gameplay.Global;
using KBCore.Refs;
using Reflex.Attributes;
using TnieYuPackage.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.BuildingGameplay
{
    [RequireComponent(typeof(UIDocument))]
    public class ArmySelectToAttackUIToolkit : SingletonDisplayUI<ArmySelectToAttackUIToolkit>
    {
        [Inject] private GameplayTransition transition;

        [SerializeField, Self] private UIDocument uiDocument;
        [SerializeField] private StyleSheet styleSheet; // Kéo file ArmySelectUI.uss vào đây

        private VisualElement root;
        private ScrollView armyListContainer;
        private Button confirmButton;
        private Button cancelButton;

        private EventData currentEvent;
        private Dictionary<ArmyType, int> selectedArmies = new Dictionary<ArmyType, int>();
        private Dictionary<ArmyType, TextField> countInputFields = new Dictionary<ArmyType, TextField>();

        private bool isStopTimer = false;
        private int preFrameRate;
        private float preTimeScale;

        private async void Start()
        {
            await UniTask.Yield();
            root = uiDocument.rootVisualElement;
            InitializeUI();
            Hide();
        }

        private void InitializeUI()
        {
            root.Clear();

            // Nạp file CSS (USS)
            if (styleSheet != null)
            {
                root.styleSheets.Add(styleSheet);
            }

            // 1. Root Container (Nền đen mờ phủ toàn màn hình)
            var rootContainer = new VisualElement();
            rootContainer.AddToClassList("root-container");
            root.Add(rootContainer);

            // 2. Panel Container (Khung popup)
            var panelContainer = new VisualElement();
            panelContainer.AddToClassList("panel-container");
            rootContainer.Add(panelContainer);

            // 3. Header
            var headerSection = new VisualElement();
            headerSection.AddToClassList("header-section");

            var headerTitle = new Label("SELECT ARMY FOR ATTACK");
            headerTitle.AddToClassList("header-title");

            headerSection.Add(headerTitle);
            panelContainer.Add(headerSection);

            // 4. ScrollView (Danh sách quân)
            armyListContainer = new ScrollView();
            armyListContainer.AddToClassList("list-section");
            panelContainer.Add(armyListContainer);

            // 5. Footer (Các nút bấm)
            var footerSection = new VisualElement();
            footerSection.AddToClassList("footer-section");

            cancelButton = new Button { text = "CANCEL" };
            cancelButton.AddToClassList("btn");
            cancelButton.AddToClassList("btn-secondary");
            cancelButton.clicked += OnCancelClicked;

            confirmButton = new Button { text = "ATTACK" };
            confirmButton.AddToClassList("btn");
            confirmButton.AddToClassList("btn-primary");
            confirmButton.clicked += OnConfirmClicked;

            footerSection.Add(cancelButton);
            footerSection.Add(confirmButton);
            panelContainer.Add(footerSection);

            // Xoá nền click xuyên để tránh click nhầm vào game
            rootContainer.pickingMode = PickingMode.Position;
        }

        public void Display(EventData eventData, bool stopTimer = false)
        {
            currentEvent = eventData;
            selectedArmies.Clear();
            countInputFields.Clear();

            PopulateArmyList();
            Open(stopTimer);
        }

        private void PopulateArmyList()
        {
            armyListContainer.Clear();

            // Duyệt qua tất cả loại quân và tạo dòng bằng code
            foreach (ArmyType armyType in Enum.GetValues(typeof(ArmyType)))
            {
                int totalAvailable = SbGameplayController.GetObservableArmy(armyType).Value;
                selectedArmies[armyType] = 0; // Khởi tạo lượng chọn ban đầu = 0

                var row = CreateRow(armyType, totalAvailable);
                armyListContainer.Add(row);
            }
        }

        private VisualElement CreateRow(ArmyType armyType, int maxAvailable)
        {
            var row = new VisualElement();
            row.AddToClassList("army-row");

            // Tên và số lượng đang có
            var nameLabel = new Label($"{armyType} (Có: {maxAvailable})");
            nameLabel.AddToClassList("army-name-lbl");

            // Cụm các nút điều khiển (-, input_field, +)
            var controls = new VisualElement();
            controls.AddToClassList("army-controls");

            var minusBtn = new Button { text = "-" };

            var countInput = new TextField { value = "0" };
            countInput.AddToClassList("army-count-input");
            // Đăng ký sự kiện thay đổi giá trị cho TextField
            countInput.RegisterValueChangedCallback(evt =>
                OnInputValueChanged(armyType, evt.newValue, maxAvailable, countInput));

            countInputFields[armyType] = countInput; // Lưu lại reference để update

            var plusBtn = new Button { text = "+" };
            var allBtn = new Button { text = "Max" };

            // Đăng ký sự kiện cho các nút
            minusBtn.clicked += () => ModifyArmyCount(armyType, -1, maxAvailable);
            plusBtn.clicked += () => ModifyArmyCount(armyType, 1, maxAvailable);
            allBtn.clicked += () => ModifyArmyCount(armyType, maxAvailable, maxAvailable);

            // Gom lại vào hierarchy
            controls.Add(minusBtn);
            controls.Add(countInput);
            controls.Add(plusBtn);
            controls.Add(allBtn);

            row.Add(nameLabel);
            row.Add(controls);

            return row;
        }

        private void OnInputValueChanged(ArmyType type, string newValue, int maxAvailable, TextField inputField)
        {
            // Parse chuỗi nhập vào thành số nguyên
            if (int.TryParse(newValue, out int parsedValue))
            {
                // Validate giá trị
                int clampedValue = Mathf.Clamp(parsedValue, 0, maxAvailable);
                selectedArmies[type] = clampedValue;

                // Nếu giá trị nhập vào bị giới hạn lại (ví dụ nhập 100 nhưng max là 50), cập nhật lại TextField
                if (parsedValue != clampedValue)
                {
                    inputField.SetValueWithoutNotify(clampedValue.ToString());
                }
            }
            else
            {
                // Nếu nhập vào không phải là số (ví dụ chữ cái), reset về giá trị hợp lệ trước đó
                inputField.SetValueWithoutNotify(selectedArmies[type].ToString());
            }
        }

        private void ModifyArmyCount(ArmyType type, int change, int maxAvailable)
        {
            int current = selectedArmies[type];

            if (change == maxAvailable)
            {
                selectedArmies[type] = maxAvailable;
            }
            else
            {
                selectedArmies[type] = Mathf.Clamp(current + change, 0, maxAvailable);
            }

            // Cập nhật lại giá trị trên TextField (không gọi lại event OnValueChanged để tránh lặp vô tận)
            countInputFields[type].SetValueWithoutNotify(selectedArmies[type].ToString());
        }

        private void OnConfirmClicked()
        {
            int totalSelected = 0;
            foreach (var count in selectedArmies.Values) totalSelected += count;

            if (totalSelected <= 0)
            {
                Debug.LogWarning("Chưa chọn quân đội nào để đi Attack!");
                return;
            }

            Hide();

            var armiesToTake = SbGameplayController.Instance.TakeArmyForAttack(selectedArmies);
            transition.LoadBaseGameplayWithEvent(currentEvent, armiesToTake).Forget();
        }

        private void OnCancelClicked()
        {
            Hide();
            EventInfoUIToolkit.Instance.Display(currentEvent, isStopTimer);
        }

        [Button]
        private void Open(bool stopTimer)
        {
            root.style.display = DisplayStyle.Flex;
            BlurBackground.Show();
            isStopTimer = stopTimer;
            if (!isStopTimer) return;

            preFrameRate = GameTimeController.TargetFrameRate;
            preTimeScale = GameTimeController.TimeScale;
            GameTimeController.SetGameStop();
        }

        [Button]
        public override void Hide()
        {
            root.style.display = DisplayStyle.None;

            if (!isStopTimer) return;
            GameTimeController.SetFrameRate(preFrameRate);
            GameTimeController.SetTimeScale(preTimeScale);
            isStopTimer = false;
        }
    }
}