using System;
using System.Collections.Generic;
using System.Linq;
using Game.BaseGameplay;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Global.UI.WorldMap
{
    /// <summary>
    /// ScriptableObject chứa toàn bộ cây/đồ thị cấu trúc của World Map
    /// </summary>
    [CreateAssetMenu(fileName = "LevelMapTree", menuName = "Game/Level Map/Level Map Tree")]
    public class LevelMapTreeSo : ScriptableObject
    {
        public List<LevelMapNode> nodes = new List<LevelMapNode>();

        /// <summary>
        /// Lấy ra danh sách các màn chơi nối tiếp từ màn hiện tại
        /// </summary>
        public List<BuildingGameplayLevel> GetNextLevels(BuildingGameplayLevel currentLevel)
        {
            var node = nodes.FirstOrDefault(n => n.level == currentLevel);
            if (node != null)
            {
                return node.children.Select(c => c.level).ToList();
            }
            return new List<BuildingGameplayLevel>();
        }
    }
}