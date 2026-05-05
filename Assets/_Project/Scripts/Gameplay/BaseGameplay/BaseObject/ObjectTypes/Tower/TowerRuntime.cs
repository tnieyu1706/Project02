using Reflex.Attributes;
using UnityEngine;

namespace Game.BaseGameplay
{
    [RequireComponent(typeof(Animator))]
    public class TowerRuntime : BaseObjectRuntime<TowerPresetSo>
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
    }
}