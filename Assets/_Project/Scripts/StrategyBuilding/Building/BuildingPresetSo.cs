using System.Collections.Generic;
using TnieYuPackage.CustomAttributes;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

namespace Game.StrategyBuilding
{
    [CreateAssetMenu(fileName = "BuildingPreset", menuName = "Game/StrategyBuilding/Building/Preset")]
    public class BuildingPresetSo : ScriptableObject
    {
        public string buildingId;
        public Tile buildingTile;
        public ActionCost costBuilding;

        [SerializeReference]
        [AbstractSupport(
            classAssembly: typeof(IBuildingBehaviourInstaller),
            abstractTypes: typeof(IBuildingBehaviourInstaller)
        )]
        public IBuildingBehaviourInstaller behaviourInstaller;
    }

    public interface IBuildingBehaviourInstaller
    {
        string BuildingName { get; }
        void Init(GameObject go);
        void Destroy(GameObject go);
    }

    public interface IBuildingBehaviour : IUIDisplay
    {
        string BuildingName { get; }
        ref ActionCost UpgradeCost { get; }
        void Upgrade();
    }

    public interface IUIDisplay
    {
        List<StyleSheet> StyleSheets { get; }
        void Render(VisualElement root);
    }
}