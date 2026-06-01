using System;
using System.Collections.Generic;
using Game.BuildingGameplay;
using Game.Global;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.StrategyBuilding
{
    [CreateAssetMenu(fileName = "Building_Station",
        menuName = "Game/StrategyBuilding/BuildingType/Station Expansion")]
    public class StationBuildingPresetSo : BuildingPresetSo
    {
        [Header("Station Settings")] public int defaultAdditionalLimit = 2; // Số lượng mở rộng cơ bản (Level 1)
        public int incrementAdditionalLimit = 1; // Thêm vào mỗi khi up level

        protected override IBuildingBehaviour CreateBehaviour(Vector2Int pos)
        {
            return new StationBuildingBehaviour(this, pos);
        }
    }

    [Serializable]
    public class StationBuildingBehaviour : BaseBuildingBehaviour<StationBuildingPresetSo>
    {
        private int preAddedLimit = 0;

        // UI Elements
        private Label limitValueLabel;

        public StationBuildingBehaviour(StationBuildingPresetSo preset, Vector2Int tilePosition) :
            base(preset, tilePosition)
        {
        }

        /// <summary>
        /// Tính toán lượng mở rộng không gian xây dựng dựa trên level, dân số và hệ số ảnh hưởng
        /// </summary>
        protected int CalculateAdditionalLimit()
        {
            // ĐÃ SỬA: Nếu đang trong quá trình chờ xây dựng, giá trị đóng góp là 0
            if (IsUnderConstruction) return 0;

            float basePotential = ActualPreset.defaultAdditionalLimit +
                                  (ActualPreset.incrementAdditionalLimit * (CurrentUpgradeLevel - 1));
            float multiplier = ActualPreset.requireVillagers ? UsedVillagers : 1f;
            return Mathf.FloorToInt((basePotential * InfluenceRatio.Value) * multiplier);
        }

        public override void RefreshBehaviour()
        {
            int newLimit = CalculateAdditionalLimit();
            int delta = newLimit - preAddedLimit;

            if (delta != 0)
            {
                if (GamePropertiesRuntime.HasInstance)
                    GamePropertiesRuntime.Instance.MaxBuildingNumber.Value += delta;
                preAddedLimit = newLimit;
            }

            UpdateLimitValueLabel(newLimit);
        }

        public override void DestroyBehaviour()
        {
            base.DestroyBehaviour();

            // Thu hồi lại phần dung lượng đã cung cấp khi trạm bị phá hủy
            if (preAddedLimit > 0)
            {
                if (GamePropertiesRuntime.HasInstance)
                    GamePropertiesRuntime.Instance.MaxBuildingNumber.Value -= preAddedLimit;
                preAddedLimit = 0;
            }
        }

        protected override void HandleUpgrade()
        {
            // Tương tự IncreaseResource, Base Class đã tăng Level nên Refresh sẽ tự tính toán
        }

        // ====================================================================
        // OVERRIDE BEHAVIOUR UI
        // ====================================================================

        // protected override List<(string, Color)> GetResourcePopupTexts()
        // {
        //     List<(string, Color)> popupTexts = base.GetResourcePopupTexts() ?? new();
        //
        //     int produceValue = CalculateAdditionalLimit();
        //     if (produceValue > 0)
        //     {
        //         // Cho hiển thị màu Xanh Lục cho số slot được cộng thêm
        //         popupTexts.Add(($"+{produceValue} Slots", Color.green));
        //     }
        //
        //     return popupTexts;
        // }

        // protected override void SubHandleActiveBuildingApplyResource()
        // {
        //     base.SubHandleActiveBuildingApplyResource();
        //     if (ActualPreset.sfxData != null)
        //     {
        //         SfxManager.PlayVfx(ActualPreset.sfxData);
        //     }
        // }

        protected override void BuildBehaviourLayoutUI(VisualElement container)
        {
            var title = new Label("Station Expansion");
            title.AddToClassList("behaviour-title");
            title.AddToClassList("title-increase");

            var resourceRow = new VisualElement();
            resourceRow.AddToClassList("resource-row");

            var iconPlaceholder = new VisualElement();
            iconPlaceholder.AddToClassList("resource-icon-placeholder");

            var nameLabel = new Label("Capacity Slots");
            nameLabel.AddToClassList("resource-name");

            limitValueLabel = new Label($"+{CalculateAdditionalLimit()}");
            limitValueLabel.AddToClassList("resource-value");

            resourceRow.Add(iconPlaceholder);
            resourceRow.Add(nameLabel);
            resourceRow.Add(limitValueLabel);

            container.Add(title);
            container.Add(resourceRow);
        }

        private void UpdateLimitValueLabel(int currentTotal)
        {
            if (limitValueLabel != null)
            {
                limitValueLabel.text = $"+{currentTotal}";
            }
        }
    }
}