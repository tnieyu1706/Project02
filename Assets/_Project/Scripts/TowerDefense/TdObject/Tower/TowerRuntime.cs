using BackboneLogger;
using UnityEngine;

namespace Game.Td
{
    [RequireComponent(typeof(Animator))]
    public class TowerRuntime : TdObjectRuntime<TowerPresetSo>
    {
        void Start()
        {
            if (!TowerUpgradeTree.Tree.TryGetValue(TdConstant.TD_EMPTY_TOWER_ID, out TowerUpgradeTreeNode node))
            {
                BLogger.Log($"[TowerRuntime] {name} cannot find empty tower node in upgrade tree. Please check if the empty tower node is added to the upgrade tree.", LogLevel.Error, "TD");
                return;
            }
            
            SetPreset(node.towerPreset);
        }

        public void Setup(TowerPresetSo preset)
        {
            SetPreset(preset);
        }
    }
}