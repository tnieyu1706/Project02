using System.Collections.Generic;
using Reflex.Attributes;
using UnityEngine;

namespace Game.BaseGameplay
{
    [RequireComponent(typeof(Animator))]
    public class TowerRuntime : BaseObjectRuntime<TowerPresetSo>, IBaseObjectInfoProvider
    {
        [Inject] TowerUpgradeTree towerUpgradeTree;

        /// <summary>
        /// score == number paths this tower nest with. set in editor each tower.
        /// </summary>
        public int towerScore;

        void Start()
        {
            if (!towerUpgradeTree.Tree.TryGetValue(BaseConstant.TOWER_EMPTY_ID, out TowerUpgradeTreeNode node))
            {
                Debug.LogError($"TowerUpgradeTree does not contain empty tower id: {BaseConstant.TOWER_EMPTY_ID}");
                return;
            }

            SetPreset(node.towerPreset);
        }

        public void Setup(TowerPresetSo preset)
        {
            BaseGameplayController.Instance.money.Value -=
                TowerPresetSo.CalculateCost(currentPreset, preset);
            SetPreset(preset);
        }

        #region IBaseObjectInfoProvider Implementation

        public string GetObjectName()
        {
            return currentPreset != null ? currentPreset.objectId : "Empty Tower";
        }

        public Dictionary<BaseObjPropertyType, string> GetObjectInfo()
        {
            if (currentPreset == null)
            {
                return null;
            }

            var info = new Dictionary<BaseObjPropertyType, string>();

            if (currentPreset == null) return info;

            info.Add(BaseObjPropertyType.Type, currentPreset.towerType.ToString());
            info.Add(BaseObjPropertyType.Cost, currentPreset.towerPriceValue.ToString());

            // Yêu cầu các Strategy (như AttackStrategy) cung cấp thêm thông số của chúng
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