using TnieYuPackage.Utils.Structures;
using UnityEngine;

namespace Game.StrategyBuilding
{
    [CreateAssetMenu(fileName = "ArmyTypeManager", menuName = "Game/StrategyBuilding/Building/ArmyType/ArmyTypeManager")]
    public class ArmyTypePresetManager : BaseAssetManager<SbArmy, ArmyTypePresetSo, ArmyTypePresetManager>
    {
        protected override SbArmy GetAssetIdentify(ArmyTypePresetSo asset)
        {
            return asset.armyType;
        }
    }
}