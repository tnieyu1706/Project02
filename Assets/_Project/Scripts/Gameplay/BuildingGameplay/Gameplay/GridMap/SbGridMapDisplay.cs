using UnityEngine;
using UnityEngine.Tilemaps;
using Game.StrategyBuilding;
using System.Collections.Generic;
using System.Linq;
using TnieYuPackage.Utils;

namespace Game.BuildingGameplay
{
    /// <summary>
    /// Component đóng vai trò View (MVC) xử lý phần hiển thị giao diện Vùng Xây Dựng (Flood Area).
    /// Nó lắng nghe (EDP) hệ thống GridMapSystem để cập nhật đúng vị trí mà không can thiệp vào logic lõi.
    /// </summary>
    public class SbGridMapDisplay : MonoBehaviour
    {
        [Header("References")] [Tooltip("Tilemap riêng dùng để vẽ hiển thị vùng xây dựng")] [SerializeField]
        private Tilemap displayTilemap;

        [Tooltip("Tile ảnh trắng mờ để thể hiện khu vực có thể xây")] [SerializeField]
        private TileBase allowedAreaTile;

        private void OnEnable()
        {
            // Subscribe các sự kiện từ hệ thống lõi
            SbGridMapSystem.OnTileCreated += HandleTileCreated;
            SbGridMapSystem.OnTileDestroyed += HandleTileDestroyed;
            SbGridMapSystem.OnMapCleared += HandleMapCleared;
        }

        private void OnDisable()
        {
            // Unsubscribe để tránh leak memory
            SbGridMapSystem.OnTileCreated -= HandleTileCreated;
            SbGridMapSystem.OnTileDestroyed -= HandleTileDestroyed;
            SbGridMapSystem.OnMapCleared -= HandleMapCleared;
        }

        private void HandleTileCreated(Vector2Int pos, SbGridTileData data)
        {
            // Khi có một Tile nào đó được đặt xuống (Building, Tree, Stone), 
            // ta chỉ cần kiểm tra lại vùng 9 ô xung quanh vị trí đó để vẽ/xoá hiển thị cho đúng.
            RefreshDisplayArea(pos);
        }

        private void HandleTileDestroyed(Vector2Int pos, SbGridTileData data)
        {
            // Tương tự, khi phá công trình/chặt cây, khu vực xung quanh sẽ có sự thay đổi
            // về "ValidForCreate" hoặc "HasAdjacentBuilding", nên ta refresh lại vùng đó.
            RefreshDisplayArea(pos);
        }

        private void HandleMapCleared()
        {
            // Xoá sạch hiển thị khi Load game mới hoặc Reset màn
            displayTilemap.ClearAllTiles();
        }

        /// <summary>
        /// Logic lõi của View: Kiểm tra khu vực (ô trung tâm + 8 hướng) 
        /// và cập nhật hiển thị mờ mờ cho các ô thoả mãn điều kiện.
        /// </summary>
        private void RefreshDisplayArea(Vector2Int centerPos)
        {
            // Tạo list gồm ô trung tâm và 8 ô xung quanh
            List<Vector2Int> tilesToUpdate = Vector2IntUtils.Get8DirectionalVectors()
                .Select(dir => centerPos + dir)
                .ToList();
            tilesToUpdate.Add(centerPos);

            foreach (var pos in tilesToUpdate)
            {
                // Điều kiện để được hiển thị ô trắng mờ (có thể xây dựng):
                // 1. Ô đó phải hợp lệ để xây (trống, không bị đá, cây, toà nhà chiếm chỗ)
                // 2. Ô đó phải nằm cạnh ít nhất 1 công trình khác
                bool canBuild = SbGridMapSystem.Instance.ValidForCreate(pos)
                                && SbGridMapSystem.Instance.HasAdjacentBuilding(pos);

                if (canBuild)
                {
                    // Nếu thoả mãn, ta vẽ cái tile mờ lên displayTilemap
                    displayTilemap.SetTile((Vector3Int)pos, allowedAreaTile);
                }
                else
                {
                    // Nếu không (ví dụ: công trình bị huỷ làm mất liên kết của ô này, 
                    // hoặc người chơi vừa đặt đá/cây đè lên), xoá cái tile mờ đi
                    displayTilemap.SetTile((Vector3Int)pos, null);
                }
            }
        }
    }
}