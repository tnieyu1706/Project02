using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TnieYuPackage.GlobalExtensions;
using UnityEngine.UIElements;

namespace Game.BuildingGameplay
{
    public static class SpawnProgressionController
    {
        private const float SPAWN_RELOAD = 0.5f;

        public static async UniTaskVoid Process(float progressEndTime, Action<float> onProgress, Action onCompleted,
            CancellationToken cancellationToken)
        {
            float progress = 0;

            try
            {
                while (progress < progressEndTime)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(SPAWN_RELOAD), cancellationToken: cancellationToken);
                    progress += SPAWN_RELOAD;
                    onProgress?.Invoke(progress / progressEndTime);
                }

                if (!cancellationToken.IsCancellationRequested)
                {
                    onCompleted?.Invoke();
                }
            }
            catch (OperationCanceledException)
            {
                // Process was cancelled safely, do nothing.
            }
        }
    }

    internal class ArmySlotItem : VisualElement
    {
        private const float ARMY_REFUND_PERCENT = 0.8f;

        private readonly VisualElement container;
        private readonly Image displayImage;
        private readonly ProgressBar progressBar;
        private readonly Button cancelButton;
        private readonly ArmyBuildingBehaviour behaviour;

        private ArmyTypePresetSo armyPresetTemp;
        private CancellationTokenSource cts;

        // BỔ SUNG: Event để báo cho Behaviour biết khi trạng thái sinh lính thay đổi
        public event Action OnSpawnStateChanged;

        // Cờ xác định slot này có đang sinh lính hay không
        public bool IsSpawning => cts != null;

        public ArmySlotItem(ArmyBuildingBehaviour behaviour)
        {
            this.behaviour = behaviour;

            this.AddClass("army-slot-wrapper");
            container = this.CreateChild("army-slot-container");

            displayImage = container.CreateChild<Image>("army-slot-icon");

            progressBar = container.CreateChild(new ProgressBar(), "army-slot-progress");
            progressBar.lowValue = 0;
            progressBar.highValue = 1;

            cancelButton = container.CreateChild<Button>("army-slot-cancel-btn");
            cancelButton.text = "X";
            cancelButton.clicked += CancelSpawn;
            cancelButton.RegisterCallback<ClickEvent>(evt => evt.StopPropagation());

            ResetItem();

            container.RegisterCallback<ClickEvent>(HandleContainerClicked);
        }

        private void ResetItem()
        {
            displayImage.sprite = null;
            progressBar.value = 0;
            cancelButton.style.display = DisplayStyle.None;
        }

        private async void HandleContainerClicked(ClickEvent evt)
        {
            if (cts != null) return;

            var totalGenerateRate = behaviour.GetTotalRate();
            if (totalGenerateRate <= 0) return;

            armyPresetTemp = await ArmyTypeListUIToolkit.Instance.DisplayAndWait();
            if (armyPresetTemp == null) return;

            // Apply cost directly 100% when starting
            if (armyPresetTemp.cost.Data != null)
            {
                SbGameplayController.ApplyCost(armyPresetTemp.cost.Data);
            }

            displayImage.sprite = armyPresetTemp.icon;
            progressBar.value = 0;
            cancelButton.style.display = DisplayStyle.Flex;

            cts = new CancellationTokenSource();

            // Trigger event khoá nút +/- Nông Dân
            OnSpawnStateChanged?.Invoke();

            SpawnProgressionController.Process(
                armyPresetTemp.delaySpawn / totalGenerateRate,
                OnProgressBarChangedValue,
                OnProgressBarCompleted,
                cts.Token
            ).Forget();
        }

        private void OnProgressBarChangedValue(float changedValue)
        {
            progressBar.value = changedValue;
        }

        private void OnProgressBarCompleted()
        {
            DisposeToken();
            ResetItem();
            SbGameplayController.AddArmy(armyPresetTemp.armyType, 1);

            // Hoàn tất, trigger event để kiểm tra mở lại nút Nông Dân
            OnSpawnStateChanged?.Invoke();
        }

        public void CancelSpawn()
        {
            if (cts != null && !cts.IsCancellationRequested)
            {
                cts.Cancel();
                DisposeToken();

                if (armyPresetTemp != null && armyPresetTemp.cost.Data != null)
                {
                    SbGameplayController.RefundCost(armyPresetTemp.cost.Data * ARMY_REFUND_PERCENT);
                }

                ResetItem();

                // Huỷ bỏ, trigger event để kiểm tra mở lại nút Nông Dân
                OnSpawnStateChanged?.Invoke();
            }
        }

        private void DisposeToken()
        {
            if (cts != null)
            {
                cts.Dispose();
                cts = null;
            }
        }
    }
}