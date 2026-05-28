using System;
using System.Collections.Generic;
using TnieYuPackage.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.BaseGameplay
{
    [Serializable]
    public struct PropertyIconMapping
    {
        public BaseObjPropertyType propertyType;
        public Sprite icon;
    }

    /// <summary>
    /// Singleton quản lý việc hiển thị UI Thông tin của BaseObject dùng UIToolkit.
    /// Yêu cầu GameObject chứa component này phải có UIDocument.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class BaseObjectInfoDisplay : SingletonDisplayUI<BaseObjectInfoDisplay>
    {
        [Header("Configuration")] [SerializeField]
        private List<PropertyIconMapping> iconConfigs = new();

        [SerializeField] private StyleSheet styleSheet;

        private Dictionary<BaseObjPropertyType, Sprite> iconDictionary = new();

        private UIDocument uiDocument;
        private VisualElement root;
        private VisualElement displayPanel;
        private Label objectNameLabel;
        private VisualElement itemsContainer;

        protected override void Awake()
        {
            base.Awake();

            // Khởi tạo Dictionary map Icon
            foreach (var config in iconConfigs)
            {
                iconDictionary.TryAdd(config.propertyType, config.icon);
            }

            uiDocument = GetComponent<UIDocument>();
            root = uiDocument.rootVisualElement;
            root.pickingMode = PickingMode.Ignore;
            
            if (styleSheet != null)
            {
                root.styleSheets.Add(styleSheet);
            }
            
            SetupUI();
            Hide();
        }

        private void SetupUI()
        {
            // Tạo Main Panel
            displayPanel = new VisualElement();
            displayPanel.AddToClassList("info-display-panel");
            displayPanel.pickingMode = PickingMode.Ignore;

            // Tạo Label Tên Object
            objectNameLabel = new Label();
            objectNameLabel.AddToClassList("info-object-name");
            objectNameLabel.pickingMode = PickingMode.Ignore;

            // Tạo Container chứa các thuộc tính (Icon: Value)
            itemsContainer = new VisualElement();
            itemsContainer.AddToClassList("info-items-container");
            itemsContainer.pickingMode = PickingMode.Ignore;

            // Ghép nối UI Tree
            displayPanel.Add(objectNameLabel);
            displayPanel.Add(itemsContainer);
            root.Add(displayPanel);
        }

        public void Display(IBaseObjectInfoProvider infoProvider)
        {
            if (infoProvider == null) return;

            var objectInfo = infoProvider.GetObjectInfo();

            if (objectInfo == null)
            {
                return;
            }

            displayPanel.style.display = DisplayStyle.Flex;
            objectNameLabel.text = infoProvider.GetObjectName();

            // Xóa các item cũ. Khởi tạo element mới trong UIToolkit rất nhanh và không tạo nhiều rác như uGUI.
            itemsContainer.Clear();

            foreach (var kvp in objectInfo)
            {
                var propertyType = kvp.Key;
                var propertyValue = kvp.Value;

                iconDictionary.TryGetValue(propertyType, out var icon);

                VisualElement itemUI = CreateInfoItem(icon, propertyValue);
                itemsContainer.Add(itemUI);
            }
            
            BlurBackground.Show();
        }

        public override void Hide()
        {
            if (displayPanel != null)
            {
                displayPanel.style.display = DisplayStyle.None;
            }
        }

        // Tạo động Visual Element cho từng thuộc tính
        private VisualElement CreateInfoItem(Sprite icon, string value)
        {
            var container = new VisualElement();
            container.AddToClassList("info-item-container");
            container.pickingMode = PickingMode.Ignore;

            // Nếu có icon thì mới khởi tạo element ảnh
            if (icon != null)
            {
                var iconImage = new Image();
                iconImage.sprite = icon;
                iconImage.AddToClassList("info-item-icon");
                iconImage.pickingMode = PickingMode.Ignore;
                container.Add(iconImage);
            }

            var valueLabel = new Label(value);
            valueLabel.AddToClassList("info-item-value");
            valueLabel.pickingMode = PickingMode.Ignore;
            container.Add(valueLabel);

            return container;
        }
    }
}