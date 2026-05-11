using System.Collections.Generic;
using System.Linq;
using Game.StrategyBuilding;
using SoundSystem.Core;
using UnityEngine;

namespace Game.BuildingGameplay
{
    [CreateAssetMenu(fileName = "BuildingPresetManager", menuName = "Game/StrategyBuilding/Building/Manager")]
    public class BuildingPresetManager : ScriptableObject
    {
        public SoundData buildSfx;
        public SoundData destroySfx;
        public List<BuildingPresetSo> presets = new();

        private Dictionary<string, BuildingPresetSo> refs;

        public Dictionary<string, BuildingPresetSo> Refs =>
            refs ??= presets.ToDictionary(p => p.buildingId, p => p);
    }
}