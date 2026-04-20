using BackboneLogger;
using UnityEngine;

namespace Game.BaseGameplay
{
    [RequireComponent(typeof(Animator))]
    public class TowerRuntime : BaseObjectRuntime<TowerPresetSo>
    {
        /// <summary>
        /// score == number paths this tower nest with. set in editor each tower.
        /// </summary>
        public int towerScore;
        
        void Start()
        {
            if (!TowerUpgradeTree.Tree.TryGetValue(BaseConstant.TOWER_EMPTY_ID, out TowerUpgradeTreeNode node))
            {
                BLogger.Log(
                    $"[TowerRuntime] {name} cannot find empty tower node in upgrade tree. Please check if the empty tower node is added to the upgrade tree.",
                    LogLevel.Error, "Base");
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