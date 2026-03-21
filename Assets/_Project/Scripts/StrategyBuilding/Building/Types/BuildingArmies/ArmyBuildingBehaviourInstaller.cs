using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TnieYuPackage.GlobalExtensions;
using UnityEngine.UIElements;

namespace Game.StrategyBuilding
{
    [Serializable]
    public class ArmyBuildingBehaviourInstaller : BaseBuildingBehaviourInstaller
    {
        public float defaultGenerateRate;
        public float defaultGenerateRateIncrease;
        public int defaultMaxSlotGenerate;

        public float convenientRatePerTile;
        public float adverseRatePerTile;

        protected override IBuildingBehaviour Create()
        {
            return new ArmyBuildingBehaviour(this);
        }

        protected override void OnInit(BuildingRuntime buildingRuntime)
        {
        }

        protected override void OnDestroy(BuildingRuntime buildingRuntime)
        {
        }
    }

    public class ArmySpawnProgress
    {
        private const float SPAWN_RELOAD = 1f;

        public readonly ArmyTypePresetSo ArmyTypePreset;
        public Action<ArmySpawnProgress> OnCompleted;
        public Action<float> OnProgress;

        private float progress = 0;
        private readonly float totalMaxProgress;

        public float Value => progress / totalMaxProgress;

        public ArmySpawnProgress(ArmyTypePresetSo armyTypePreset, float generateRate)
        {
            this.ArmyTypePreset = armyTypePreset;
            this.totalMaxProgress = ArmyTypePreset.delaySpawn / generateRate;
        }

        public async UniTaskVoid Spawn()
        {
            ArmyTypePreset.cost.CollectCost();

            while (progress < totalMaxProgress)
            {
                //handle
                await UniTask.WaitForSeconds(SPAWN_RELOAD);
                progress += SPAWN_RELOAD;
                OnProgress?.Invoke(Value);
            }

            ArmyTypePreset.Generate();
            OnCompleted?.Invoke(this);
        }
    }

    [Serializable]
    public class ArmyBuildingBehaviour : BaseBuildingBehaviour
    {
        public float generateRate;
        private readonly float generateRateIncrease;
        private readonly int maxSlotGenerate;
        private readonly List<ArmySpawnSlot> spawnSlots;

        private readonly float convenientRatePerTile;
        private readonly float adverseRatePerTile;

        private Action<float> onRateChanged;

        public class ArmySpawnSlot
        {
            public Action<ArmySpawnProgress> OnArmyChanged;

            private ArmySpawnProgress currentProgress;

            public ArmySpawnProgress CurrentProgress
            {
                get => currentProgress;
                set
                {
                    currentProgress = value;

                    OnArmyChanged?.Invoke(currentProgress);
                }
            }
        }

        public ArmyBuildingBehaviour(ArmyBuildingBehaviourInstaller installer) : base(installer)
        {
            generateRate = installer.defaultGenerateRate;
            generateRateIncrease = installer.defaultGenerateRateIncrease;
            maxSlotGenerate = installer.defaultMaxSlotGenerate;

            spawnSlots = new(maxSlotGenerate);
            for (int i = 0; i < maxSlotGenerate; i++)
            {
                spawnSlots.Add(new ArmySpawnSlot());
            }

            convenientRatePerTile = installer.convenientRatePerTile;
            adverseRatePerTile = installer.adverseRatePerTile;
        }

        internal readonly struct ItemSlotHandlerData
        {
            public ArmyBuildingBehaviour ArmyBuildingBehaviour { get; }
            public ArmySlotItem ArmySlotItem { get; }
            public ArmySpawnSlot Data { get; }
            public int Index { get; }

            public ItemSlotHandlerData(ArmyBuildingBehaviour armyBuildingBehaviour, ArmySlotItem armySlotItem,
                ArmySpawnSlot data, int index)
            {
                ArmyBuildingBehaviour = armyBuildingBehaviour;
                ArmySlotItem = armySlotItem;
                Data = data;
                Index = index;
            }

            public void HandleItemClicked(ClickEvent evt)
            {
                ArmyTypeListUIToolkit.Instance.Display(ArmyBuildingBehaviour, Index);
            }

            public void HandleProgressChanged(float value)
            {
                ArmySlotItem.ProgressBar.value = value;
            }

            public void HandleProgressDetach(DetachFromPanelEvent _)
            {
                if (Data.CurrentProgress == null) return;

                Data.CurrentProgress.OnProgress -= HandleProgressChanged;
            }
        }

        internal readonly struct ArmyHandlerData
        {
            public ItemSlotHandlerData ItemSlotHandlerData { get; }

            public ArmyHandlerData(ItemSlotHandlerData itemSlotHandlerData)
            {
                ItemSlotHandlerData = itemSlotHandlerData;
            }

            public void HandleArmyChanged(ArmySpawnProgress armyProgress)
            {
                RefreshItemSlot(
                    ItemSlotHandlerData.ArmySlotItem,
                    armyProgress,
                    ItemSlotHandlerData.HandleItemClicked,
                    ItemSlotHandlerData.HandleProgressChanged,
                    ItemSlotHandlerData.HandleProgressDetach
                );
            }

            public void HandleDetach(DetachFromPanelEvent _)
            {
                ItemSlotHandlerData.Data.OnArmyChanged -= HandleArmyChanged;
            }
        }

        protected override void HandleUpgrade()
        {
            generateRate += generateRateIncrease;
            onRateChanged?.Invoke(generateRate);
        }

