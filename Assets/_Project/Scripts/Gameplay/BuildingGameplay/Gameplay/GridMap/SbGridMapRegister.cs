using System.Collections.Generic;
using TnieYuPackage.DesignPatterns;
using TnieYuPackage.DictionaryUtilities;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.StrategyBuilding
{
    /// <summary>
    /// Component dùng để đặt trên Scene. Hỗ trợ quét các Tilemap môi trường do Level Designer vẽ, 
    /// sau đó merge (gộp) tất cả vào GridMapSystem (cả logic data lẫn visual).
    /// </summary>
    public class SbGridMapRegister : SingletonBehavior<SbGridMapRegister>
    {
        [Header("Environment Settings")]
        [Tooltip("Danh sách các Tilemap chứa tài nguyên môi trường cần đăng ký vào hệ thống")]
        public SerializableDictionary<SbTileLayer, Tilemap> environmentMaps = new();

        [Header("Block Map Settings")] [Tooltip("Danh sách các Tilemap chứa các ô không thể xây dựng/tương tác")]
        public List<Tilemap> blockMaps = new();

        [Header("Auto Execute")] [Tooltip("Nếu true, tự động Merge Map khi game bắt đầu")]
        public bool registerBlockOnStart = true;

        private void Start()
        {
            if (registerBlockOnStart)
            {
                RegisterBlockMaps();
            }
        }

        private void RegisterBlockMaps()
        {
            if (SbGridMapSystem.Instance == null) return;

            foreach (var blockTilemap in blockMaps)
            {
                if (blockTilemap == null) continue;

                // Quét qua tất cả các ô có chứa tile trong Tilemap này
                BoundsInt bounds = blockTilemap.cellBounds;
                foreach (var pos in bounds.allPositionsWithin)
                {
                    if (blockTilemap.HasTile(pos))
                    {
                        Vector2Int gridPos = new Vector2Int(pos.x, pos.y);

                        // Thêm vào BlockMap của hệ thống nếu chưa tồn tại
                        if (!SbGridMapSystem.Instance.BlockMap.Contains(gridPos))
                        {
                            SbGridMapSystem.Instance.BlockMap.Add(gridPos);
                        }
                    }
                }
            }
        }

        public void RegisterEnvironmentMaps()
        {
            if (SbGridMapSystem.Instance == null) return;
            // Sử dụng Dictionary từ SerializableDictionary
            foreach (var kvp in environmentMaps.Dictionary)
            {
                var tileLayer = kvp.Key;
                var sourceTilemap = kvp.Value;

                if (sourceTilemap == null) continue;

                BoundsInt bounds = sourceTilemap.cellBounds;
                foreach (var pos in bounds.allPositionsWithin)
                {
                    if (sourceTilemap.HasTile(pos))
                    {
                        Vector2Int gridPos = new Vector2Int(pos.x, pos.y);

                        // Hàm CreateAndWriteTile của bạn đã bao gồm việc:
                        // 1. Kiểm tra hợp lệ (không trùng BlockMap, không trùng GridMap)
                        // 2. Tạo SbGridTileData (truyền null cho building vì đây là Env)
                        // 3. Ghi vào Dictionary và SetTile lên main gridTilemap
                        SbGridTileData createdTile = SbGridMapSystem.Instance.CreateAndWriteTile(
                            gridPos,
                            tileLayer,
                            null // EnvTile nên không có BuildingRuntime
                        );

                        if (createdTile == null)
                        {
                            Debug.LogWarning(
                                $"[SbGridMapRegister] Vị trí {gridPos} đã bị chiếm hoặc bị block. Bỏ qua Env Tile thuộc layer {tileLayer}.");
                        }
                    }
                }

                // Tắt Gameobject chứa Tilemap này đi để tránh việc render đè lên mainGrid
                sourceTilemap.gameObject.SetActive(false);
            }
        }
    }
}