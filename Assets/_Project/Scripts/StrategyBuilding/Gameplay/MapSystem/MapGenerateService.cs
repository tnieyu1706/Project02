using System;
using System.Collections.Generic;
using BackboneLogger;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.StrategyBuilding
{
    [Serializable]
    public struct SbMapLayer
    {
        public MapEnvType envType;
        public Tilemap tilemap;
    }
    
    [Serializable]
    public class MapGenerateService
    {
        public void GenerateMap(MapData mapData, List<SbMapLayer> layers)
        {
            foreach (var configLayer in layers)
            {
                GenerateLayer(mapData, configLayer);
            }
        }

        private static void GenerateLayer(MapData mapData, SbMapLayer configSbMapLayer)
        {
            var bounds = configSbMapLayer.tilemap.cellBounds;

            int count = 0;
            foreach (var pos in bounds.allPositionsWithin)
            {
                var pos2d = (Vector2Int)pos;
                if (configSbMapLayer.tilemap.HasTile(pos) && mapData.Validate(pos2d))
                {
                    mapData.SetTile(pos2d, configSbMapLayer.envType);
                    count++;
                }
            }
            
            BLogger.Log($"Generated layer: {configSbMapLayer.envType} - {count} tiles", category: "SB");
        }
    }
}