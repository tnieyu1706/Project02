using System;
using System.Collections.Generic;
using Game.BuildingGameplay;
using Game.Global;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.StrategyBuilding
{
    [CreateAssetMenu(fileName = "Building_SkillPointIncrease",
        menuName = "Game/StrategyBuilding/BuildingType/Skill Point Increase")]
    public class IncreaseSkillPointBuildingPresetSo : BuildingPresetSo
    {
        [Header("Skill Point Settings")] public int defaultSkillPoints = 1; // Điểm cộng thêm cơ bản ở cấp 1
        public int incrementSkillPoints = 1; // Điểm tăng thêm mỗi level

        protected override IBuildingBehaviour CreateBehaviour(Vector2Int pos)
        {
            return new IncreaseSkillPointBuildingBehaviour(this, pos);
        }
    }

    [Serializable]
    public class IncreaseSkillPointBuildingBehaviour : BaseBuildingBehaviour<IncreaseSkillPointBuildingPresetSo>
    {
        // UI Elements
        private Label skillPointValueLabel;

        public IncreaseSkillPointBuildingBehaviour(IncreaseSkillPointBuildingPresetSo preset, Vector2Int tilePosition) :
            base(preset, tilePosition)
        {
        }

        public override void Setup()
        {
            base.Setup();
            // Đăng ký lắng nghe mỗi khi hệ thống chạy ApplyResourceIncrement
            SbGameplayController.OnActiveBuildingApplyResource += HandleProduceSkillPoints;
        }

        public override void DestroyBehaviour()
        {
            base.DestroyBehaviour();
            // Hủy đăng ký lắng nghe để tránh lỗi Memory Leak khi phá nhà
            SbGameplayController.OnActiveBuildingApplyResource -= HandleProduceSkillPoints;
        }

        /// <summary>
        /// Tính toán lượng Skill Point sản xuất ở mỗi chu kỳ
        /// </summary>
        protected int CalculateSkillPointsToProduce()
        {
            float basePoints = ActualPreset.defaultSkillPoints +
                               (ActualPreset.incrementSkillPoints * (CurrentUpgradeLevel - 1));
            float multiplier = ActualPreset.requireVillagers ? UsedVillagers : 1f;
            return Mathf.FloorToInt((basePoints * InfluenceRatio.Value) * multiplier);
        }

        private void HandleProduceSkillPoints()
        {
            int pointsToProduce = CalculateSkillPointsToProduce();
            if (pointsToProduce > 0)
            {
                // Cộng trực tiếp vào ObservableValue của GamePropertiesRuntime
                GamePropertiesRuntime.Instance.SkillPoints.Value += pointsToProduce;
            }
        }

        public override void RefreshBehaviour()
        {
            // Cập nhật lại UI thông số mỗi khi có sự thay đổi (như thêm/bớt dân làng)
            UpdateSkillPointValueLabel(CalculateSkillPointsToProduce());
        }

        protected override void HandleUpgrade()
        {
            // Base class đã tăng CurrentUpgradeLevel.
            // RefreshBehaviour() tự động được gọi sau đó nên không cần làm gì thêm ở đây.
        }

        // ====================================================================
        // OVERRIDE BEHAVIOUR UI
        // ====================================================================

        protected override List<(string, Color)> GetResourcePopupTexts()
        {
            // Lấy danh sách tiêu thụ tài nguyên của base
            List<(string, Color)> popupTexts = base.GetResourcePopupTexts() ?? new();

            int produceValue = CalculateSkillPointsToProduce();
            if (produceValue > 0)
            {
                // Cho Skill Point màu khác biệt (vd màu Cyan) để dễ nhận diện trên màn hình
                popupTexts.Add(($"+{produceValue} SP", Color.cyan));
            }

            return popupTexts;
        }

        protected override void BuildBehaviourLayoutUI(VisualElement container)
        {
            var title = new Label("Sản xuất Điểm Kỹ Năng");
            title.AddToClassList("behaviour-title");

            var row = new VisualElement();
            row.AddToClassList("resource-row");

            var iconPlaceholder = new VisualElement();
            iconPlaceholder.AddToClassList("resource-icon-placeholder");
            // TODO: Gán ảnh icon Skill Point vào đây

            var nameLabel = new Label("Skill Point");
            nameLabel.AddToClassList("resource-name");

            skillPointValueLabel = new Label($"+{CalculateSkillPointsToProduce()} / chu kỳ");
            skillPointValueLabel.AddToClassList("resource-value");

            row.Add(iconPlaceholder);
            row.Add(nameLabel);
            row.Add(skillPointValueLabel);

            container.Add(title);
            container.Add(row);
        }

        private void UpdateSkillPointValueLabel(int currentTotal)
        {
            if (skillPointValueLabel != null)
            {
                skillPointValueLabel.text = $"+{currentTotal} / chu kỳ";
            }
        }
    }
}