        protected override void RenderContent(VisualElement content)
        {
            //state & list click => choose spawn.
            var leftContainer = content.CreateChild<VisualElement>("content__left-container");
            var propertiesText = leftContainer.CreateChild<Label>("content_left-container-text");
            propertiesText.text = GetPropertiesText(generateRate);
            Action<float> rateChangedHandler =
                value => propertiesText.text = GetPropertiesText(value);
            onRateChanged += rateChangedHandler;
            propertiesText.RegisterCallback<DetachFromPanelEvent>(_ => onRateChanged -= rateChangedHandler);

            var rightContainer = content.CreateChild<VisualElement>("content__right-container");
            var listText = rightContainer.CreateChild<Label>("content__right-container__name");
            listText.text = "Army Spawn Slots";

            var slotList = rightContainer.CreateChild<ListView>("content__right-container__list");
            slotList.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            // slotList.fixedItemHeight = 78;

            slotList.itemsSource = spawnSlots;

            slotList.makeItem = MakeItemSlot;
            slotList.bindItem = BindItemSlot;
        }

        private string GetPropertiesText(float rate)
        {
            return $"Properties:\n" +
                   $"-SpeedRate: {rate}\n" +
                   $"-ConvenientRate/Tile: {convenientRatePerTile} - Count: {ConvenientTiles.Count}\n" +
                   $"-AdverseRate/Tile: {adverseRatePerTile} - Count: {AdverseTiles.Count}\n" +
                   $"Total Rate: {GetTotalRate()}";
        }

        private VisualElement MakeItemSlot()
        {
            return new ArmySlotItem();
        }

        private void BindItemSlot(VisualElement element, int index)
        {
            var armySlotItem = element as ArmySlotItem;
            if (armySlotItem == null) return;
            var data = spawnSlots[index];

            #region OLD CODE

            //GC risk: using capture delegate

            // //occur: only when progressSpawn is null
            // EventCallback<ClickEvent> itemClicked =
            //     evt => ArmyTypeListUIToolkit.Instance.Display(this, index);
            // Action<float> progressHandler =
            //     progressValue => armySlotItem.ProgressBar.value = progressValue;
            // //occur: only when progressSpawn exist && progressBarUI detach
            // EventCallback<DetachFromPanelEvent> progressDetachHandler =
            //     _ => data.CurrentProgress.OnProgress -= progressHandler;
            //
            // RefreshItemSlot(
            //     armySlotItem,
            //     data.CurrentProgress,
            //     itemClicked,
            //     progressHandler,
            //     progressDetachHandler
            // );
            // Action<ArmySpawnProgress> onArmyChanged = armyProgress =>
            //     RefreshItemSlot(
            //         armySlotItem,
            //         armyProgress,
            //         itemClicked,
            //         progressHandler,
            //         progressDetachHandler
            //     );
            //
            // data.OnArmyChanged += onArmyChanged;
            // armySlotItem.RegisterCallback<DetachFromPanelEvent>(_ => data.OnArmyChanged -= onArmyChanged);

            #endregion

            //safe: using extract struct + static extract

            var handlerData = new ItemSlotHandlerData(this, armySlotItem, data, index);

            RefreshItemSlot(
                handlerData.ArmySlotItem,
                handlerData.Data.CurrentProgress,
                handlerData.HandleItemClicked,
                handlerData.HandleProgressChanged,
                handlerData.HandleProgressDetach
            );

            var armyHandlerData = new ArmyHandlerData(handlerData);

            data.OnArmyChanged += armyHandlerData.HandleArmyChanged;
            armySlotItem.UnregisterCallback<DetachFromPanelEvent>(armyHandlerData.HandleDetach);
        }

        private static void RefreshItemSlot(
            ArmySlotItem armySlotItem, ArmySpawnProgress armySpawnProgress,
            EventCallback<ClickEvent> itemClickEvent, Action<float> onProgress,
            EventCallback<DetachFromPanelEvent> progressBarDetachEvent
        )
        {
            //refactor: need to separate to State-Machine pattern
            if (armySpawnProgress == null)
            {
                armySlotItem.DisplayImage.sprite = null; //default
                armySlotItem.ProgressBar.value = 0;

                armySlotItem.RegisterCallback(itemClickEvent);
                armySlotItem.ProgressBar.UnregisterCallback(progressBarDetachEvent);
            }
            else
            {
                armySlotItem.UnregisterCallback(itemClickEvent);
                armySlotItem.ProgressBar.RegisterCallback(progressBarDetachEvent);

                armySlotItem.DisplayImage.sprite = armySpawnProgress.ArmyTypePreset.icon;
                armySlotItem.ProgressBar.value = armySpawnProgress.Value;
                armySpawnProgress.OnProgress += onProgress;
            }
        }

        private float GetTotalRate()
        {
            return generateRate
                   + convenientRatePerTile * ConvenientTiles.Count
                   - adverseRatePerTile * AdverseTiles.Count;
        }

        public void SetGeneratedArmy(ArmyTypePresetSo armyType, int slotIndex)
        {
            var spawnSlot = spawnSlots[slotIndex];
            var spawnProcess = new ArmySpawnProgress(armyType, GetTotalRate())
            {
                OnCompleted = _ => spawnSlot.CurrentProgress = null
            };

            spawnSlot.CurrentProgress = spawnProcess;

            spawnProcess.Spawn().Forget();
        }
    }

    internal class ArmySlotItem : VisualElement
    {
        public readonly Image DisplayImage;
        public readonly ProgressBar ProgressBar;

        public ArmySlotItem()
        {
            this.AddClass("item-slot-item-wrapper");
            var container = this.CreateChild("army-slot-item");
            DisplayImage = container.CreateChild<Image>("army-slot-item__display");
            ProgressBar = container.CreateChild<ProgressBar>("army-slot-item__progress-bar");
            ProgressBar.lowValue = 0;
            ProgressBar.highValue = 1;
        }
    }
}