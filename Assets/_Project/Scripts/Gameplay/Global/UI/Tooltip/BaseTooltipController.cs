using Cysharp.Threading.Tasks;
using KBCore.Refs;
using TnieYuPackage.DesignPatterns;
using TnieYuPackage.GlobalExtensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Scripts.Gameplay.Global.Tooltip
{
    [RequireComponent(typeof(UIDocument))]
    public abstract class BaseTooltipController<TSingleton> : SingletonBehavior<TSingleton>
        where TSingleton : BaseTooltipController<TSingleton>
    {
        [SerializeField, Self] private UIDocument uiDocument;
        [SerializeField] private StyleSheet styleSheet;

        protected VisualElement Root;
        protected VisualElement TooltipContainer;

        private Vector2 _currentMousePos;

        async void Start()
        {
            await UniTask.Yield();
            Root = uiDocument.rootVisualElement;

            SetupUI();
            Hide();
        }

        protected void RefreshPosition(Vector2 screenPos)
        {
            _currentMousePos = screenPos;

            // Đặt tạm vị trí dựa theo chuột (Lúc này Tooltip đang tàng hình do Opacity = 0)
            TooltipContainer.style.left = screenPos.x + 15;
            TooltipContainer.style.top = screenPos.y + 15;

            // Đăng ký sự kiện lắng nghe ngay khi UI Toolkit tính toán xong Width/Height thực tế
            TooltipContainer.RegisterCallback<GeometryChangedEvent>(OnTooltipGeometryChanged);
        }

        private void OnTooltipGeometryChanged(GeometryChangedEvent evt)
        {
            // 1. Huỷ đăng ký ngay lập tức để tránh event lặp vô tận khi ta thay đổi vị trí bên dưới
            TooltipContainer.UnregisterCallback<GeometryChangedEvent>(OnTooltipGeometryChanged);

            // 2. Lấy kích thước thực tế sau khi Layout đã dàn trang xong
            float tooltipWidth = TooltipContainer.resolvedStyle.width;
            float tooltipHeight = TooltipContainer.resolvedStyle.height;

            // Lấy kích thước màn hình UI
            float screenWidth = Root.resolvedStyle.width;
            float screenHeight = Root.resolvedStyle.height;

            float offset = 15f;
            float finalLeft = _currentMousePos.x + offset;
            float finalTop = _currentMousePos.y + offset;

            // 3. THUẬT TOÁN CHỐNG TRÀN VIỀN
            // Nếu Tooltip tràn mép Phải màn hình -> Lật sang mép Trái của chuột
            if (finalLeft + tooltipWidth > screenWidth)
            {
                finalLeft = _currentMousePos.x - tooltipWidth - offset;
            }

            // Nếu Tooltip tràn mép Dưới màn hình -> Đẩy lên phía Trên của chuột
            if (finalTop + tooltipHeight > screenHeight)
            {
                finalTop = _currentMousePos.y - tooltipHeight - offset;
            }

            // 4. Áp dụng toạ độ an toàn mới
            TooltipContainer.style.left = finalLeft;
            TooltipContainer.style.top = finalTop;

            // 5. Hiện Tooltip lên mượt mà (Hết tàng hình)
            TooltipContainer.style.opacity = 1f;
        }

        protected virtual void SetupUI()
        {
            Root.Clear();
            Root.styleSheets.Add(styleSheet);
            Root.AddClass("root");

            // Tạo container với position absolute
            TooltipContainer = Root.CreateChild("tooltip-container");
            TooltipContainer.style.position = Position.Absolute;
            TooltipContainer.pickingMode = PickingMode.Ignore;
        }

        protected virtual void Show()
        {
            Root.style.display = DisplayStyle.Flex;

            // Tạm thời làm Tooltip trở nên "tàng hình" (trong suốt)
            // Việc này giúp tránh lỗi nháy hình (flicker) trong 1 frame đợi UI Toolkit tính toán kích thước
            TooltipContainer.style.opacity = 0f;
        }

        public virtual void Hide()
        {
            Root.style.display = DisplayStyle.None;
        }
    }
}