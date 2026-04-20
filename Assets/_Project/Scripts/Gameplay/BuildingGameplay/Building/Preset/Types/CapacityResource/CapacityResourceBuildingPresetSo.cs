using System;
using Game.BuildingGameplay;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.StrategyBuilding
{
    [CreateAssetMenu(fileName = "Building_ResourceCapacity", menuName = "Game/StrategyBuilding/BuildingType/Resource Capacity")]
    public class CapacityResourceBuildingPresetSo : BuildingPresetSo
    {
        public LimitResourceType limitResourceType;
        public int defaultValue;
        public int incrementValue;

        protected override IBuildingBehaviour CreateBehaviour(Vector2Int pos)
        {
            var behaviour = new CapacityResourceBuildingBehaviour(this, pos);
            return behaviour;
        }
    }

    [Serializable]
    public class CapacityResourceBuildingBehaviour : BaseBuildingBehaviour<CapacityResourceBuildingPresetSo>
    {
        protected int PreTotalValue = 0;

        private LimitResourceType LimitResourceType => ActualPreset.limitResourceType;
        
        // UI Elements
        private Label capacityLabel;
        private Label usageIconValueLabel;

        public CapacityResourceBuildingBehaviour(CapacityResourceBuildingPresetSo preset, Vector2Int tilePosition) :
            base(preset, tilePosition)
        {
            // Bỏ ObservableValue cục bộ, sử dụng cơ chế RefreshBehaviour mỗi khi có biến động từ Nông dân / Level / Hiệu suất
        }

        /// <summary>
        /// Công thức mới: (Sức chứa cơ bản + Cấp độ * Tăng trưởng) * Tỉ lệ ảnh hưởng
        /// </summary>
        private int CalculateTotalValue()
        {
            float basePotential = ActualPreset.defaultValue + (ActualPreset.incrementValue * (CurrentUpgradeLevel - 1));
            // Bỏ đi "* UsedVillagers" vì công trình này không cần dân
            return Mathf.RoundToInt(basePotential * InfluenceRatio.Value);
        }

        public override void RefreshBehaviour()
        {
            var totalValue = CalculateTotalValue();
            
            // Tính lượng delta để bù trừ vào Storage tổng
            var delta = totalValue - PreTotalValue;
            
            if (delta != 0)
            {
                SbGameplayController.Instance.LimitResourceStorage[LimitResourceType].Value += delta;
                PreTotalValue = totalValue;

                // NẾU LƯỢNG MAX BỊ GIẢM ĐI -> ÉP LẠI TÀI NGUYÊN HIỆN CÓ XUỐNG DƯỚI MAX MỚI
                if (delta < 0)
                {
                    SbGameplayController.RevalidateResourceLimits();
                }
            }
            
            UpdateCapacityLabel(totalValue);
        }

        public override void DestroyBehaviour()
        {
            base.DestroyBehaviour(); // Gọi base trước để dọn UI Text cảnh báo
            var totalValue = CalculateTotalValue();
            
            // Phá kho thì sức chứa tổng phải giảm đi
            SbGameplayController.Instance.LimitResourceStorage[LimitResourceType].Value -= totalValue; 
            
            // KIỂM TRA LẠI SỨC CHỨA VÀ XOÁ BỎ TÀI NGUYÊN BỊ THỪA
            SbGameplayController.RevalidateResourceLimits();
        }

        protected override void HandleUpgrade()
        {
            // Logic Level và Nông dân được Handle ở BaseBuildingBehaviour. 
            // Ở đây không cần xử lý thêm, RefreshBehaviour sẽ tự động tính lại.
        }
        
        // ====================================================================
        // OVERRIDE BEHAVIOUR UI
        // ====================================================================
        protected override void BuildBehaviourLayoutUI(VisualElement container)
        {
            var title = new Label("Sức chứa tối đa");
            title.AddToClassList("behaviour-title");
            title.AddToClassList("title-capacity");

            var capacityRow = new VisualElement();
            capacityRow.AddToClassList("capacity-row");

            var iconPlaceholder = new VisualElement();
            iconPlaceholder.AddToClassList("capacity-icon-placeholder");
            // TODO: Gán icon thực tế vào iconPlaceholder

            var nameLabel = new Label(LimitResourceType.ToString());
            nameLabel.AddToClassList("capacity-name");

            // Giá trị khởi tạo lúc mới xây sẽ là 0 vì UsedVillagers = 0
            capacityLabel = new Label($"+{CalculateTotalValue()}");
            capacityLabel.AddToClassList("capacity-value");

            capacityRow.Add(iconPlaceholder);
            capacityRow.Add(nameLabel);
            capacityRow.Add(capacityLabel);

            container.Add(title);
            container.Add(capacityRow);
        }

        private void UpdateCapacityLabel(int currentTotal)
        {
            if (capacityLabel != null)
            {
                capacityLabel.text = $"+{currentTotal}";
            }

            if(usageIconValueLabel != null)
            {
                usageIconValueLabel.text = currentTotal.ToString();
            }
        }
    }
}