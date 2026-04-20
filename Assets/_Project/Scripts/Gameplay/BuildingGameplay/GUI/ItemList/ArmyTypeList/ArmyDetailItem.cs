using _Project.Scripts.Gameplay.Global.Tooltip;
using TnieYuPackage.GlobalExtensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.BuildingGameplay
{
    public class ArmyDetailItem : DetailItem
    {
        private static StyleSheet styleSheet;

        public ArmyTypePresetSo ArmyTypePreset;

        // Cache lại các Element để tối ưu hiệu suất giống như Building
        private VisualElement tooltipContent;
        private Label tooltipNameLabel;
        private Label tooltipCostLabel;
        private Label tooltipDescLabel;

        public ArmyDetailItem() : base()
        {
            InitTooltipStructure();

            // Đăng ký sự kiện Hover chuột
            this.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            this.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
        }

        private void InitTooltipStructure()
        {
            tooltipContent = new VisualElement();
            tooltipContent.AddClass("army-tooltip-root");
            tooltipContent.pickingMode = PickingMode.Ignore;

            // Tự động load stylesheet riêng của Army từ thư mục Resources
            styleSheet ??= Resources.Load<StyleSheet>("ArmyDetailItemStyle");
            if (styleSheet != null)
            {
                tooltipContent.styleSheets.Add(styleSheet);
            }
            else
            {
                Debug.LogWarning("Không tìm thấy file ArmyDetailItemStyle.uss trong thư mục Resources!");
            }

            // 1. Tên loại quân (Được căn giữa theo style mới)
            tooltipNameLabel = tooltipContent.CreateChild<Label>("tooltip-army-name");
            tooltipNameLabel.pickingMode = PickingMode.Ignore;

            // Helper function để tạo các block box và thêm class chung
            VisualElement CreateBorderedBox(string className)
            {
                var box = tooltipContent.CreateChild<VisualElement>(className);
                box.AddClass("tooltip-bordered-box");
                box.pickingMode = PickingMode.Ignore;
                return box;
            }

            // 2. Cost Layout
            var costContainer = CreateBorderedBox("tooltip-cost-container");
            tooltipCostLabel = costContainer.CreateChild<Label>("tooltip-cost-text");

            // 3. Description Layout
            var descContainer = CreateBorderedBox("tooltip-desc-container");
            tooltipDescLabel = descContainer.CreateChild<Label>("tooltip-desc-text");
        }

        public void SetItem(ArmyTypePresetSo armyTypePreset)
        {
            this.ArmyTypePreset = armyTypePreset;

            this.Image.sprite = armyTypePreset.icon;

            // Update data vào khung Tooltip đã dựng sẵn
            tooltipNameLabel.text = armyTypePreset.armyType.ToString();

            // Dùng GetTextHorizontal để dàn hàng ngang cho đẹp vì layout ta thiết kế dạng box
            tooltipCostLabel.text = $"Cost: {armyTypePreset.cost.CloneData.GetTextHorizontal()}";

            tooltipDescLabel.text =
                $"Thời gian huấn luyện: {armyTypePreset.delaySpawn}s.\nĐây là mô tả chi tiết sức mạnh của loại quân này.";
        }

        private void OnMouseEnter(MouseEnterEvent evt)
        {
            if (ArmyTypePreset == null) return;

            // Gọi Layout Tooltip và truyền hàm gắn layout vào
            LayoutTooltipController.Instance.Display(
                AttachTooltipLayout,
                DetachTooltipLayout,
                evt.mousePosition // Lấy vị trí chuột hiện tại để hiển thị Tooltip
            );
        }

        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            LayoutTooltipController.Instance.Hide();
        }

        /// <summary>
        /// Gắn layout đã tạo sẵn vào Tooltip Container
        /// </summary>
        private void AttachTooltipLayout(VisualElement container)
        {
            if (!container.Contains(tooltipContent))
            {
                container.Add(tooltipContent);
            }
        }

        /// <summary>
        /// Gỡ layout khỏi Tooltip Container
        /// </summary>
        private void DetachTooltipLayout(VisualElement container)
        {
            if (container.Contains(tooltipContent))
            {
                container.Remove(tooltipContent);
            }
        }
    }
}