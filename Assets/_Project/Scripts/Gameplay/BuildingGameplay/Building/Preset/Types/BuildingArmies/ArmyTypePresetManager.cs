using System.Collections.Generic;
using System.Linq;
using Game.Global;
using UnityEngine;

namespace Game.BuildingGameplay
{
    [CreateAssetMenu(fileName = "ArmyTypeManager",
        menuName = "Game/StrategyBuilding/Building/ArmyType/ArmyTypeManager")]
    public class ArmyTypePresetManager : ScriptableObject
    {
        public List<ArmyTypePresetSo> presets;

        private Dictionary<ArmyType, ArmyTypePresetSo> refs;

        public Dictionary<ArmyType, ArmyTypePresetSo> Refs =>
            refs ??= presets.ToDictionary(p => p.armyType, p => p);
    }
}