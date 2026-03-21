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
        public MapEnvType convenientEnv;
        public MapEnvType adverseEnv;

        public Vector2Int BuildingPos { get; private set; }

        public MapEnvType ConvenientEnv => convenientEnv;
        public MapEnvType AdverseEnv => adverseEnv;
        public string BuildingName => buildingName;

        public void Init(GameObject go)
        {
            if (go.TryGetComponent(out BuildingRuntime buildingRuntime))
            {
                BuildingPos = buildingRuntime.TilePosition;
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
        public Vector2Int BuildingPos { get; }

        public List<StyleSheet> StyleSheets { get; }

        private ActionCost upgradeCost;
        private MapEnvType convenientEnv;
        private List<Vector2Int> convenientTiles;
        private MapEnvType adverseEnv;
        private List<Vector2Int> adverseTiles;

        public ref ActionCost UpgradeCost => ref upgradeCost;
        private ActionCost UpgradeCostIncrease { get; }
        public MapEnvType ConvenientEnv => convenientEnv;
        public List<Vector2Int> ConvenientTiles => convenientTiles;
        public MapEnvType AdverseEnv => adverseEnv;
        public List<Vector2Int> AdverseTiles => adverseTiles;

        public BaseBuildingBehaviour(BaseBuildingBehaviourInstaller installer)
        {
            BuildingName = installer.BuildingName;
            BuildingPos = installer.BuildingPos;
            StyleSheets = installer.styleSheets;

            upgradeCost = installer.defaultCostUpgrade;
            UpgradeCostIncrease = installer.defaultCostUpgradeIncrease;

            convenientEnv = installer.convenientEnv;
            adverseEnv = installer.adverseEnv;
            convenientTiles = new List<Vector2Int>();
            adverseTiles = new List<Vector2Int>();
        }

        public void Upgrade()
        {
            //upgrade
            HandleUpgrade();

            //cost
            UpgradeCost.CollectCost();
            ActionCost.AddCost(ref UpgradeCost, UpgradeCostIncrease);
        }

        public void ApplyConvenientNeighborTiles(Dictionary<Vector2Int, SbTileData> tiles)
        {
            foreach (var tile in tiles)
            {
                if (tile.Value.EnvType != convenientEnv) continue;

                convenientTiles.Add(tile.Key);
                tile.Value.BuildingTiles.Add(BuildingPos);
            }
        }

        public void ApplyAdverseNeighborTiles(Dictionary<Vector2Int, SbTileData> tiles)
        {
            foreach (var tile in tiles)
            {
                if (tile.Value.EnvType != adverseEnv) continue;

                adverseTiles.Add(tile.Key);
                tile.Value.BuildingTiles.Add(BuildingPos);
            }
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
            upgradeBtn.RegisterCallback<MouseLeaveEvent>(_ => { tooltip.style.display = DisplayStyle.None; });
        }

        protected abstract void RenderContent(VisualElement content);
    }
}