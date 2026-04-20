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
        public ArmyType armyType;
        public ArmyCategory armyCategory;
        public Sprite icon;
        public float delaySpawn;
        public SerializableActionCost cost;
    }
}