using System;
using System.Collections.Generic;
using Game.BuildingGameplay;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.StrategyBuilding
{
    [CreateAssetMenu(fileName = "Building_VillagerCapacity",
        menuName = "Game/StrategyBuilding/BuildingType/Villager Capacity")]
    public class CapacityVillagerBuildingPresetSo : BuildingPresetSo
    {
        [Header("Capacity Settings")] [Tooltip("Số lượng dân làng tối đa tăng thêm ở cấp độ 1")]
        public int defaultCapacityValue = 2;

        [Tooltip("Số lượng dân làng chứa thêm mỗi khi nâng cấp")]
        public int incrementCapacityValue = 1;

        protected override IBuildingBehaviour CreateBehaviour(Vector2Int pos)
        {
            var behaviour = new CapacityVillagerBuildingBehaviour(this, pos);
            return behaviour;
        }
    }

    [Serializable]
    public class CapacityVillagerBuildingBehaviour : BaseBuildingBehaviour<CapacityVillagerBuildingPresetSo>
    {
        // Lưu lại sức chứa đã cộng thêm để trừ đi chính xác khi phá huỷ hoặc tính toán delta khi nâng cấp
        protected int PreAddedCapacity = 0;

        // UI Elements
        private Label capacityValueLabel;

        public CapacityVillagerBuildingBehaviour(CapacityVillagerBuildingPresetSo preset, Vector2Int tilePosition) :
            base(preset, tilePosition)
        {
        }

        /// <summary>
        /// Tính toán tổng sức chứa dân làng cộng thêm dựa trên Cấp độ, Nông dân sử dụng và Tỉ lệ ảnh hưởng.
        /// Thường thì Nhà ở (House) sẽ không yêu cầu nông dân vận hành (requireVillagers = false).
        /// </summary>
        protected int CalculateTotalCapacity()
        {
            float baseCapacity = ActualPreset.defaultCapacityValue +
                                 (ActualPreset.incrementCapacityValue * (CurrentUpgradeLevel - 1));

            float multiplier = ActualPreset.requireVillagers ? UsedVillagers : 1f;

            // Ép kiểu về int vì số lượng dân làng là số nguyên
            return Mathf.FloorToInt((baseCapacity * InfluenceRatio.Value) * multiplier);
        }

        public override void RefreshBehaviour()
        {
            int totalCapacity = CalculateTotalCapacity();

            // Tính toán lượng thay đổi (Delta) để cộng/trừ vào MaxVillagers
            int delta = totalCapacity - PreAddedCapacity;

            if (delta != 0)
            {
                SbGameplayController.Instance.VillagerData.MaxVillagers.Value += delta;
                PreAddedCapacity = totalCapacity;
            }

            // Cập nhật giao diện
            UpdateCapacityValueLabel(totalCapacity);
        }

        public override void DestroyBehaviour()
        {
            // Gọi base trước để hoàn trả nông dân đang làm việc (nếu công trình này có requireVillagers)
            base.DestroyBehaviour();

            // Trừ đi lượng sức chứa mà nhà này đang cung cấp
            if (PreAddedCapacity > 0)
            {
                SbGameplayController.Instance.VillagerData.MaxVillagers.Value -= PreAddedCapacity;
                PreAddedCapacity = 0;
            }
        }

        protected override void HandleUpgrade()
        {
            // Base class đã tăng CurrentUpgradeLevel.
            // Hàm RefreshBehaviour() được gọi ngay sau đó trong UpgradeBehaviour() sẽ tự tính toán và cộng thêm Delta.
        }

        // ====================================================================
        // OVERRIDE BEHAVIOUR UI
        // ====================================================================

        protected override List<(string, Color)> GetResourcePopupTexts()
        {
            // Lấy danh sách tiêu thụ tài nguyên mặc định
            List<(string, Color)> popupTexts = base.GetResourcePopupTexts() ?? new();

            // Bạn có thể thêm text popup "+X Dân làng" ở đây nếu muốn hiển thị khi nhà hoạt động, 
            // nhưng thường sức chứa là số tĩnh (tăng max) chứ không phải sản xuất mỗi giây (produce), 
            // nên ta có thể bỏ qua không popup.
            return popupTexts;
        }

        protected override void BuildBehaviourLayoutUI(VisualElement container)
        {
            var title = new Label("Cung cấp chỗ ở");
            title.AddToClassList("behaviour-title");

            var capacityRow = new VisualElement();
            capacityRow.AddToClassList("resource-row"); // Tận dụng lại class CSS cũ để UI đồng bộ

            var iconPlaceholder = new VisualElement();
            iconPlaceholder.AddToClassList("resource-icon-placeholder");
            // TODO: Gán icon Dân làng thực tế: iconPlaceholder.style.backgroundImage = ...

            var nameLabel = new Label("Sức chứa");
            nameLabel.AddToClassList("resource-name");

            // Giá trị khởi tạo
            capacityValueLabel = new Label($"+{CalculateTotalCapacity()} Người");
            capacityValueLabel.AddToClassList("resource-value");

            capacityRow.Add(iconPlaceholder);
            capacityRow.Add(nameLabel);
            capacityRow.Add(capacityValueLabel);

            container.Add(title);
            container.Add(capacityRow);
        }

        private void UpdateCapacityValueLabel(int currentTotal)
        {
            if (capacityValueLabel != null)
            {
                capacityValueLabel.text = $"+{currentTotal} Người";
            }
        }
    }
}