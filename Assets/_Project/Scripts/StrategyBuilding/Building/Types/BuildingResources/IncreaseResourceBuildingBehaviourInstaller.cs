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

        public int convenientValuePerTile;
        public int adverseValuePerTile;

        protected override IBuildingBehaviour Create()
        {
            return new IncreaseResourceBuildingBehaviour(this);
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

        private readonly int convenientValuePerTile;
        private readonly int adverseValuePerTile;

        public IncreaseResourceBuildingBehaviour(IncreaseResourceBuildingBehaviourInstaller installer) : base(installer)
        {
            ResourceType = installer.resourceType;
            value = installer.value;
            upgradeValueIncrease = installer.upgradeValueIncrease;
            convenientValuePerTile = installer.convenientValuePerTile;
            adverseValuePerTile = installer.adverseValuePerTile;
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

        protected int GetTotalValue()
        {
            return value
                   + convenientValuePerTile * ConvenientTiles.Count
                   - adverseValuePerTile * AdverseTiles.Count;
        }

        public void Execute()
        {
            SbGameplayController.GetResource(ResourceType).Value += GetTotalValue();
        }
    }
}