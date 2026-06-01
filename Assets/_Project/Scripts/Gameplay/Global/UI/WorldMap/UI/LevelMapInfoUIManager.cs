using System;
using Cysharp.Threading.Tasks;
using EditorAttributes;
using Game.BaseGameplay;
using Gameplay.Global;
using KBCore.Refs;
using Reflex.Attributes;
using TnieYuPackage.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Scripts.Gameplay.Global.UI.WorldMap
{
    /// <summary>
    /// Singleton quản lý việc hiển thị UI Toolkit cho Popup thông tin Level
    /// Đã được tùy chỉnh để sử dụng Sprite cho Panel và Slider sao (0-3).
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class LevelMapInfoUIManager : SingletonDisplayUI<LevelMapInfoUIManager>
    {
        [Inject] GameplayTransition transition;

        [SerializeField, Self] private UIDocument uiDocument;
        [SerializeField] private StyleSheet styleSheet;

        [Header("UI Sprites & Assets")]
        [Tooltip("Kéo thả Assets/_Project/Sprites/GUI/panel_2.png vào đây")]
        [SerializeField]
        private Sprite panelBackgroundSprite;

        [Tooltip("Kéo thả Assets/_Project/Sprites/GUI/triple_star.png vào đây")] [SerializeField]
        private Sprite tripleStarSprite;

        private VisualElement _root;

        // Các thành phần UI được build bằng code
        private VisualElement _popupContainer;
        private Label _titleLabel;
        private Label _statusLabel;
        private Button _playButton;

        // Các thành phần cho Star Slider
        private VisualElement _starMask; // Khung dùng để che/cắt hình ảnh sao
        private VisualElement _starImage; // Chứa sprite 3 sao

        // Lưu trữ data hiện tại đang được chọn
        private LevelData _currentLevelData;
        private BuildingGameplayLevel _currentLevelInfo;

        private async void Start()
        {
            await UniTask.Yield();

            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();

            _root = uiDocument.rootVisualElement;

            if (_root == null)
            {
                Debug.LogError("UIDocument Root is null!");
                return;
            }

            InitializeUI();
            Hide();
        }

        /// <summary>
        /// Khởi tạo toàn bộ Element của giao diện thông qua code
        /// </summary>
        private void InitializeUI()
        {
            _root.Clear();
            _root.pickingMode = PickingMode.Ignore;

            if (styleSheet != null)
            {
                _root.styleSheets.Add(styleSheet);
            }

            // 1. Tạo Overlay Container
            _popupContainer = new VisualElement();
            _popupContainer.AddToClassList("popup-container");
            _popupContainer.pickingMode = PickingMode.Ignore;

            // 2. Khung popup chính (Main Panel)
            var mainPanel = new VisualElement();
            mainPanel.AddToClassList("main-panel");

            // Áp dụng Sprite cho Panel nền
            if (panelBackgroundSprite != null)
            {
                mainPanel.style.backgroundImage = new StyleBackground(panelBackgroundSprite);
                mainPanel.style.backgroundColor = new StyleColor(Color.clear); // Tắt màu nền mặc định
                mainPanel.style.borderTopWidth = 0; // Tắt viền nếu xài sprite
                mainPanel.style.borderBottomWidth = 0;
                mainPanel.style.borderLeftWidth = 0;
                mainPanel.style.borderRightWidth = 0;
            }

            // 3. Tiêu đề (Title Label)
            _titleLabel = new Label();
            _titleLabel.AddToClassList("title-label");

            // 4. Hệ thống Star Slider (Thay thế cho Score Label)
            var starSliderContainer = new VisualElement();
            starSliderContainer.AddToClassList("star-slider-container");

            _starMask = new VisualElement();
            _starMask.AddToClassList("star-slider-mask");

            _starImage = new VisualElement();
            _starImage.AddToClassList("star-image");

            // Áp dụng Sprite 3 sao
            if (tripleStarSprite != null)
            {
                _starImage.style.backgroundImage = new StyleBackground(tripleStarSprite);
            }

            // Lắp ráp hệ thống Sao
            _starMask.Add(_starImage);
            starSliderContainer.Add(_starMask);

            // 5. Trạng thái mở khóa (Status Label)
            _statusLabel = new Label();
            _statusLabel.AddToClassList("status-label");

            // 6. Nút Play (Play Button)
            _playButton = new Button { text = "PLAY" };
            _playButton.AddToClassList("play-button");
            _playButton.clicked += OnPlayClicked;

            // Lắp ráp hệ thống Hierarchy UI
            mainPanel.Add(_titleLabel);
            mainPanel.Add(starSliderContainer);
            mainPanel.Add(_statusLabel);
            mainPanel.Add(_playButton);

            _popupContainer.Add(mainPanel);
            _root.Add(_popupContainer);
        }

        /// <summary>
        /// Gắn dữ liệu và hiển thị thông tin level lên UI
        /// </summary>
        public void ShowInfo(LevelData data, BuildingGameplayLevel level)
        {
            _currentLevelData = data;
            _currentLevelInfo = level;

            // Cập nhật Tiêu đề
            _titleLabel.text = level != null ? level.name : "Unknown Level";

            // Cập nhật Star Slider (Int constraint từ 0 -> 3)
            int starCount = Mathf.Clamp(data.score, 0, 3);
            float fillPercentage = (starCount / 3f) * 100f;
            _starMask.style.width = Length.Percent(fillPercentage); // Tính % chiều rộng để lộ sao tương ứng

            // Cập nhật Trạng thái
            _statusLabel.text = data.isUnlocked ? "Unlocked" : "Locked";
            _statusLabel.style.color = data.isUnlocked
                ? new StyleColor(new Color(0.2f, 0.8f, 0.2f))
                : new StyleColor(new Color(0.8f, 0.2f, 0.2f));

            // Nút Play
            _playButton.SetEnabled(data.isUnlocked);

            Show();
        }

        [Button]
        public void Show()
        {
            _root.style.display = DisplayStyle.Flex;
            BlurBackground.Show();
        }

        [Button]
        public override void Hide()
        {
            if (_root != null)
            {
                _root.style.display = DisplayStyle.None;
            }
        }

        private void OnPlayClicked()
        {
            if (_currentLevelData == null || _currentLevelInfo == null) return;

            Hide();
            BlurBackground.CloseManual();
            transition.CreateBuildingGameplay(_currentLevelInfo, _currentLevelData).Forget();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_playButton != null) _playButton.clicked -= OnPlayClicked;
        }
    }
}