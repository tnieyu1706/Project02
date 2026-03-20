using System;
using System.Collections.Generic;
using System.Reflection;
using TnieYuPackage.GlobalExtensions;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using ZLinq;

namespace Game.StrategyBuilding
{
    [Serializable]
    public class IncreaseResourceBuildingBehaviourInstaller : BaseBuildingBehaviourInstaller
    {
        public SbResource resourceType;
        public int value;
        public int upgradeValueIncrease;
        
        protected override IBuildingBehaviour Create()
        {
            return new IncreaseResourceBuildingBehaviour(
                buildingName,
                styleSheets,
                defaultCostUpgrade,
                defaultCostUpgradeIncrease,
                resourceType,
                value,
                upgradeValueIncrease);
        }

        protected override void OnInit(BuildingRuntime buildingRuntime)
        {
            if (buildingRuntime.buildingBehaviour is IncreaseResourceBuildingBehaviour increaseResourceBuildingType)
            {
                SbTimeController.Instance.Registry(increaseResourceBuildingType.Execute);
            }
        }

        protected override void OnDestroy(BuildingRuntime buildingRuntime)
        {
            if (buildingRuntime.buildingBehaviour is IncreaseResourceBuildingBehaviour increaseResourceBuildingType)
            {
                SbTimeController.Instance.UnRegistry(increaseResourceBuildingType.Execute);
            }
        }
    }

    [Serializable]
    public class IncreaseResourceBuildingBehaviour : BaseBuildingBehaviour
    {
        public readonly SbResource ResourceType;
        public int value;
        private readonly int upgradeValueIncrease;

        public IncreaseResourceBuildingBehaviour(string buildingName, List<StyleSheet> styleSheets, ActionCost upgradeCost,
            ActionCost upgradeCostIncrease,
            SbResource resourceType, int value, int upgradeValueIncrease) : base(buildingName, styleSheets, upgradeCost, upgradeCostIncrease)
        {
            ResourceType = resourceType;
            this.value = value;
            this.upgradeValueIncrease = upgradeValueIncrease;
        }

        protected override void HandleUpgrade()
        {
            value += upgradeValueIncrease;
        }

        protected override void RenderContent(VisualElement content)
        {
            var resourceTypeText = content.CreateChild<Label>("content-info");
            resourceTypeText.text = $"Resource Type: {ResourceType}";

            var valueText = content.CreateChild<Label>("content-info");
            valueText.text = $"Value: {value}";
        }

        public void Execute()
        {
            SbGameplayController.GetResource(ResourceType).Value += value;
        }
    }
}