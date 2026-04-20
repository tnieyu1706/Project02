using System;
using Game.StrategyBuilding;
using TnieYuPackage.GlobalExtensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.BuildingGameplay
{
    [CreateAssetMenu(fileName = "Building_Army", menuName = "Game/StrategyBuilding/BuildingType/Army")]
    public class ArmyBuildingPresetSo : BuildingPresetSo
    {
        public int maxSpawnSlot;

        public float defaultRate;
        public float incrementRate;

        protected override IBuildingBehaviour CreateBehaviour(Vector2Int pos)
        {
            var behaviour = new ArmyBuildingBehaviour(this, pos);
            return behaviour;
        }
    }

    [Serializable]
    public class ArmyBuildingBehaviour : BaseBuildingBehaviour<ArmyBuildingPresetSo>
    {
        private Label rateLabel;
        private VisualElement slotsContainer;

        // Ghi đè trạng thái báo Bận khi có bất kỳ slot lính nào đang đếm tiến trình
        protected override bool IsBusy
        {
            get
            {
                if (slotsContainer == null) return false;
                foreach (var child in slotsContainer.Children())
                {
                    if (child is ArmySlotItem slotItem && slotItem.IsSpawning)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        // Ghi đè Tooltip
        protected override string BusyReason => "Đang huấn luyện lính, vui lòng chờ hoàn thành trước khi thay đổi nông dân.";

        public ArmyBuildingBehaviour(ArmyBuildingPresetSo preset, Vector2Int tilePosition) : base(preset, tilePosition)
        {
        }

        public float GetTotalRate()
        {
            float basePotential = ActualPreset.defaultRate + (ActualPreset.incrementRate * (CurrentUpgradeLevel - 1));
            return (basePotential * InfluenceRatio.Value) * UsedVillagers;
        }

        public override void RefreshBehaviour()
        {
            OnRateUIUpdated(GetTotalRate());

            if (slotsContainer != null)
            {
                slotsContainer.SetEnabled(UsedVillagers > 0);
            }
        }

        public override void DestroyBehaviour()
        {
            base.DestroyBehaviour(); // Quan trọng: Gọi base để xoá Persistent UI/Villagers
            
            if (slotsContainer != null)
            {
                foreach (var child in slotsContainer.Children())
                {
                    if (child is ArmySlotItem slotItem)
                    {
                        slotItem.CancelSpawn();
                    }
                }
            }
        }

        protected override void HandleUpgrade()
        {
        }

        // ==========================================
        // UI TOOLKIT LAYOUT OVERRIDE
        // ==========================================
        protected override void BuildBehaviourLayoutUI(VisualElement container)
        {
            container.style.flexDirection = FlexDirection.Row;

            // --- LEFT PANEL: Army Information ---
            var leftPanel = container.CreateChild("army-info-panel");
            leftPanel.CreateChild(new Label("Army Camp Info"), "army-info-title");
            
            rateLabel = leftPanel.CreateChild(new Label(GetPropertiesText(GetTotalRate())), "army-info-rate");

            // --- RIGHT PANEL: Spawn Slots ---
            var rightPanel = container.CreateChild("army-slots-panel");
            rightPanel.CreateChild(new Label("Spawn Slots"), "army-slots-title");

            slotsContainer = rightPanel.CreateChild("army-slots-container");

            for (int i = 0; i < ActualPreset.maxSpawnSlot; i++)
            {
                var slotItem = new ArmySlotItem(this);
                
                // Móc nối event OnSpawnStateChanged để gọi lại UpdateVillagersUIState ở Base Class
                slotItem.OnSpawnStateChanged += UpdateVillagersUIState;
                
                slotsContainer.Add(slotItem);
            }

            // Gán trạng thái ban đầu
            slotsContainer.SetEnabled(UsedVillagers > 0);
        }

        private void OnRateUIUpdated(float totalRate)
        {
            if (rateLabel != null)
            {
                rateLabel.text = GetPropertiesText(totalRate);
            }
        }

        private string GetPropertiesText(float totalRate)
        {
            float basePotential = ActualPreset.defaultRate + (ActualPreset.incrementRate * (CurrentUpgradeLevel - 1));
            return $"Base Speed: {basePotential}\n" +
                   $"Total Rate: {totalRate}";
        }
    }
}