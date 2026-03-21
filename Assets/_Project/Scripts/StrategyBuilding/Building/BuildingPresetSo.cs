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
        Vector2Int BuildingPos { get; }
        
        MapEnvType ConvenientEnv { get; }
        MapEnvType AdverseEnv { get; }
        
        void Init(GameObject go);
        void Destroy(GameObject go);
    }

    public interface IBuildingBehaviour : IUIDisplay
    {
        string BuildingName { get; }
        Vector2Int BuildingPos { get; }
        ref ActionCost UpgradeCost { get; }
        
        MapEnvType ConvenientEnv { get; }
        List<Vector2Int> ConvenientTiles { get; }
        MapEnvType AdverseEnv { get; }
        List<Vector2Int> AdverseTiles { get; }
        
        void Upgrade();
        void ApplyConvenientNeighborTiles(Dictionary<Vector2Int, SbTileData> tiles);
        void ApplyAdverseNeighborTiles(Dictionary<Vector2Int, SbTileData> tiles);
    }

    public interface IUIDisplay
    {
        List<StyleSheet> StyleSheets { get; }
        void Render(VisualElement root);
    }
}