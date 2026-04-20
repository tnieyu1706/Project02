using System.Collections.Generic;
using Game.StrategyBuilding;
using TnieYuPackage.Utils;
using UnityEngine;

namespace Game.BuildingGameplay
{
    [CreateAssetMenu(fileName = "BuildingPresetManager", menuName = "Game/StrategyBuilding/Building/Manager")]
    public class BuildingPresetManager : BaseAssetManager<string, BuildingPresetSo, BuildingPresetManager>
    {
        [SerializeField] private List<string> excludedIds;

        protected override string GetAssetIdentify(BuildingPresetSo asset)
        {
            return asset.buildingId;
        }

        protected override bool FilterData(BuildingPresetSo asset)
        {
            return !excludedIds.Contains(asset.buildingId);
        }
    }
}