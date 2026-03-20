using TnieYuPackage.Utils.Structures;
using UnityEngine;

namespace Game.StrategyBuilding
{
    [CreateAssetMenu(fileName = "BuildingPresetManager", menuName = "Game/StrategyBuilding/Building/Manager")]
    public class BuildingPresetManager : BaseAssetManager<string, BuildingPresetSo, BuildingPresetManager>
    {
        protected override string GetAssetIdentify(BuildingPresetSo asset)
        {
            return asset.buildingId;
        }
    }
}