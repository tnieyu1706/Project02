using System;
using System.Collections.Generic;
using TnieYuPackage.GlobalExtensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.StrategyBuilding
{
    [Serializable]
    public abstract class BaseBuildingBehaviourInstaller : IBuildingBehaviourInstaller
    {
        public string buildingName;
        public List<StyleSheet> styleSheets;
        public ActionCost defaultCostUpgrade;
        public ActionCost defaultCostUpgradeIncrease;

        public string BuildingName => buildingName;

        public void Init(GameObject go)
        {
            if (go.TryGetComponent(out BuildingRuntime buildingRuntime))
            {
                buildingRuntime.buildingBehaviour = Create();

                OnInit(buildingRuntime);
            }
        }

        protected abstract IBuildingBehaviour Create();

        public void Destroy(GameObject go)
        {
            if (go.TryGetComponent(out BuildingRuntime buildingRuntime))
            {
                buildingRuntime.buildingBehaviour = null;

                OnDestroy(buildingRuntime);
            }
        }

        protected abstract void OnInit(BuildingRuntime buildingRuntime);
        protected abstract void OnDestroy(BuildingRuntime buildingRuntime);
    }

    [Serializable]
    public abstract class BaseBuildingBehaviour : IBuildingBehaviour
    {
        public string BuildingName { get; }
        public List<StyleSheet> StyleSheets { get; }

        private ActionCost upgradeCost;
        public ref ActionCost UpgradeCost => ref upgradeCost;
        private ActionCost UpgradeCostIncrease { get; }

        protected BaseBuildingBehaviour(string buildingName, List<StyleSheet> styleSheets, ActionCost upgradeCost,
            ActionCost upgradeCostIncrease)
        {
            this.upgradeCost = upgradeCost;
            BuildingName = buildingName;
            StyleSheets = styleSheets;
            UpgradeCostIncrease = upgradeCostIncrease;
        }

        public void Upgrade()
        {
            //upgrade
            HandleUpgrade();

            //cost
            UpgradeCost.CollectCost();
            ActionCost.AddCost(ref UpgradeCost, UpgradeCostIncrease);
        }

        protected abstract void HandleUpgrade();

        public void Render(VisualElement root)
        {
            root.Clear();
            root.styleSheets.Clear();

            foreach (var styleSheet in StyleSheets)
            {
                root.styleSheets.Add(styleSheet);
            }

            VisualElement container = root.CreateChild<VisualElement>("container");

            //header
            VisualElement header = container.CreateChild<VisualElement>("header");
            var title = header.CreateChild<Label>("title");
            title.text = BuildingName;

            //content
            VisualElement content = container.CreateChild<VisualElement>("content");
            RenderContent(content);
            
            //tooltip
            var tooltip = root.CreateChild<Label>("tooltip");
            tooltip.BringToFront();

            //footer
            VisualElement footer = container.CreateChild<VisualElement>("footer");
            var upgradeBtn = footer.CreateChild<Button>("footer-btn");
            upgradeBtn.text = "Upgrade";
            upgradeBtn.clicked += Upgrade;
            
            upgradeBtn.RegisterCallback<MouseEnterEvent>(evt =>
            {
                tooltip.text = UpgradeCost.ToString();
                tooltip.style.display = DisplayStyle.Flex;

                var pos = evt.mousePosition;

                tooltip.style.left = pos.x + 10;
                tooltip.style.top = pos.y + 10;
            });
            upgradeBtn.RegisterCallback<MouseLeaveEvent>(_ =>
            {
                tooltip.style.display = DisplayStyle.None;
            });
        }

        protected abstract void RenderContent(VisualElement content);
    }
}