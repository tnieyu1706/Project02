using System;
using System.Collections.Generic; // Để xài List<string>
using Game.BuildingGameplay;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.StrategyBuilding
{
    [CreateAssetMenu(fileName = "Building_ResourceIncrease",
        menuName = "Game/StrategyBuilding/BuildingType/Resource Increase")]
    public class IncreaseResourceBuildingPresetSo : BuildingPresetSo
    {
        public ResourceType resourceType;
        public float defaultValue; // Base production (Level 1)
        public float incrementValue; // Thêm vào base mỗi khi up level

        protected override IBuildingBehaviour CreateBehaviour(Vector2Int pos)
        {
            var behaviour = new IncreaseResourceBuildingBehaviour(this, pos);
            return behaviour;
        }
    }

    [Serializable]
    public class IncreaseResourceBuildingBehaviour : BaseBuildingBehaviour<IncreaseResourceBuildingPresetSo>
    {
        protected float PreTotalValue = 0;

        private ResourceType ResourceType => ActualPreset.resourceType;

        // UI Elements
        private Label resourceValueLabel;
        private Label usageIconValueLabel; // Để gán lên chỗ icon của base layout (như trong ảnh vẽ)

        public IncreaseResourceBuildingBehaviour(IncreaseResourceBuildingPresetSo preset, Vector2Int tilePosition) :
            base(preset, tilePosition)
        {
            // Do bây giờ giá trị phụ thuộc vào UsedVillagers và Level, ta không cần ObservableValue cục bộ nữa 
            // (hoặc nếu dùng thì để event bắn UI, nhưng gọi Update thẳng trong RefreshBehaviour cũng được).
        }

        /// <summary>
        /// Công thức mới: ((Giá trị cơ bản + Cấp độ * Tăng trưởng) * Tỉ lệ ảnh hưởng) * Hệ số Nông dân
        /// </summary>
        protected float CalculateTotalValue()
        {
            // CurrentUpgradeLevel bắt đầu từ 1, nên tăng trưởng sẽ nhân với (Level - 1)
            float basePotential = ActualPreset.defaultValue + (ActualPreset.incrementValue * (CurrentUpgradeLevel - 1));

            // Hỗ trợ trường hợp nếu bạn tạo nhà sản xuất thụ động (requireVillagers = false)
            float multiplier = ActualPreset.requireVillagers ? UsedVillagers : 1f;

            return (float)Math.Round((basePotential * this.InfluenceRatio.Value) * multiplier, 1);
        }

        public override void RefreshBehaviour()
        {
            var totalValue = CalculateTotalValue();

            // Tính toán lượng thay đổi (Delta) để cộng/trừ vào storage chung
            var delta = totalValue - PreTotalValue;

            if (delta != 0)
            {
                SbGameplayController.Instance.IncrementResources[ResourceType].Value += delta;
                PreTotalValue = totalValue;
            }

            // Cập nhật giao diện
            UpdateResourceValueLabel(totalValue);
        }

        public override void DestroyBehaviour()
        {
            base.DestroyBehaviour(); // Đừng quên gọi base.DestroyBehaviour để xử lí tiêu hao nhé
            var totalValue = CalculateTotalValue();
            SbGameplayController.Instance.IncrementResources[ResourceType].Value -= totalValue;
        }

        protected override void HandleUpgrade()
        {
            // Base class đã tăng CurrentUpgradeLevel.
            // Ở đây bạn không cần làm gì thêm, RefreshBehaviour() được gọi sau đó sẽ tự tính lại dựa trên Level mới.
        }

        // ====================================================================
        // OVERRIDE BEHAVIOUR UI VÀ POPUP TEXT
        // ====================================================================

        protected override List<(string, Color)> GetResourcePopupTexts()
        {
            // 1. Lấy danh sách các tài nguyên bị tiêu thụ từ base class
            List<(string, Color)> popupTexts = base.GetResourcePopupTexts() ?? new();

            // 2. Tính toán tài nguyên được sản xuất ra
            float produceValue = CalculateTotalValue();

            // 3. Nếu có sản xuất, nhét thêm vào danh sách (Mang dấu + để ScreenTextDisplayController tô màu xanh)
            if (produceValue > 0)
            {
                popupTexts.Add(($"+{produceValue} {ActualPreset.resourceType}", Color.green));
            }

            return popupTexts;
        }

        protected override void BuildBehaviourLayoutUI(VisualElement container)
        {
            var title = new Label("Sản xuất Tài nguyên");
            title.AddToClassList("behaviour-title");
            title.AddToClassList("title-increase");

            var resourceRow = new VisualElement();
            resourceRow.AddToClassList("resource-row");

            var iconPlaceholder = new VisualElement();
            iconPlaceholder.AddToClassList("resource-icon-placeholder");
            // TODO: Gán icon thực tế: iconPlaceholder.style.backgroundImage = ResourceTypeDataManager.Resources[ResourceType].icon.texture;

            var nameLabel = new Label(ResourceType.ToString());
            nameLabel.AddToClassList("resource-name");

            // Giá trị khởi tạo lúc xây (sẽ là 0 vì UsedVillager ban đầu = 0)
            resourceValueLabel = new Label($"+{CalculateTotalValue()} / s");
            resourceValueLabel.AddToClassList("resource-value");

            resourceRow.Add(iconPlaceholder);
            resourceRow.Add(nameLabel);
            resourceRow.Add(resourceValueLabel);

            container.Add(title);
            container.Add(resourceRow);
        }

        private void UpdateResourceValueLabel(float currentTotal)
        {
            if (resourceValueLabel != null)
            {
                resourceValueLabel.text = $"+{currentTotal} / s";
            }

            // Cập nhật cả cái label Usage icon nhỏ ở layout base nếu bạn móc được tham chiếu
            if (usageIconValueLabel != null)
            {
                usageIconValueLabel.text = currentTotal.ToString();
            }
        }
    }
}