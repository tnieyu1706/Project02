using System;
using System.Collections.Generic;
using Game.BuildingGameplay;
using Game.Global;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.StrategyBuilding
{
    /// <summary>
    /// Struct lưu trữ cấu hình cho từng Cấp Độ (Level) của Nhà Chính
    /// </summary>
    [Serializable]
    public class MainBuildingLevelConfig
    {
        [Tooltip("Số lượng công trình MỞ RỘNG THÊM trên bản đồ ở cấp độ này")]
        public int additionalBuildingLimit = 10;

        [Tooltip("Các loại công trình sẽ được mở khoá khi đạt cấp độ này")]
        public List<BuildingType> unlockBuildingTypes = new List<BuildingType>();
    }

    [CreateAssetMenu(fileName = "Building_MainTownHall",
        menuName = "Game/StrategyBuilding/BuildingType/Main Town Hall")]
    public class MainBuildingPresetSo : BuildingPresetSo
    {
        [Header("Main Building Specifics")]
        [Tooltip("Cấu hình cho từng Level. Phần tử 0 = Level 1, Phần tử 1 = Level 2,...")]
        public List<MainBuildingLevelConfig> levelConfigs = new List<MainBuildingLevelConfig>();

        protected override IBuildingBehaviour CreateBehaviour(Vector2Int pos)
        {
            return new MainBuildingBehaviour(this, pos);
        }
    }

    [Serializable]
    public class MainBuildingBehaviour : BaseBuildingBehaviour<MainBuildingPresetSo>
    {
        private Label limitValueLabel;
        private int preAddedLimit = 0; // Biến nhớ để tính Delta

        public MainBuildingBehaviour(MainBuildingPresetSo preset, Vector2Int pos) : base(preset, pos)
        {
        }

        public override void Setup()
        {
            base.Setup();
            ApplyLevelConfig();
        }

        public override void RefreshBehaviour()
        {
            ApplyLevelConfig();
        }

        protected override void HandleUpgrade()
        {
            ApplyLevelConfig();
        }

        public override void DestroyBehaviour()
        {
            base.DestroyBehaviour();

            // Nếu nhà chính bị phá, thu hồi lại số slot đã cộng thêm
            if (preAddedLimit > 0)
            {
                if (GamePropertiesRuntime.HasInstance)
                    GamePropertiesRuntime.Instance.MaxBuildingNumber.Value -= preAddedLimit;
                preAddedLimit = 0;
            }
        }

        private void ApplyLevelConfig()
        {
            if (ActualPreset.levelConfigs == null || ActualPreset.levelConfigs.Count == 0) return;

            int index = Mathf.Clamp(CurrentUpgradeLevel - 1, 0, ActualPreset.levelConfigs.Count - 1);
            var currentConfig = ActualPreset.levelConfigs[index];

            // ĐÃ SỬA: Nếu đang xây thì chưa cung cấp thêm Limit
            int newLimit = IsUnderConstruction ? 0 : currentConfig.additionalBuildingLimit;
            int delta = newLimit - preAddedLimit;

            if (delta != 0)
            {
                if (GamePropertiesRuntime.HasInstance)
                    GamePropertiesRuntime.Instance.MaxBuildingNumber.Value += delta;
                preAddedLimit = newLimit;
            }

            // ĐÃ SỬA: Đang xây thì chưa mở khóa công trình
            if (!IsUnderConstruction && currentConfig.unlockBuildingTypes != null)
            {
                foreach (var buildingType in currentConfig.unlockBuildingTypes)
                {
                    GamePropertiesRuntime.Instance.UnlockBuildingTypeDict[buildingType] = true;
                }
            }

            UpdateLimitValueLabel(newLimit);
        }

        // ====================================================================
        // OVERRIDE BEHAVIOUR UI
        // ====================================================================

        protected override void UpdateBuildingLayoutUI()
        {
            base.UpdateBuildingLayoutUI();

            bool isMaxLevel = CurrentUpgradeLevel >= ActualPreset.levelConfigs.Count;

            if (btnUpgrade != null)
            {
                btnUpgrade.SetEnabled(!isMaxLevel && !IsUnderConstruction);
            }
        }

        protected override void HandleMouseEnterUpgradeButton(MouseEnterEvent evt)
        {
            bool isMaxLevel = CurrentUpgradeLevel >= ActualPreset.levelConfigs.Count;
            if (isMaxLevel)
            {
                _Project.Scripts.Gameplay.Global.Tooltip.TextTooltipController.Instance.Display(
                    "Level đã đạt đến tối đa",
                    evt.mousePosition + new Vector2(10, -10));
            }
            else
            {
                base.HandleMouseEnterUpgradeButton(evt);
            }
        }

        protected override void BuildBehaviourLayoutUI(VisualElement container)
        {
            var title = new Label("Operations Center");
            title.AddToClassList("behaviour-title");

            var row = new VisualElement();
            row.AddToClassList("resource-row");

            var iconPlaceholder = new VisualElement();
            iconPlaceholder.AddToClassList("resource-icon-placeholder");

            var nameLabel = new Label("Capacity Bonus");
            nameLabel.AddToClassList("resource-name");

            limitValueLabel = new Label();
            limitValueLabel.AddToClassList("resource-value");

            UpdateLimitValueLabel(preAddedLimit);

            row.Add(iconPlaceholder);
            row.Add(nameLabel);
            row.Add(limitValueLabel);

            container.Add(title);
            container.Add(row);
        }

        private void UpdateLimitValueLabel(int maxLimit)
        {
            if (limitValueLabel != null)
            {
                limitValueLabel.text = $"+{maxLimit}";
            }
        }
    }
}