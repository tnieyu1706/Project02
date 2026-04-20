using System;
using Game.StrategyBuilding;

namespace Game.Global
{
    /// <summary>
    /// Unlocks specific building types in BuildingGameplay
    /// </summary>
    [Serializable]
    public class BuildingUnlockInfluencer : ISkillInfluencer
    {
        public BuildingType buildingType;

        public void ApplyAffect()
        {
            // Add new entry if it doesn't exist in the dictionary
            GamePropertiesRuntime.Instance.UnlockBuildingTypeDict[buildingType] = true;
        }
    }
}