using System;
using TnieYuPackage.Core;
using UnityEngine;

namespace Game.BaseGameplay
{
    [RequireComponent(typeof(Animator))]
    public class SoldierRuntime : EntityRuntime<SoldierPresetSo>
    {
        // Thêm Event để Strategy có thể lắng nghe
        public event Action<SoldierRuntime> OnSoldierDeadEvent;

        protected override void HandleEntityDead()
        {
            // Gọi event ngay khi lính chết để Strategy cập nhật số lượng
            OnSoldierDeadEvent?.Invoke(this);
            
            EventManager.Instance.RegistryDelay(HandleSoldierDead, BaseConstant.ENTITY_DEAD_DELAY);
        }

        private void HandleSoldierDead()
        {
            // Xóa hết listener để tránh memory leak khi tái sử dụng từ Pool
            OnSoldierDeadEvent = null;
            
            BaseGameplayPrefabSpawnManager.Instance.PoolTrackers[PrefabType.BaseSoldier].Release(gameObject);
        }

        public void Setup(SoldierPresetSo soldierPreset)
        {
            SetPreset(soldierPreset);
        }
    }
}