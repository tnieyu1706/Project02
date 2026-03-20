using TnieYuPackage.DesignPatterns;
using UnityEngine;

namespace Game.StrategyBuilding
{
    [CreateAssetMenu(fileName = "ArmyTypePreset", menuName = "Game/StrategyBuilding/Building/ArmyType/ArmyTypePreset")]
    public class ArmyTypePresetSo : ScriptableObject
    {
        public SbArmy armyType;
        public Sprite icon;
        public float delaySpawn;
        public ActionCost cost;

        public void Generate()
        {
            SbGameplayController.GetArmy(armyType).Value++;
        }
    }
}