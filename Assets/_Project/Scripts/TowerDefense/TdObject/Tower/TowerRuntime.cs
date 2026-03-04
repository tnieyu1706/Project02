using System.Collections.Generic;
using EditorAttributes;
using KBCore.Refs;
using UnityEngine;

namespace Game.Td
{
    [RequireComponent(typeof(Animator))]
    public class TowerRuntime : MonoBehaviour
    {
        [SerializeField, Self] private Animator animator;

        [SerializeField, ReadOnly] private string currentTowerContextId = string.Empty;
        private TowerPresetSo currentPreset;

        private Dictionary<string, TowerUpgradeTreeNode> UpgradeTree => TowerUpgradeTree.Instance.tree.Dictionary;

        void Awake()
        {
            animator ??= GetComponent<Animator>();
        }

        void Start()
        {
            SetContext(TdConstant.TD_EMPTY_TOWER_ID);
        }

        [Button]
        public void UpgradeContext(string contextId)
        {
            if (currentTowerContextId == string.Empty
                || !UpgradeTree.TryGetValue(currentTowerContextId, out TowerUpgradeTreeNode upgradeTreeNode)
                || !upgradeTreeNode.nextUpgradeIds.Contains(contextId))
            {
                return;
            }

            SetContext(contextId);
        }

        //Test ==> default Tower = EmptyContext.
        private void SetContext(string contextId)
        {
            if (!UpgradeTree.TryGetValue(contextId, out TowerUpgradeTreeNode upgradeTreeNode)) return;

            var preset = upgradeTreeNode.towerPreset;
            SetPreset(preset);

            currentTowerContextId = contextId;
        }

        private void SetPreset(TowerPresetSo preset)
        {
            if (currentPreset is not null)
            {
                currentPreset.configurators.ForEach(conf => conf.UnConfig(gameObject));
                currentPreset.behaviourInstaller?.UnInstall(gameObject);
            }

            //preset properties
            animator.runtimeAnimatorController = preset.animatorController;

            currentPreset = preset;
            currentPreset.behaviourInstaller?.Install(gameObject);
            currentPreset.configurators.ForEach(conf => conf.Config(gameObject));
        }
    }
}