using _Project.Scripts.Gameplay.Global.Tooltip; // Cần tham chiếu tới LayoutTooltipController
using Game.StrategyBuilding;
using TnieYuPackage.GlobalExtensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.BuildingGameplay
{
    public class BuildingDetailItem : DetailItem
    {
        private static StyleSheet styleSheet;
        public BuildingPresetSo BuildingPreset;

        // Cache lại các Element để tối ưu hiệu suất, không cần khởi tạo lại nhiều lần
        private VisualElement tooltipContent;
        private Label tooltipNameLabel;
        private Label tooltipTileLayerLabel;
        private Label tooltipCostLabel;
        private Label tooltipDescLabel;
        private VisualElement effectsContainer;

        public BuildingDetailItem() : base()
        {
            // Khởi tạo sẵn cấu trúc Tooltip 1 lần duy nhất
            InitTooltipStructure();

            // Đăng ký sự kiện Hover chuột
            this.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            this.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
        }

        private void InitTooltipStructure()
        {
            tooltipContent = new VisualElement();
            tooltipContent.AddClass("building-tooltip-root");
            tooltipContent.pickingMode = PickingMode.Ignore;

            // Tự động load stylesheet từ thư mục Resources
            styleSheet ??= Resources.Load<StyleSheet>("BuildingDetailItemStyle");
            if (styleSheet != null)
            {
                tooltipContent.styleSheets.Add(styleSheet);
            }
            else
            {
                Debug.LogWarning("Không tìm thấy file BuildingDetailItemStyle.uss trong thư mục Resources!");
            }

            // 1. Header Row (Name & TileLayer)
            var headerRow = tooltipContent.CreateChild<VisualElement>("tooltip-header-row");
            headerRow.pickingMode = PickingMode.Ignore;
            tooltipNameLabel = headerRow.CreateChild<Label>("tooltip-building-name");
            tooltipTileLayerLabel = headerRow.CreateChild<Label>("tooltip-tile-layer");

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

            // 4. Influence Effects Container
            effectsContainer = CreateBorderedBox("tooltip-effects-container");
        }

        public void SetItem(BuildingPresetSo buildingPreset)
        {
            BuildingPreset = buildingPreset;

            // 1. Update UI của Item trong List
            this.Image.sprite = buildingPreset.buildingTile.sprite;

            // 2. Update data vào khung Tooltip đã được dựng sẵn
            tooltipNameLabel.text = buildingPreset.buildingId;
            tooltipTileLayerLabel.text = buildingPreset.tileLayer.ToString();

            tooltipCostLabel.text = buildingPreset.costBuilding.CloneData.GetTextHorizontal();

            tooltipDescLabel.text =
                "Đây là mô tả của công trình. Nó giúp sản xuất tài nguyên hoặc cung cấp các hiệu ứng đặc biệt.";

            // 3. Xây dựng lại danh sách Influence Effects
            effectsContainer.Clear();
            if (buildingPreset.InfluenceEffects != null && buildingPreset.InfluenceEffects.Count > 0)
            {
                var effectTitle = effectsContainer.CreateChild<Label>("tooltip-effects-title");
                effectTitle.text = "Influence Effects";

                foreach (var kvp in buildingPreset.InfluenceEffects)
                {
                    var effectRow = effectsContainer.CreateChild<VisualElement>("tooltip-effect-row");
                    effectRow.AddClass("tooltip-effect-row-class");

                    // Hiển thị dạng (TileLayer: EffectName (Value))
                    var layerLabel = effectRow.CreateChild<Label>("tooltip-effect-layer");
                    layerLabel.text = $"{kvp.Key}: ";

                    var effectTypeLabel = effectRow.CreateChild<Label>("tooltip-effect-type");
                    effectTypeLabel.text = $"{kvp.Value.EffectName} ({kvp.Value.GetEffectValue()})";

                    // Giữ lại style.color vì thuộc tính này là Dynamic (phụ thuộc vào Data chứ không cố định)
                    effectTypeLabel.style.color = kvp.Value.EffectColor;
                }

                effectsContainer.style.display = DisplayStyle.Flex;
            }
            else
            {
                // Ẩn đi nếu công trình không có hiệu ứng ảnh hưởng nào
                effectsContainer.style.display = DisplayStyle.None;
            }
        }

        private void OnMouseEnter(MouseEnterEvent evt)
        {
            if (BuildingPreset == null) return;

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
            // Tránh lỗi add 2 lần nếu spam hover
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