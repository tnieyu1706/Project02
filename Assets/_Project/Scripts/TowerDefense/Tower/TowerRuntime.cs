using System.Collections.Generic;
using EditorAttributes;
using KBCore.Refs;
using UnityEngine;

namespace _Project.Scripts.TowerDefense.Tower
{
    [RequireComponent(typeof(Animator))]
    public class TowerRuntime : MonoBehaviour
    {
        [SerializeField, Self] private Animator animator;
        [Child] private SpriteRenderer spriteRenderer;

        [SerializeField, ReadOnly] private string currentTowerContextId = string.Empty;
        private TowerPresetSo currentPreset;

        private Dictionary<string, TowerUpgradeTreeNode> upgradeTree => TowerUpgradeTree.Instance.tree.Dictionary;

        void Awake()
        {
            animator ??= GetComponent<Animator>();
            spriteRenderer ??= GetComponent<SpriteRenderer>();
        }

        //Test

        [Button]
        public void SetContext(string contextId)
        {
            if (!upgradeTree.TryGetValue(contextId, out TowerUpgradeTreeNode upgradeTreeNode)) return;

            var preset = upgradeTreeNode.towerPreset;
            SetPreset(preset);

            currentTowerContextId = contextId;
        }

        [Button]
        public void UpgradeContext(string contextId)
        {
            if (currentTowerContextId == string.Empty) return;

            if (!upgradeTree.TryGetValue(currentTowerContextId, out TowerUpgradeTreeNode upgradeTreeNode)) return;

            if (!upgradeTreeNode.nextUpgradeIds.Contains(contextId)) return;

            SetContext(contextId);
        }

        private void SetPreset(TowerPresetSo preset)
        {
            if (currentPreset is not null)
            {
                currentPreset.behaviourInstaller.UnInstall(gameObject);
            }
            
            //preset properties
            animator.runtimeAnimatorController = preset.animatorController;

            currentPreset = preset;
            currentPreset.behaviourInstaller.Install(gameObject);
        }
    }
}