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
        [Tooltip("Số lượng công trình tối đa trên bản đồ ở cấp độ này")]
        public int maxBuildingLimit = 10;
        
        [Tooltip("Các loại công trình sẽ được mở khoá khi đạt cấp độ này")]
        public List<BuildingType> unlockBuildingTypes = new List<BuildingType>();

        // Bạn hoàn toàn có thể thêm các thông số như tăng % sản xuất, % HP ở đây trong tương lai.
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

        public MainBuildingBehaviour(MainBuildingPresetSo preset, Vector2Int pos) : base(preset, pos)
        {
            // Mặc định nhà chính không cần dân làng (bạn có thể bỏ tick requireVillagers trong Inspector)
        }

        public override void Setup()
        {
            base.Setup();
            // Cập nhật các chỉ số ở Runtime ngay khi vừa Load Game hoặc Xây mới
            ApplyLevelConfig();
        }

        public override void RefreshBehaviour()
        {
            // Do nhà chính mang tính thụ động, ta có thể gọi lại ApplyLevelConfig để đảm bảo an toàn data
            ApplyLevelConfig();
        }

        protected override void HandleUpgrade()
        {
            // Base class đã tăng CurrentUpgradeLevel.
            // Ở đây ta gọi ApplyLevelConfig để cập nhật RuntimeProperties (Mở khoá nhà, tăng Max Building)
            ApplyLevelConfig();
        }

        public override void DestroyBehaviour()
        {
            base.DestroyBehaviour();
            
            // Giả sử phá Nhà Chính, reset giới hạn công trình về mốc mặc định (vd: 5)
            // Tuy nhiên với game xây dựng thường người chơi không thể phá Town Hall.
            GamePropertiesRuntime.Instance.MaxBuildingNumber.Value = 5;
        }

        /// <summary>
        /// Logic đọc cấu hình theo Level hiện tại và ghi đè vào GamePropertiesRuntime
        /// </summary>
        private void ApplyLevelConfig()
        {
            if (ActualPreset.levelConfigs == null || ActualPreset.levelConfigs.Count == 0) return;

            // Đảm bảo không truy cập mảng vượt quá giới hạn nếu Level cao hơn mảng cấu hình
            int index = Mathf.Clamp(CurrentUpgradeLevel - 1, 0, ActualPreset.levelConfigs.Count - 1);
            var currentConfig = ActualPreset.levelConfigs[index];

            // 1. Cập nhật Giới hạn công trình toàn bản đồ
            GamePropertiesRuntime.Instance.MaxBuildingNumber.Value = currentConfig.maxBuildingLimit;

            // 2. Mở khoá các loại công trình
            if (currentConfig.unlockBuildingTypes != null)
            {
                foreach (var buildingType in currentConfig.unlockBuildingTypes)
                {
                    // Lật flag trong dictionary thành true
                    GamePropertiesRuntime.Instance.UnlockBuildingTypeDict[buildingType] = true;
                }
            }

            // 3. Cập nhật UI nếu panel đang mở
            UpdateLimitValueLabel(currentConfig.maxBuildingLimit);
        }

        // ====================================================================
        // OVERRIDE BEHAVIOUR UI
        // ====================================================================

        protected override void UpdateBuildingLayoutUI()
        {
            base.UpdateBuildingLayoutUI(); // Gọi base để cập nhật level text

            bool isMaxLevel = CurrentUpgradeLevel >= ActualPreset.levelConfigs.Count;

            // Vô hiệu hoá nút Upgrade nếu đã đạt max cấp độ của mảng levelConfigs
            if (btnUpgrade != null)
            {
                btnUpgrade.SetEnabled(!isMaxLevel);
            }
        }

        protected override void HandleMouseEnterUpgradeButton(MouseEnterEvent evt)
        {
            bool isMaxLevel = CurrentUpgradeLevel >= ActualPreset.levelConfigs.Count;
            if (isMaxLevel)
            {
                // Hiển thị Tooltip Max Level thay vì giá nâng cấp
                _Project.Scripts.Gameplay.Global.Tooltip.TextTooltipController.Instance.Display(
                    "Level đã đạt đến tối đa", 
                    evt.mousePosition + new Vector2(10, -10));
            }
            else
            {
                // Gọi Tooltip của base class (hiển thị Cost) nếu chưa Max
                base.HandleMouseEnterUpgradeButton(evt);
            }
        }

        protected override void BuildBehaviourLayoutUI(VisualElement container)
        {
            var title = new Label("Trung Tâm Điều Hành");
            title.AddToClassList("behaviour-title");

            var row = new VisualElement();
            row.AddToClassList("resource-row"); // Có thể xài chung CSS của CapacityVillager

            var iconPlaceholder = new VisualElement();
            iconPlaceholder.AddToClassList("resource-icon-placeholder");
            // TODO: Gán ảnh icon đại diện cho "Giới hạn công trình"

            var nameLabel = new Label("Giới hạn xây dựng");
            nameLabel.AddToClassList("resource-name");

            limitValueLabel = new Label();
            limitValueLabel.AddToClassList("resource-value");
            
            // Lấy giá trị ban đầu để hiển thị
            int currentLimit = GamePropertiesRuntime.Instance.MaxBuildingNumber.Value;
            UpdateLimitValueLabel(currentLimit);

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
                limitValueLabel.text = $"{maxLimit} Công trình";
            }
        }
    }
}