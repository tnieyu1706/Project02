using System;
using System.Collections.Generic;
using EditorAttributes;
using TnieYuPackage.DesignPatterns;
using UnityEditor;
using UnityEngine;
using ZLinq;

namespace Game.Td
{
    [Serializable]
    public class TowerUpgradeTreeNode
    {
        public TowerPresetSo towerPreset;

        [SerializeField] public List<TowerPresetSo> nextUpgradeTowers = new();

        public List<string> NextUpgradeTowerIds =>
            nextUpgradeTowers
                .AsValueEnumerable()
                .Select(t => t.towerId)
                .ToList();
    }

    [CreateAssetMenu(fileName = "TowerUpgradeTree", menuName = "Game/TD/UpgradeTree")]
    public class TowerUpgradeTree : SingletonScriptable<TowerUpgradeTree>
    {
        [SerializeField] private List<TowerUpgradeTreeNode> nodes;

        private static Dictionary<string, TowerUpgradeTreeNode> tree;

        public static Dictionary<string, TowerUpgradeTreeNode> Tree =>
            tree ??= Instance.nodes
                .AsValueEnumerable()
                .ToDictionary(n => n.towerPreset.towerId, n => n);
        
#if UNITY_EDITOR

        [Button]
        private void LoadData()
        {
            List<TowerPresetSo> presetNodes = nodes.AsValueEnumerable()
                .Select(n => n.towerPreset)
                .ToList();

            string[] guids = AssetDatabase.FindAssets($"t:{nameof(TowerPresetSo)}");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TowerPresetSo preset = AssetDatabase.LoadAssetAtPath<TowerPresetSo>(path);

                if (preset != null && !presetNodes.Contains(preset))
                {
                    nodes.Add(new TowerUpgradeTreeNode()
                    {
                        towerPreset = preset,
                    });
                }
            }
        }

#endif
    }
}