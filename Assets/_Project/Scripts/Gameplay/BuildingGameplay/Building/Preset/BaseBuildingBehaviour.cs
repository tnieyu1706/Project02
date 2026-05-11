using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Global.Tooltip;
using Cysharp.Threading.Tasks;
using Game.BuildingGameplay;
using Reflex.Attributes;
using SoundSystem.Core;
using System.Threading; // THÊM THƯ VIỆN NÀY ĐỂ DÙNG CancellationTokenSource
using TnieYuPackage.GlobalExtensions;
using TnieYuPackage.UI;
using TnieYuPackage.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.StrategyBuilding
{
    [Serializable]
    public abstract class BaseBuildingBehaviour<TPreset> : IBuildingBehaviour<TPreset>
        where TPreset : BuildingPresetSo
    {
        public const float REFUND_RATIO = 0.8f;

        [Inject] protected SfxManager SfxManager;

        public TPreset ActualPreset { get; }
        public ActionCost UpgradeCostRuntime { get; private set; }

        public int CurrentUpgradeLevel { get; set; } = 1;
        public int UsedVillagers { get; set; } = 0;
        public int MaxVillagersCanUse { get; set; } = 1;

        public Guid BehaviourId { get; } = Guid.NewGuid();
        public Guid ConstructionTextId { get; } = Guid.NewGuid();

        // THÊM: CancellationTokenSource để quản lý lifecycle của các UniTask
        protected CancellationTokenSource behaviourCts = new CancellationTokenSource();

        // THÊM: Các biến quản lý xây dựng
        public bool IsUnderConstruction { get; protected set; }
        public float RemainingBuildTime { get; protected set; }

        protected VisualElement rootPanel;
        protected VisualElement behaviourLayoutContainer;
        protected VisualElement buildingLayoutContainer;

        protected Label levelLabel;
        protected Label influenceRatioLabel;

        protected Label villagersCountLabel;
        protected Button btnAddVillager;
        protected Button btnRemoveVillager;

        protected Button btnUpgrade;

        protected Dictionary<ResourceType, float> appliedConsumptions = new Dictionary<ResourceType, float>();

        // THAY ĐỔI: Chặn thao tác nếu đang bận HOẶC đang xây dựng
        protected virtual bool IsBusy => IsUnderConstruction;

        protected virtual string BusyReason => IsUnderConstruction
            ? $"Đang xây dựng... ({Mathf.CeilToInt(RemainingBuildTime)}s)"
            : "Công trình đang hoạt động, không thể thay đổi nông dân.";

        public BaseBuildingBehaviour(TPreset preset, Vector2Int tilePosition)
        {
            ActualPreset = preset;
            this.TilePosition = tilePosition;
            UpgradeCostRuntime = ActualPreset.DefaultUpgradeCost;

            MaxVillagersCanUse = preset.defaultMaxVillagersCanUse;

            // Khởi tạo trạng thái xây dựng
            IsUnderConstruction = preset.buildWaitingTime > 0;
            RemainingBuildTime = preset.buildWaitingTime;

            InfluenceRatio.OnValueChanged += HandleInfluenceRatioChanged;
            SbGameplayController.OnActiveBuildingApplyResource += HandleActiveBuildingApplyResource;
        }

        public virtual void Setup()
        {
            if (IsUnderConstruction)
            {
                // Bắt đầu đếm ngược xây dựng, truyền CancellationToken vào
                ConstructionRoutine(behaviourCts.Token).Forget();
            }
            else
            {
                // Nếu không cần xây, hoàn thành luôn
                CompleteConstruction();
            }
        }

        private async UniTaskVoid ConstructionRoutine(CancellationToken token)
        {
            UpdateConstructionPersistentText();

            while (RemainingBuildTime > 0)
            {
                // Đếm ngược từng giây và tự động huỷ nếu nhận được token cancel
                bool isCanceled = await UniTask.Delay(1000, delayType: DelayType.DeltaTime, cancellationToken: token)
                    .SuppressCancellationThrow();   

                // Nếu Task bị huỷ (do công trình bị xoá), thoát luôn vòng lặp
                if (isCanceled) return;

                RemainingBuildTime -= 1f;

                if (RemainingBuildTime > 0)
                {
                    UpdateConstructionPersistentText();
                }

                // Cập nhật UI để thấy thời gian giảm
                if (rootPanel != null) UpdateBuildingLayoutUI();
            }

            ScreenTextDisplayController.Instance.RemovePersistentText(ConstructionTextId);
            CompleteConstruction();
        }

        protected virtual void UpdateConstructionPersistentText()
        {
            if (!IsUnderConstruction) return;

            // Lấy vị trí trên đỉnh công trình
            Vector3 textPos = GetWorldPosition() + Vector3.up * 0.5f;

            // Hiển thị Persistent Text màu vàng cùng icon (nếu dùng font hỗ trợ Emoji/Kí tự)
            ScreenTextDisplayController.Instance.SetPersistentText(ConstructionTextId,
                $"⏳ {Mathf.CeilToInt(RemainingBuildTime)}s", textPos, Color.yellow);
        }

        private void CompleteConstruction()
        {
            IsUnderConstruction = false;
            RemainingBuildTime = 0;

            // Chạy logic Setup gốc (gán nông dân...)
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

            // GỌI MAP SYSTEM TÍNH LẠI INFLUENCE KHI XÂY XONG
            if (SbGridMapSystem.HasInstance)
            {
                SbGridMapSystem.Instance.UpdateInfluenceForTile(TilePosition);
            }

            RefreshBehaviour();
            if (rootPanel != null) UpdateBuildingLayoutUI();
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
            // THÊM: Huỷ tất cả các UniTask đang chạy ngầm của Behaviour này
            behaviourCts?.Cancel();
            behaviourCts?.Dispose();

            SbGameplayController.OnActiveBuildingApplyResource -= HandleActiveBuildingApplyResource;

            if (UsedVillagers > 0)
            {
                UsedVillagers = 0;
                UpdateResourceConsumption();
            }

            ScreenTextDisplayController.Instance.RemovePersistentText(BehaviourId);
            ScreenTextDisplayController.Instance.RemovePersistentText(ConstructionTextId);
        }

        public void UpgradeBehaviour()
        {
            if (IsUnderConstruction) return;

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
                MaxVillagersCanUse = this.MaxVillagersCanUse,
                IsUnderConstruction = this.IsUnderConstruction,
                RemainingBuildTime = this.RemainingBuildTime
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

            // Phục hồi trạng thái xây dựng
            this.IsUnderConstruction = data.IsUnderConstruction;
            this.RemainingBuildTime = data.RemainingBuildTime;

            // Bổ sung: Tiếp tục đếm ngược nếu Load Game mà công trình vẫn đang xây
            if (this.IsUnderConstruction)
            {
                ConstructionRoutine(behaviourCts.Token).Forget();
            }

            UpgradeCostRuntime = ActualPreset.DefaultUpgradeCost;
            for (int i = 1; i < CurrentUpgradeLevel; i++)
            {
                UpgradeCostRuntime += ActualPreset.IncrementUpgradeCost;
            }

            UpdateResourceConsumption();
            RefreshBehaviour();

            if (rootPanel != null) UpdateBuildingLayoutUI();
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
            string tooltipText = $"Refund ({Mathf.RoundToInt(REFUND_RATIO * 100)}%):\n{refundCost.GetTextVertical()}";
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
            if (SbGridMapSystem.HasInstance && SbGridMapSystem.Instance.gridTilemap != null)
            {
                return SbGridMapSystem.Instance.gridTilemap.GetCellCenterWorld((Vector3Int)TilePosition);
            }

            return new Vector3(TilePosition.x, TilePosition.y, 0f);
        }

        protected virtual void UpdateVillagerPersistentText()
        {
            if (!ActualPreset.requireVillagers || IsUnderConstruction) // Ẩn text nếu đang xây
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
            if (IsUnderConstruction) return; // Không sinh tài nguyên nếu đang xây
            if (ActualPreset.requireVillagers && UsedVillagers <= 0) return;

            List<(string, Color)> displayTexts = GetResourcePopupTexts();
            if (displayTexts == null || displayTexts.Count == 0) return;

            Vector3 worldPos = GetWorldPosition() + Vector3.up * 0.5f;

            DisplayTextsSequentiallyAsync(displayTexts, worldPos, behaviourCts.Token).Forget();

            SubHandleActiveBuildingApplyResource();
        }

        protected virtual void SubHandleActiveBuildingApplyResource()
        {
        }

        private async UniTaskVoid DisplayTextsSequentiallyAsync(List<(string, Color)> texts, Vector3 worldPos,
            CancellationToken token)
        {
            for (int i = 0; i < texts.Count; i++)
            {
                if (token.IsCancellationRequested) return;

                ScreenTextDisplayController.Instance.DisplayText(texts[i].Item1, worldPos, texts[i].Item2);

                if (i < texts.Count - 1)
                {
                    bool isCanceled = await UniTask.Delay(TimeSpan.FromMilliseconds(300), cancellationToken: token)
                        .SuppressCancellationThrow();
                    if (isCanceled) return;
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
                    texts.Add(($"-{totalConsumed:F1} {kvp.Key}", Color.red));
                }
            }

            return texts;
        }

        protected virtual void UpdateResourceConsumption()
        {
            if (IsUnderConstruction) return;

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

            if (btnUpgrade != null)
            {
                btnUpgrade.SetEnabled(!IsBusy);
            }

            UpdateVillagerPersistentText();
        }

        protected virtual void UpdateBuildingLayoutUI()
        {
            if (levelLabel != null)
            {
                levelLabel.text = IsUnderConstruction
                    ? $"Xây dựng: {Mathf.CeilToInt(RemainingBuildTime)}s"
                    : $"Lv.{CurrentUpgradeLevel}";
                levelLabel.style.color = IsUnderConstruction ? Color.yellow : Color.white;
            }

            if (influenceRatioLabel != null)
                influenceRatioLabel.text = $"{Mathf.RoundToInt(InfluenceRatio.Value * 100)}%";

            UpdateVillagersUIState();
        }

        private void HandleUpgradeButtonClicked()
        {
            if (IsUnderConstruction) return;
            if (!SbGameplayController.ValidateCost(UpgradeCostRuntime)) return;
            UpgradeBehaviour();
        }

        protected virtual void HandleMouseEnterUpgradeButton(MouseEnterEvent evt)
        {
            if (IsUnderConstruction) return;
            TextTooltipController.Instance.Display(UpgradeCostRuntime.GetTextVertical(),
                evt.mousePosition + new Vector2(10, -10));
        }

        protected virtual void HandleMouseLeaveUpgradeButton(MouseLeaveEvent evt)
        {
            TextTooltipController.Instance.Hide();
        }

        public Vector2Int TilePosition { get; }
        public ObservableValue<float> InfluenceRatio { get; set; } = new(1f);
        public Dictionary<Vector2Int, ITileInfluencer> TileInfluencers { get; } = new();
    }
}