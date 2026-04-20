using Game.Global;
using TnieYuPackage.Utils;
using UnityEngine;

namespace Game.BuildingGameplay
{
    [CreateAssetMenu(fileName = "ArmyTypeManager", menuName = "Game/StrategyBuilding/Building/ArmyType/ArmyTypeManager")]
    public class ArmyTypePresetManager : BaseAssetManager<ArmyType, ArmyTypePresetSo, ArmyTypePresetManager>
    {
        protected override ArmyType GetAssetIdentify(ArmyTypePresetSo asset)
        {
            return asset.armyType;
        }
    }
}