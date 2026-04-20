using System;
using EditorAttributes;
using TnieYuPackage.DesignPatterns;
using TnieYuPackage.GlobalExtensions;
using TnieYuPackage.Utils; // Giả định chứa IDisplayGUI
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Global
{
    [RequireComponent(typeof(UIDocument))]
    public class PropertyRuntimeDisplayUIToolkit : SingletonBehavior<PropertyRuntimeDisplayUIToolkit>, IDisplayGUI
    {
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private StyleSheet displayStyleSheet; // Thêm tham chiếu đến file .uss

        private VisualElement root;
        private ScrollView propertyList;

        private void Start()
        {
            if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
            root = uiDocument.rootVisualElement;

            Initialize();
            Hide();
        }

        private void HandleScreenClicked(ClickEvent evt)
        {
            Hide();
            evt.StopPropagation();
        }

        private void Initialize()
        {
            root.Clear();

            // Add StyleSheet vào root để áp dụng các class
            if (displayStyleSheet != null)
            {
                root.styleSheets.Add(displayStyleSheet);
            }

            root.AddClass("root");
            // Xử lý click ra ngoài để đóng UI
            root.RegisterCallback<ClickEvent>(HandleScreenClicked);
            root.pickingMode = PickingMode.Position;

            // Tạo Panel chính
            var panel = new VisualElement();
            panel.AddToClassList("property-panel");

            // Ngăn sự kiện click xuyên qua Panel làm tắt UI
            panel.RegisterCallback<ClickEvent>(evt => evt.StopPropagation());
            root.Add(panel);

            // Tạo Tiêu đề
            var title = new Label("Game Properties");
            title.AddToClassList("property-title");
            panel.Add(title);

            // Tạo danh sách ScrollView
            propertyList = new ScrollView();
            propertyList.AddToClassList("property-list");
            panel.Add(propertyList);
        }

        private void RefreshData()
        {
            propertyList.Clear();
            var props = GamePropertiesRuntime.Instance;
            if (props == null) return;

            // Header - BuildingGameplay
            AddSectionHeaderUI("Building Gameplay Properties");
            AddPropertyUI("General Resource Received Scale", props.GeneralResourceReceivedScale.ToString("F2"));
            AddDictionaryUI("Resource Received Scale", props.ResourceReceivedScaleDict);
            AddDictionaryUI("Unlock Building Type", props.UnlockBuildingTypeDict);
            AddPropertyUI("Max Building Number", props.MaxBuildingNumber.Value.ToString());
            AddPropertyUI("Current Building Number", props.CurrentBuildingNumber.Value.ToString());

            // Header - BaseGameplay
            AddSectionHeaderUI("Base Gameplay Properties");
            AddPropertyUI("Damage Scale", props.DamageScale.ToString("F2"));
            AddPropertyUI("Defense Scale", props.DefenseScale.ToString("F2"));

            // Header - Defense Properties
            AddSectionHeaderUI("Defense Properties");
            AddPropertyUI("Receive Money Scale", props.ReceiveMoneyScale.ToString("F2"));
            AddPropertyUI("Refund Scale", props.RefundScale.ToString("F2"));
            AddDictionaryUI("Tower Type Damage Scale", props.TowerTypeDamageScaleDict);
            AddDictionaryUI("Unlock Tower Level", props.UnlockTowerLevelDict);
            AddDictionaryUI("Unlock Tower Type", props.UnlockTowerTypeDict);

            // Header - Attack Properties
            AddSectionHeaderUI("Attack Properties");
            AddPropertyUI("Max Entity Per Wave", props.MaxEntityPerWave.ToString());
            AddDictionaryUI("Army Damage Scale", props.ArmyDamageScaleDict);
            AddDictionaryUI("Army Defense Scale", props.ArmyDefenseScaleDict);
            AddDictionaryUI("Army Health Scale", props.ArmyHealthScaleDict);
            AddDictionaryUI("Army Speed Scale", props.ArmySpeedScaleDict);

            // Global
            AddSectionHeaderUI("Global");
            AddPropertyUI("Skill Points", props.SkillPoints.Value.ToString());
        }

        private void AddDictionaryUI<TKey, TValue>(string title,
            System.Collections.Generic.Dictionary<TKey, TValue> dict)
        {
            // Thêm Header nhỏ cho Dictionary
            AddPropertyUI(title, "");

            if (dict == null || dict.Count == 0)
            {
                var emptyContainer = new VisualElement();
                emptyContainer.AddToClassList("dict-item-container");
                var emptyLabel = new Label("- (Empty)");
                emptyLabel.AddToClassList("dict-item-name");
                emptyContainer.Add(emptyLabel);
                propertyList.Add(emptyContainer);
                return;
            }

            foreach (var kvp in dict)
            {
                var container = new VisualElement();
                container.AddToClassList("dict-item-container");

                var nameLabel = new Label($"- {kvp.Key.ToString()}");
                nameLabel.AddToClassList("dict-item-name");

                var valueLabel = new Label(kvp.Value?.ToString() ?? "null");
                valueLabel.AddToClassList("dict-item-value");

                container.Add(nameLabel);
                container.Add(valueLabel);
                propertyList.Add(container);
            }
        }

        private void AddSectionHeaderUI(string headerName)
        {
            var headerLabel = new Label(headerName);
            headerLabel.AddToClassList("section-header");
            propertyList.Add(headerLabel);
        }

        private void AddPropertyUI(string propName, string propValue)
        {
            var container = new VisualElement();
            container.AddToClassList("property-container");

            var nameLabel = new Label(propName);
            nameLabel.AddToClassList("property-name");

            var valueLabel = new Label(propValue);
            valueLabel.AddToClassList("property-value");

            container.Add(nameLabel);
            container.Add(valueLabel);
            propertyList.Add(container);
        }

        [Button]
        public void Open()
        {
            RefreshData(); // Mỗi khi mở sẽ làm mới dữ liệu
            root.style.display = DisplayStyle.Flex;
        }

        [Button]
        public void Hide()
        {
            root.style.display = DisplayStyle.None;
        }
    }
}