using System;
using System.Globalization;
using Cysharp.Threading.Tasks;
using TnieYuPackage.Handlers;
using UnityEngine;

namespace Game.BaseGameplay
{
    [RequireComponent(typeof(Animator))]
    public class SoldierRuntime : EntityRuntime<SoldierPresetSo>, IBaseObjectInfoProvider
    {
        // Thêm Event để Strategy có thể lắng nghe
        public event Action<SoldierRuntime> OnSoldierDeadEvent;

        protected override void HandleEntityDead()
        {
            // Gọi event ngay khi lính chết để Strategy cập nhật số lượng
            OnSoldierDeadEvent?.Invoke(this);

            EventManager.Instance.RegistryDelay(HandleSoldierDead, BaseConstant.ENTITY_DEAD_DELAY).Forget();
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

        #region IBaseObjectInfoProvider Implementation

        public string GetObjectName()
        {
            return currentPreset != null ? currentPreset.objectId : "Soldier";
        }

        public System.Collections.Generic.Dictionary<BaseObjPropertyType, string> GetObjectInfo()
        {
            var info = new System.Collections.Generic.Dictionary<BaseObjPropertyType, string>();

            if (currentPreset == null) return info;

            info.Add(BaseObjPropertyType.Type, currentPreset.armyType.ToString());
            info.Add(BaseObjPropertyType.Health, $"{Hp.Value}/{currentPreset.maxHp}");
            info.Add(BaseObjPropertyType.Defense, currentPreset.def.ToString(CultureInfo.InvariantCulture));

            // Lấy thêm Attack, AttackSpeed từ Behaviour/Strategy
            foreach (var strategy in InteractStrategyList)
            {
                if (strategy is IStrategyInfoProvider strategyInfo)
                {
                    strategyInfo.AppendStrategyInfo(info);
                }
            }

            return info;
        }

        #endregion
    }
}