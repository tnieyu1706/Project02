using System;
using Cysharp.Threading.Tasks;
using EditorAttributes;
using Game.BaseGameplay;
using Gameplay.Global;
using KBCore.Refs;
using TnieYuPackage.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Scripts.Gameplay.Global.UI.WorldMap
{
    /// <summary>
    /// Singleton quản lý việc hiển thị UI Toolkit cho Popup thông tin Level
    /// Tự động khởi tạo giao diện bằng code UI Toolkit.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class LevelMapInfoUIManager : SingletonDisplayUI<LevelMapInfoUIManager>
    {
        [SerializeField, Self] private UIDocument uiDocument;
        [SerializeField] private StyleSheet styleSheet; // Gắn tệp LevelMapInfoStyle.uss vào đây ở Inspector

        private VisualElement _root;
        
        // Các thành phần UI được build bằng code
        private VisualElement _popupContainer;
        private Label _titleLabel;
        private Label _scoreLabel;
        private Label _statusLabel;
        private Button _playButton;

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
            
            // Đặt PickingMode của root thành Ignore để không chặn sự kiện click xuống BlurBackground (UGUI) bên dưới
            _root.pickingMode = PickingMode.Ignore;

            // Nhúng StyleSheet vào Root
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

            // 3. Tiêu đề (Title Label)
            _titleLabel = new Label();
            _titleLabel.AddToClassList("title-label");

            // 4. Thông tin điểm số (Score Label)
            _scoreLabel = new Label();
            _scoreLabel.AddToClassList("score-label");

            // 5. Trạng thái mở khóa (Status Label)
            _statusLabel = new Label();
            _statusLabel.AddToClassList("status-label");

            // 6. Nút Play (Play Button)
            _playButton = new Button { text = "PLAY" };
            _playButton.AddToClassList("play-button");
            _playButton.clicked += OnPlayClicked;

            // Lắp ráp hệ thống Hierarchy UI
            mainPanel.Add(_titleLabel);
            mainPanel.Add(_scoreLabel);
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

            // Cập nhật thông tin UI
            _titleLabel.text = level != null ? level.name : "Unknown Level";
            _scoreLabel.text = $"High Score: {data.score}";
            
            _statusLabel.text = data.isUnlocked ? "Status: Unlocked" : "Status: Locked";
            _statusLabel.style.color = data.isUnlocked ? new StyleColor(Color.green) : new StyleColor(Color.red);

            // Chỉ cho phép bấm Play nếu level đã được unlock
            _playButton.SetEnabled(data.isUnlocked);

            Show();
        }

        [Button]
        public void Show()
        {
            _root.style.display = DisplayStyle.Flex;
            BlurBackground.Show(); // Kích hoạt hiệu ứng Background được cung cấp từ SingletonDisplayUI
        }

        [Button]
        public override void Hide()
        {
            if (_root != null)
            {
                _root.style.display = DisplayStyle.None;
            }
        }

        /// <summary>
        /// Xử lý khi người chơi bấm nút Play
        /// </summary>
        private void OnPlayClicked()
        {
            if (_currentLevelData == null || _currentLevelInfo == null) return;

            // Ẩn UI trước khi chuyển scene
            Hide();
            BlurBackground.CloseManual(); // Tắt luôn màn hình mờ khi bắt đầu Transition

            // Gọi logic transition load màn
            GameplayTransition.CreateBuildingGameplay(_currentLevelInfo, _currentLevelData).Forget();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy(); // Cực kỳ quan trọng để un-register BlurBackground 

            // Xóa đăng ký sự kiện để tránh memory leak
            if (_playButton != null) _playButton.clicked -= OnPlayClicked;
        }
    }
}