using Game.Global;
using UnityEngine;

namespace Game.BuildingGameplay
{
    public enum ArmyCategory
    {
        Infantry,
        Cavalry,
        Archer,
        Siege,
        Support
    }

    [CreateAssetMenu(fileName = "ArmyTypePreset", menuName = "Game/StrategyBuilding/Building/ArmyType/ArmyTypePreset")]
    public class ArmyTypePresetSo : ScriptableObject
    {
        [Header("Basic Info")] public ArmyType armyType;
        public ArmyCategory armyCategory;
        public Sprite icon;

        [Header("Spawn Info")] public float delaySpawn;
        public int spawnAmount = 5;
        public SerializableActionCost cost;
    }
}