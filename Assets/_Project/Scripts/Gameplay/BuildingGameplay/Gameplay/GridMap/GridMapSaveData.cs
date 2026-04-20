using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace Game.StrategyBuilding
{
    /// <summary>
    /// DTO Class chứa toàn bộ dữ liệu của GridMap cần save/load.
    /// </summary>
    [Serializable]
    public class GridMapSaveData
    {
        public List<GridTileDTO> GridTiles = new();
    }

    /// <summary>
    /// DTO Class lưu trữ thông tin của từng ô Tile.
    /// </summary>
    [Serializable]
    public class GridTileDTO
    {
        public Vector2Int Position;
        public SbTileLayer TileLayer;
        
        // Nếu ô này là một công trình, ta lưu ID của Preset. Nếu là môi trường, chuỗi này sẽ rỗng/null.
        public string BuildingId;

        // Lưu trữ dữ liệu riêng của Behaviour bằng JObject để file JSON lưu ra cấu trúc lồng nhau (nested) gọn gàng
        public JObject BehaviourData;
    }
}