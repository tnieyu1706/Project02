using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Global.Tooltip;
using Game.BuildingGameplay;
using TnieYuPackage.Utils;
using TnieYuPackage.GlobalExtensions;
using TnieYuPackage.UI;
using UnityEngine;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks;

namespace Game.StrategyBuilding
{
    [Serializable]
    public abstract class BaseBuildingBehaviour<TPreset> : IBuildingBehaviour<TPreset>
        where TPreset : BuildingPresetSo
    {
        public const float REFUND_RATIO = 0.8f; // Tỉ lệ hoàn trả tài nguyên khi phá hủy công trình

        public TPreset ActualPreset { get; }
        public ActionCost UpgradeCostRuntime { get; private set; }

        public int CurrentUpgradeLevel { get; set; } = 1;
        public int UsedVillagers { get; set; } = 0;
        public int MaxVillagersCanUse { get; set; } = 1;

        public Guid BehaviourId { get; } = Guid.NewGuid();

        protected VisualElement rootPanel;
        protected VisualElement behaviourLayoutContainer;
        protected VisualElement buildingLayoutContainer;

        protected Label levelLabel;
        protected Label influenceRatioLabel;

        protected Label villagersCountLabel;
        protected Button btnAddVillager;
        protected Button btnRemoveVillager;

        // THAY ĐỔI: Chuyển btnUpgrade lên đây để class con (MainBuildingBehaviour) có thể chỉnh sửa
        protected Button btnUpgrade;

        protected Dictionary<ResourceType, float> appliedConsumptions = new Dictionary<ResourceType, float>();

        // THÊM: PROPERTY CHO PHÉP OVERRIDE TRẠNG THÁI "BẬN RỘN" (VÍ DỤ: ĐANG SPAWN LÍNH)
        protected virtual bool IsBusy => false;
        protected virtual string BusyReason => "Công trình đang hoạt động, không thể thay đổi nông dân.";

        public BaseBuildingBehaviour(TPreset preset, Vector2Int tilePosition)
        {
            ActualPreset = preset;
            this.TilePosition = tilePosition;
            UpgradeCostRuntime = ActualPreset.DefaultUpgradeCost;

            MaxVillagersCanUse = preset.defaultMaxVillagersCanUse;

            InfluenceRatio.OnValueChanged += HandleInfluenceRatioChanged;
            SbGameplayController.OnActiveBuildingApplyResource += HandleActiveBuildingApplyResource;
        }

        public virtual void Setup()
        {
            if (ActualPreset.requireVillagers)
            {
                int available = SbGameplayController.Instance.VillagerData.RemainingVillagers;
                int toAssign = Mathf.Min(MaxVillagersCanUse, available);
                if (toAssign > 0)
                {
                    int actualReceived = SbGameplayController.Instance.VillagerData.UseVillagers(toAssign);
                    UsedVillagers = actualReceived;
                    UpdateResourceConsumption();
                }
            }

            UpdateVillagerPersistentText();
        }

        private void HandleInfluenceRatioChanged(float newRatio)
        {
            if (influenceRatioLabel != null)
            {
                influenceRatioLabel.text = $"{Mathf.RoundToInt(newRatio * 100)}%";
            }

            RefreshBehaviour();
        }

        public abstract void RefreshBehaviour();

        public virtual void DestroyBehaviour()
        {
            SbGameplayController.OnActiveBuildingApplyResource -= HandleActiveBuildingApplyResource;

            if (UsedVillagers > 0)
            {
                UsedVillagers = 0;
                UpdateResourceConsumption();
            }

            ScreenTextDisplayController.Instance.RemovePersistentText(BehaviourId);
        }

        public void UpgradeBehaviour()
        {
            SbGameplayController.ApplyCost(UpgradeCostRuntime);

            CurrentUpgradeLevel++;
            UpgradeCostRuntime += ActualPreset.IncrementUpgradeCost;

            HandleUpgrade();
            RefreshBehaviour();
            UpdateBuildingLayoutUI();
        }

        protected abstract void HandleUpgrade();

        public virtual BuildingBehaviourSaveData SaveData()
        {
            return new BuildingBehaviourSaveData
            {
                CurrentUpgradeLevel = this.CurrentUpgradeLevel,
                UsedVillagers = this.UsedVillagers,
                MaxVillagersCanUse = this.MaxVillagersCanUse
            };
        }

        public virtual void BindData(BuildingBehaviourSaveData data)
        {
            if (this.UsedVillagers > 0)
            {
                SbGameplayController.Instance.VillagerData.RefundVillagers(this.UsedVillagers);
                this.UsedVillagers = 0;
                UpdateResourceConsumption();
            }

            this.CurrentUpgradeLevel = data.CurrentUpgradeLevel;
            this.UsedVillagers = data.UsedVillagers;

            this.MaxVillagersCanUse = Mathf.Max(data.MaxVillagersCanUse, ActualPreset.defaultMaxVillagersCanUse);

            UpgradeCostRuntime = ActualPreset.DefaultUpgradeCost;
            for (int i = 1; i < CurrentUpgradeLevel; i++)
            {
                UpgradeCostRuntime += ActualPreset.IncrementUpgradeCost;
            }

            UpdateResourceConsumption();

            RefreshBehaviour();
            UpdateBuildingLayoutUI();
        }

        public virtual void AttachUIToPanel(VisualElement root)
        {
            if (rootPanel == null)
            {
                InitializeUI();
            }

            UpdateBuildingLayoutUI();

            SbGameplayController.Instance.VillagerData.CurrentVillagers.OnValueChanged += HandleGlobalVillagersChanged;
            SbGameplayController.Instance.VillagerData.UsedVillagers.OnValueChanged += HandleGlobalVillagersChanged;

            root.Add(rootPanel);
        }

        public virtual void DetachUIFromPanel(VisualElement root)
        {
            if (rootPanel != null && root.Contains(rootPanel))
            {
                SbGameplayController.Instance.VillagerData.CurrentVillagers.OnValueChanged -=
                    HandleGlobalVillagersChanged;
                SbGameplayController.Instance.VillagerData.UsedVillagers.OnValueChanged -= HandleGlobalVillagersChanged;

                root.Remove(rootPanel);
            }
        }

        private void HandleGlobalVillagersChanged(int _)
        {
            UpdateVillagersUIState();
        }

        private void InitializeUI()
        {
            rootPanel = new VisualElement().AddClass("base-root-panel");
            foreach (var styleSheet in ActualPreset.styleSheets)
            {
                rootPanel.styleSheets.Add(styleSheet);
            }

            rootPanel.pickingMode = PickingMode.Ignore;

            var mainContainer = rootPanel.CreateChild("main-container");
            mainContainer.pickingMode = PickingMode.Ignore;

            behaviourLayoutContainer = new VisualElement().AddClass("behaviour-layout-container");
            BuildBehaviourLayoutUI(behaviourLayoutContainer);

            buildingLayoutContainer = new VisualElement().AddClass("building-layout-container");
            BuildBuildingLayoutUI(buildingLayoutContainer);

            if (behaviourLayoutContainer.childCount > 0)
            {
                behaviourLayoutContainer.AddTo(mainContainer);
            }

            buildingLayoutContainer.AddTo(mainContainer);
        }

        protected virtual void BuildBehaviourLayoutUI(VisualElement container)
        {
        }

        private void BuildBuildingLayoutUI(VisualElement container)
        {
            var mainContentRow = container.CreateChild("building-main-content-row");
            var imgElement = mainContentRow.CreateChild("building-image");
            var infoCol = mainContentRow.CreateChild("building-info-col");
            var headerRow = infoCol.CreateChild("building-header-row");
            var titleContainer = headerRow.CreateChild("building-title-container");

            titleContainer.CreateChild(new Label(ActualPreset.buildingId), "building-title");
            levelLabel = titleContainer.CreateChild(new Label($"Lv.{CurrentUpgradeLevel}"), "label-level");

            var ratioContainer = headerRow.CreateChild("ratio-container");
            ratioContainer.CreateChild(new Label("Efficiency:"), "ratio-title");
            influenceRatioLabel =
                ratioContainer.CreateChild(new Label($"{Mathf.RoundToInt(InfluenceRatio.Value * 100)}%"),
                    "ratio-value");

            infoCol.CreateChild(new Label("Building description..."), "building-desc");

            var footerRow = container.CreateChild("building-footer-row");

            var btnDestroy = footerRow.CreateChild(new Button(HandleDestroyButtonClicked) { text = "Destroy" },
                "btn-destroy");

            btnDestroy.RegisterCallback<MouseEnterEvent>(HandleDestroyButtonEnter);
            btnDestroy.RegisterCallback<MouseLeaveEvent>(HandleDestroyButtonLeave);

            var villagersContainer = footerRow.CreateChild("villagers-control-container");

            if (!ActualPreset.requireVillagers)
            {
                villagersContainer.style.display = DisplayStyle.None;
            }

            btnRemoveVillager = villagersContainer.CreateChild(new Button() { text = "-" }, "btn-villager-math");
            btnRemoveVillager.clicked += RemoveVillager;

            btnRemoveVillager.RegisterCallback<MouseEnterEvent>(HandleVillagerButtonEnter);
            btnRemoveVillager.RegisterCallback<MouseLeaveEvent>(HandleVillagerButtonLeave);

            villagersCountLabel = villagersContainer.CreateChild(new Label($"{UsedVillagers}/{MaxVillagersCanUse}"),
                "label-villagers-count");

            btnAddVillager = villagersContainer.CreateChild(new Button() { text = "+" }, "btn-villager-math");
            btnAddVillager.clicked += AddVillager;

            btnAddVillager.RegisterCallback<MouseEnterEvent>(HandleVillagerButtonEnter);
            btnAddVillager.RegisterCallback<MouseLeaveEvent>(HandleVillagerButtonLeave);

            var upgradeContainer = footerRow.CreateChild("upgrade-container");

            // SỬ DỤNG BIẾN protected btnUpgrade ĐÃ KHAI BÁO
            btnUpgrade = upgradeContainer.CreateChild(new Button(HandleUpgradeButtonClicked) { text = "Upgrade" },
                "btn-upgrade");
            btnUpgrade.RegisterCallback<MouseEnterEvent>(HandleMouseEnterUpgradeButton);
            btnUpgrade.RegisterCallback<MouseLeaveEvent>(HandleMouseLeaveUpgradeButton);

            UpdateVillagersUIState();
        }

        private void HandleDestroyButtonClicked()
        {
            if (UsedVillagers > 0)
            {
                SbGameplayController.Instance.VillagerData.RefundVillagers(UsedVillagers);
                UsedVillagers = 0;
                UpdateResourceConsumption();
            }

            ActionCost refundCost = ActualPreset.costBuilding.Data * REFUND_RATIO;
            SbGameplayController.RefundCost(refundCost);

            SbGridMapSystem.Instance.DeleteTile(TilePosition);
            BuildingInfoUIToolkit.Instance.BlurBackground.CloseAll();
        }

        private void HandleDestroyButtonEnter(MouseEnterEvent evt)
        {
            ActionCost refundCost = ActualPreset.costBuilding.Data * REFUND_RATIO;
            string tooltipText =
                $"Refund ({Mathf.RoundToInt(REFUND_RATIO * 100)}%):\n{refundCost.GetTextVertical()}";
            TextTooltipController.Instance.Display(tooltipText, evt.mousePosition + new Vector2(10, -10));
        }

        private void HandleDestroyButtonLeave(MouseLeaveEvent evt)
        {
            TextTooltipController.Instance.Hide();
        }

        private void AddVillager()
        {
            if (!ActualPreset.requireVillagers || IsBusy) return;

            if (UsedVillagers < MaxVillagersCanUse)
            {
                int actualReceived = SbGameplayController.Instance.VillagerData.UseVillagers(1);
                if (actualReceived > 0)
                {
                    UsedVillagers += actualReceived;
                    UpdateResourceConsumption();
                    RefreshBehaviour();
                    UpdateBuildingLayoutUI();
                }
            }
        }

        private void RemoveVillager()
        {
            if (!ActualPreset.requireVillagers || IsBusy) return;

            if (UsedVillagers > 0)
            {
                int actualRefunded = SbGameplayController.Instance.VillagerData.RefundVillagers(1);
                if (actualRefunded > 0)
                {
                    UsedVillagers -= actualRefunded;
                    UpdateResourceConsumption();
                    RefreshBehaviour();
                    UpdateBuildingLayoutUI();
                }
            }
        }

        private void HandleVillagerButtonEnter(MouseEnterEvent evt)
        {
            if (IsBusy)
            {
                TextTooltipController.Instance.Display(BusyReason, evt.mousePosition + new Vector2(10, -10));
            }
        }

        private void HandleVillagerButtonLeave(MouseLeaveEvent evt)
        {
            TextTooltipController.Instance.Hide();
        }

        protected Vector3 GetWorldPosition()
        {
            if (SbGridMapSystem.Instance != null && SbGridMapSystem.Instance.gridTilemap != null)
            {
                return SbGridMapSystem.Instance.gridTilemap.GetCellCenterWorld((Vector3Int)TilePosition);
            }

            return new Vector3(TilePosition.x, TilePosition.y, 0f);
        }

        protected virtual void UpdateVillagerPersistentText()
        {
            if (!ActualPreset.requireVillagers)
            {
                ScreenTextDisplayController.Instance.RemovePersistentText(BehaviourId);
                return;
            }

            Vector3 textPos = GetWorldPosition() + Vector3.up * 0.5f;

            if (UsedVillagers == 0)
            {
                ScreenTextDisplayController.Instance.SetPersistentText(BehaviourId, "< ! >", textPos, Color.red);
            }
            else
            {
                ScreenTextDisplayController.Instance.SetPersistentText(BehaviourId, UsedVillagers.ToString(), textPos,
                    Color.white);
            }
        }

        private void HandleActiveBuildingApplyResource()
        {
            if (ActualPreset.requireVillagers && UsedVillagers <= 0) return;

            List<(string, Color)> displayTexts = GetResourcePopupTexts();
            if (displayTexts == null || displayTexts.Count == 0) return;

            Vector3 worldPos = GetWorldPosition() + Vector3.up * 0.5f;

            DisplayTextsSequentiallyAsync(displayTexts, worldPos).Forget();
        }

        private async UniTaskVoid DisplayTextsSequentiallyAsync(List<(string, Color)> texts, Vector3 worldPos)
        {
            for (int i = 0; i < texts.Count; i++)
            {
                ScreenTextDisplayController.Instance.DisplayText(texts[i].Item1, worldPos, texts[i].Item2);

                if (i < texts.Count - 1)
                {
                    await UniTask.Delay(TimeSpan.FromMilliseconds(300));
                }
            }
        }

        protected virtual List<(string, Color)> GetResourcePopupTexts()
        {
            List<(string, Color)> texts = new();
            var consumedCost = GetConsumedResourcesData();

            if (consumedCost == null || consumedCost.Count == 0) return texts;

            float multiplier = ActualPreset.requireVillagers ? UsedVillagers : 1f;

            foreach (var kvp in consumedCost)
            {
                float totalConsumed = kvp.Value * multiplier;
                if (totalConsumed > 0)
                {
                    texts.Add(($"{-totalConsumed} {kvp.Key}", Color.red));
                }
            }

            return texts;
        }

        protected virtual void UpdateResourceConsumption()
        {
            var consumedCost = GetConsumedResourcesData();
            if (consumedCost == null) return;

            float multiplier = ActualPreset.requireVillagers ? UsedVillagers : 1f;

            foreach (var resourceCost in consumedCost)
            {
                ResourceType type = resourceCost.Key;
                float baseConsumption = resourceCost.Value;

                float targetConsumption = baseConsumption * multiplier;

                appliedConsumptions.TryGetValue(type, out float currentApplied);

                float diff = targetConsumption - currentApplied;

                if (diff != 0)
                {
                    SbGameplayController.Instance.IncrementResources[type].Value -= diff;
                    appliedConsumptions[type] = targetConsumption;
                }
            }
        }

        protected virtual Dictionary<ResourceType, float> GetConsumedResourcesData()
        {
            return ActualPreset.ConsumedResources;
        }

        protected void UpdateVillagersUIState()
        {
            if (villagersCountLabel != null)
                villagersCountLabel.text = $"{UsedVillagers}/{MaxVillagersCanUse}";

            if (btnAddVillager != null)
            {
                bool hasSpace = UsedVillagers < MaxVillagersCanUse;
                bool hasIdleVillagers = SbGameplayController.Instance.VillagerData.RemainingVillagers > 0;
                btnAddVillager.SetEnabled(hasSpace && hasIdleVillagers && !IsBusy);
            }

            if (btnRemoveVillager != null)
            {
                btnRemoveVillager.SetEnabled(UsedVillagers > 0 && !IsBusy);
            }

            UpdateVillagerPersistentText();
        }

        protected virtual void UpdateBuildingLayoutUI()
        {
            if (levelLabel != null) levelLabel.text = $"Lv.{CurrentUpgradeLevel}";
            if (influenceRatioLabel != null)
                influenceRatioLabel.text = $"{Mathf.RoundToInt(InfluenceRatio.Value * 100)}%";

            UpdateVillagersUIState();
        }

        private void HandleUpgradeButtonClicked()
        {
            if (!SbGameplayController.ValidateCost(UpgradeCostRuntime)) return;
            UpgradeBehaviour();
        }

        // THAY ĐỔI: Chuyển thành protected virtual để class con override
        protected virtual void HandleMouseEnterUpgradeButton(MouseEnterEvent evt)
        {
            TextTooltipController.Instance.Display(UpgradeCostRuntime.GetTextVertical(),
                evt.mousePosition + new Vector2(10, -10));
        }

        // THAY ĐỔI: Chuyển thành protected virtual
        protected virtual void HandleMouseLeaveUpgradeButton(MouseLeaveEvent evt)
        {
            TextTooltipController.Instance.Hide();
        }

        public Vector2Int TilePosition { get; }
        public ObservableValue<float> InfluenceRatio { get; set; } = new(1f);
        public Dictionary<Vector2Int, ITileInfluencer> TileInfluencers { get; } = new();
    }
}