using System;
using System.Collections.Generic;
using TnieYuPackage.GlobalExtensions;
using UnityEngine;

namespace Game.StrategyBuilding
{
    public enum MapEnvType
    {
        None,
        Tree,
        Water,
        Mountain,
        X
    }

    public struct SbTileData
    {
        public MapEnvType EnvType;
        public readonly List<Vector2Int> BuildingTiles;

        public SbTileData(MapEnvType envType)
        {
            EnvType = envType;
            BuildingTiles = new();
        }
    }

    [Serializable]
    public class MapData
    {
        [SerializeField] private int widthMapSize;
        [SerializeField] private int heightMapSize;
        private SbTileData[,] tiles;
        

        private BuildingManager BuildingManagerSource => SbMapController.Instance.buildingManager;

        public MapData(int widthMapSize, int heightMapSize)
        {
            this.widthMapSize = widthMapSize;
            this.heightMapSize = heightMapSize;

            Initialize();
        }

        private void Initialize()
        {
            var widthCapacity = widthMapSize * 2 + 1;
            var heightCapacity = heightMapSize * 2 + 1;
            tiles = new SbTileData[widthCapacity, heightCapacity];

            for (int x = 0; x < widthCapacity; x++)
            {
                for (int y = 0; y < heightCapacity; y++)
                {
                    tiles[x, y] = new SbTileData(MapEnvType.None);
                }
            }
        }

        private Vector2Int GetCoordinatePosVector(Vector2Int pos)
        {
            return new Vector2Int(pos.x + widthMapSize, pos.y + heightMapSize);
        }

        private (int, int) GetCoordinatePosTuple(Vector2Int pos)
        {
            return (pos.x + widthMapSize, pos.y + heightMapSize);
        }

        public bool Validate(Vector2Int pos)
        {
            return (-widthMapSize <= pos.x && pos.x <= widthMapSize)
                   && (-heightMapSize <= pos.y && pos.y <= heightMapSize);
        }

        public bool CheckExists(Vector2Int pos)
        {
            var (x, y) = GetCoordinatePosTuple(pos);
            return tiles[x, y].EnvType != MapEnvType.None;
        }

        public void SetTile(Vector2Int pos, MapEnvType envType)
        {
            var (x, y) = GetCoordinatePosTuple(pos);
            
            //handle: data
            ref var tileData = ref tiles[x, y];
            
            var oldEnvType = tileData.EnvType;
            tileData.EnvType = envType;

            if (envType == MapEnvType.None)
            {
                HandleTileAsNone(pos, ref tileData);
                
                //handle tilemap
                SbMapController.Instance.LayerTilemaps[oldEnvType]
                    .SetTile(pos.ToVector3Int(0), null); //default
            }
        }

        private void HandleTileAsNone(Vector2Int pos, ref SbTileData tileData)
        {
            foreach (var building in tileData.BuildingTiles)
            {
                var buildingBehaviour = BuildingManagerSource.Get(building).buildingBehaviour;

                var convenientTiles = buildingBehaviour.ConvenientTiles;
                convenientTiles.Remove(pos);

                var adverseTiles = buildingBehaviour.AdverseTiles;
                adverseTiles.Remove(pos);
            }

            tileData.BuildingTiles.Clear();
        }

        public SbTileData GetTile(Vector2Int pos)
        {
            var (x, y) = GetCoordinatePosTuple(pos);
            return tiles[x, y];
        }

        public ref SbTileData GetTileRef(Vector2Int pos)
        {
            var (x, y) = GetCoordinatePosTuple(pos);
            return ref tiles[x, y];
        }

        /// <summary>
        /// Get neighbor tiles with specific envType. (include diagonal)
        /// Values of dictionary: origin reference.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="envType"></param>
        /// <returns></returns>
        public Dictionary<Vector2Int, SbTileData> GetNeighbors(Vector2Int pos, MapEnvType envType)
        {
            Dictionary<Vector2Int, SbTileData> neighbors = new();

            Vector2Int startSearch = pos - Vector2Int.one;
            Vector2Int endSearch = pos + Vector2Int.one;

            for (int x = startSearch.x; x <= endSearch.x; x++)
            {
                for (int y = startSearch.y; y <= endSearch.y; y++)
                {
                    var searchPos = new Vector2Int(x, y);
                    if (searchPos.Equals(pos)) continue;
                    
                    if (!Validate(searchPos)) continue;

                    var searchTile = GetTileRef(searchPos);
                    if (searchTile.EnvType != envType) continue;

                    neighbors.Add(searchPos, searchTile);
                }
            }

            return neighbors;
        }
    }
}