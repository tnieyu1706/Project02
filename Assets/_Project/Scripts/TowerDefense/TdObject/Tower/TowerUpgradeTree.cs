using System;
using System.Collections.Generic;
using TnieYuPackage.DesignPatterns;
using TnieYuPackage.DictionaryUtilities;
using UnityEngine;

namespace Game.Td
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