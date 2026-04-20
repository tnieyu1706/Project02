using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.BaseGameplay.Strategies
{
    // current: only apply for Tower
    public abstract class TowerSpawnInteractStrategyInstaller : IBaseObjectInteractStrategyInstaller
    {
        [Header("General Spawn Settings")] public SoldierPresetSo soldierPreset;
        public int maxSpawnQuantity = 3;
        public float spawnCooldown = 5f;
    }

    public abstract class TowerSpawnInteractStrategy<TInstaller> : BaseObjectInteractStrategy<TInstaller>
        where TInstaller : TowerSpawnInteractStrategyInstaller
    {
        // Dùng mảng để giữ "Slot" của đội hình, tránh lính spawn sau đè lên slot lính đang sống
        protected SoldierRuntime[] ActiveSoldiers;

        public TowerSpawnInteractStrategy(TInstaller installer) : base(installer)
        {
        }

        public override void OnInitBehaviour(IBaseObjectRuntime runtime)
        {
            base.OnInitBehaviour(runtime);

            ActiveSoldiers = new SoldierRuntime[ActualInstaller.maxSpawnQuantity];
            OnDefaultSetup();
        }

        protected virtual void OnDefaultSetup()
        {
            for (int i = 0; i < ActualInstaller.maxSpawnQuantity; i++)
            {
                SpawnSoldierWithFreeSlot();
            }

            CanUse = false;
        }

        public override void OnDestroyBehaviour()
        {
            base.OnDestroyBehaviour();

            // 2. Thu hồi toàn bộ lính vào ObjectPool khi Tower bị nâng cấp / bán
            if (ActiveSoldiers != null)
            {
                for (int i = 0; i < ActiveSoldiers.Length; i++)
                {
                    if (ActiveSoldiers[i] != null)
                    {
                        // Tránh memory leak bằng cách gỡ event trước khi cho vào Pool
                        ActiveSoldiers[i].OnSoldierDeadEvent -= HandleSoldierDead;
                        BaseGameplayPrefabSpawnManager.Instance.PoolTrackers[PrefabType.BaseSoldier]
                            .Release(ActiveSoldiers[i].gameObject);
                        ActiveSoldiers[i] = null;
                    }
                }
            }
        }

        public override bool TrackTarget(Vector3 position, out IObjectInteractable target)
        {
            target = null;
            return true;
        }

        public override void Interact(IObjectInteractable interactable)
        {
            SpawnSoldierWithFreeSlot();
        }

        private void SpawnSoldierWithFreeSlot()
        {
            int freeSlot = GetFreeSlotIndex();
            if (freeSlot == -1) // max capacity
            {
                // when spawn reach max capacity CanUse always false, only true when one soldier death.
                // prevent calling meaningless after. (when max capacity -> freeSlot == -1 always)
                CanUse = false;
                return;
            }

            ExecuteSpawnSequence(OwnerRuntime.CurrentPosition, freeSlot).Forget();
        }

        private int GetFreeSlotIndex()
        {
            for (int i = 0; i < ActiveSoldiers.Length; i++)
            {
                if (ActiveSoldiers[i] == null) return i;
            }

            return -1;
        }

        private UniTask? waitForSpawnCooldownTask = null;

        private async UniTaskVoid ExecuteSpawnSequence(Vector3 spawnCenterPosition, int slotIndex)
        {
            CanUse = false;

            var soldierObj = BaseGameplayPrefabSpawnManager.Instance.PoolTrackers[PrefabType.BaseSoldier].Get();
            soldierObj.transform.position = this.OwnerRuntime.CurrentPosition;
            if (soldierObj.TryGetComponent<SoldierRuntime>(out var soldier))
            {
                soldier.OnSoldierDeadEvent += HandleSoldierDead;
                soldier.Setup(ActualInstaller.soldierPreset);

                // Đăng ký lính vào slot
                ActiveSoldiers[slotIndex] = soldier;
                DispatchSoldier(soldier, slotIndex);
            }

            try
            {
                // Delay có sử dụng CancellationToken
                waitForSpawnCooldownTask =
                    UniTask.Delay(TimeSpan.FromSeconds(ActualInstaller.spawnCooldown), cancellationToken: cts.Token);
                await waitForSpawnCooldownTask.Value;
            }
            catch (OperationCanceledException)
            {
                /* Bỏ qua lỗi an toàn khi Cancel */
            }
            finally
            {
                waitForSpawnCooldownTask = null;
                CanUse = true;
            }
        }

        protected abstract void DispatchSoldier(SoldierRuntime soldier, int soldierIndex);

        private async void HandleSoldierDead(SoldierRuntime deadSoldier)
        {
            deadSoldier.OnSoldierDeadEvent -= HandleSoldierDead;

            // check before: ensure current situation is max capacity.
            bool isMaxCapacity = GetFreeSlotIndex() == -1;

            // Xóa lính khỏi mảng để giải phóng đúng cái slot đó
            for (int i = 0; i < ActiveSoldiers.Length; i++)
            {
                if (ActiveSoldiers[i] == deadSoldier)
                {
                    ActiveSoldiers[i] = null;
                    break;
                }
            }

            if (!isMaxCapacity) return;

            if (waitForSpawnCooldownTask != null)
                await waitForSpawnCooldownTask.Value;
            else
                await UniTask.Delay(TimeSpan.FromSeconds(ActualInstaller.spawnCooldown),
                    cancellationToken: cts.Token);

            CanUse = true;
        }
    }
}