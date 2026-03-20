using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Game.StrategyBuilding
{
    [Serializable]
    public class CapacityResourceBuildingBehaviourInstaller : IncreaseResourceBuildingBehaviourInstaller
    {
        protected override IBuildingBehaviour Create()
        {
            return new CapacityResourceBuildingBehaviour(
                buildingName,
                styleSheets,
                defaultCostUpgrade,
                defaultCostUpgradeIncrease,
                resourceType,
                value,
                upgradeValueIncrease
            );
        }

        protected override void OnInit(BuildingRuntime buildingRuntime)
        {
            if (buildingRuntime.buildingBehaviour is CapacityResourceBuildingBehaviour capacityResourceBuildingType)
            {
                SbGameplayController.GetResource(capacityResourceBuildingType.ResourceType).Value +=
                    capacityResourceBuildingType.value;
            }
        }

        protected override void OnDestroy(BuildingRuntime buildingRuntime)
        {
            if (buildingRuntime.buildingBehaviour is CapacityResourceBuildingBehaviour capacityResourceBuildingType)
            {
                SbGameplayController.GetResource(capacityResourceBuildingType.ResourceType).Value -=
                    capacityResourceBuildingType.value;
            }
        }
    }

    [Serializable]
    public class CapacityResourceBuildingBehaviour : IncreaseResourceBuildingBehaviour
    {
        public CapacityResourceBuildingBehaviour(string buildingName, List<StyleSheet> styleSheets, ActionCost upgradeCost, ActionCost upgradeCostIncrease, SbResource resourceType, int value, int upgradeValueIncrease) : base(buildingName, styleSheets, upgradeCost, upgradeCostIncrease, resourceType, value, upgradeValueIncrease)
        {
        }

        protected override void HandleUpgrade()
        {
            var preValue = value;
            base.HandleUpgrade();

            ref var resourceValue = ref SbGameplayController.GetResource(ResourceType);
            resourceValue.Value += value - preValue;
        }
    }
}