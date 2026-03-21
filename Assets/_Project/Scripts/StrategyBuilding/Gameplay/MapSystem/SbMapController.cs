using System.Collections.Generic;
using EditorAttributes;
using TnieYuPackage.DesignPatterns;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.StrategyBuilding
{
    public class SbMapController : SingletonBehavior<SbMapController>
    {
        [HideInInspector] public MapData mapData;
        public BuildingManager buildingManager;
        [SerializeField] private MapGenerateService mapGenerateService;

        #region PROPERTIES

        [SerializeField] internal Tilemap buildingTilemap;
        [SerializeField] private List<SbMapLayer> layers = new();

        [SerializeField] private int widthMapSize;
        [SerializeField] private int heightMapSize;

        #endregion

        private Dictionary<MapEnvType, Tilemap> layerTilemaps;
        public Dictionary<MapEnvType, Tilemap> LayerTilemaps
        {
            get
            {
                if (layerTilemaps == null)
                {
                    layerTilemaps = new Dictionary<MapEnvType, Tilemap>();
                    foreach (var layer in layers)
                    {
                        layerTilemaps[layer.envType] = layer.tilemap;
                    }
                }

                return layerTilemaps;
            }
        }

        protected override void Awake()
        {
            dontDestroyOnLoad = false;

            base.Awake();

            mapData = new MapData(widthMapSize, heightMapSize);
            mapGenerateService ??= new MapGenerateService();
        }

        private void Start()
        {
            GenerateMapManual();
        }

        [Button]
        private void GenerateMapManual()
        {
            mapGenerateService.GenerateMap(mapData, layers);
        }

        #region SUPPORT METHODS

        public Vector3Int GetCellFromWorld(Vector3 worldPos)
        {
            return buildingTilemap.WorldToCell(worldPos);
        }

        #endregion

        //test
        [Button]
        private void CheckTileData(Vector2Int pos)
        {
            if (!mapData.Validate(pos)) return;

            var tileData = mapData.GetTile(pos);
            Debug.Log($"Tile {pos} - Type: {tileData.EnvType} - Buildings: {tileData.BuildingTiles.Count}");
        }

        [Button]
        private void TestDestroyBuilding(Vector2Int pos)
        {
            if (!buildingManager.CheckExists(pos)) return;
            
            buildingManager.DestroyBuilding(pos);
        }
    }
}