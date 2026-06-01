using System.Collections.Generic;
using Game.BaseGameplay;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Global.UI.WorldMap
{
    /// <summary>
    /// ScriptableObject đại diện cho 1 Node trong đồ thị Level.
    /// Tách riêng ra file LevelMapNode.cs để tránh xung đột class trong Unity.
    /// </summary>
    public class LevelMapNode : ScriptableObject
    {
        [HideInInspector] public string guid;
        [HideInInspector] public Vector2 position;
        
        public string nodeName = "New Level Node"; 
        
        public BuildingGameplayLevel level;
        public List<LevelMapNode> children = new List<LevelMapNode>();
    }
}