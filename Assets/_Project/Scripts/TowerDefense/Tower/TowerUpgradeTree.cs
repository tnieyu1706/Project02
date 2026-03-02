using System;
using System.Collections.Generic;
using TnieYuPackage.DictionaryUtilities;
using TnieYuPackage.DesignPatterns.Patterns.Singleton;
using UnityEngine;

namespace _Project.Scripts.TowerDefense.Tower
{
    [Serializable]
    public class TowerUpgradeTreeNode
    {
        public TowerPresetSo towerPreset;
        
        public List<string> nextUpgradeIds;
    }

    [CreateAssetMenu(fileName="TowerUpgradeTree", menuName="Game/TD/UpgradeTree")]
    public class TowerUpgradeTree : SingletonScriptable<TowerUpgradeTree>
    {
        public SerializableDictionary<string, TowerUpgradeTreeNode> tree;
    }
}