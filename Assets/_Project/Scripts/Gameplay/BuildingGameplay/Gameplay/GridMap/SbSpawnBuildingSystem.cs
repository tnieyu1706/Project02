using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Gameplay.Global.GameController; // THÊM LINQ
using BackboneLogger;
using Game.BuildingGameplay;
using TnieYuPackage.Core;
using TnieYuPackage.DesignPatterns;
using TnieYuPackage.GlobalExtensions;
using TnieYuPackage.Utils;
using UnityEngine;

namespace Game.StrategyBuilding
{
    public class SbSpawnBuildingSystem : SingletonBehavior<SbSpawnBuildingSystem>
    {
        [SerializeField] private GameObject buildingPrefab;
        private static BuildingPresetSo currentBuildingPreset;

        // Cờ kiểm tra trạng thái có cho phép người chơi hủy (Cancel) đặt công trình hay không
        private static bool currentCanCancel = true;
        private static bool currentTimeStop = false;

        // THÊM THAM SỐ canCancel (Mặc định = true như cũ)
        public static void StartBuilding(BuildingPresetSo preset, bool canCancel = true, bool timeStop = false)
        {
            // KIỂM TRA ĐỘC QUYỀN NHÀ CHÍNH: Tránh trường hợp người chơi bấm trong Menu
            if (preset is MainBuildingPresetSo)
            {
                bool hasMainBuilding = SbGridMapSystem.Instance.GridMap.Values.Any(
                    t => t.BuildingRuntime != null && t.BuildingRuntime.currentPreset is MainBuildingPresetSo);

                if (hasMainBuilding)
                {
                    BLogger.Log("Chỉ được phép xây dựng 1 Nhà Chính trên bản đồ!", LogLevel.Warning, category: "SB");
                    return; // Chặn không cho spawn blueprint
                }
            }

            currentBuildingPreset = preset;
            currentCanCancel = canCancel;
            currentTimeStop = timeStop;

            if (currentTimeStop)
            {
                GameTimeController.SetGameStop();
            }

            MapInteractionHandler.Instance.enabled = false;

            InputEventManager.Instance.enabled = true;
            InputEventManager.Instance.OnMouseMove += DisplayBlueprint;

            InputEventManager.Instance.RegistryOnce(KeyCode.Mouse0, ConfirmBuildingHandler);

            // Nếu được phép hủy, đăng ký phím MOUSE 1 (Right Click)
            if (canCancel)
            {
                InputEventManager.Instance.RegistryOnce(KeyCode.Mouse1, CancelBuildingItemEvent);
            }
        }

        private static Vector2Int preTilePos;
        private static bool isTileValid;

        private static readonly Dictionary<Vector2Int, Sprite> ContextSpritesTemp = new();

        private static Dictionary<Vector2Int, Sprite> GetTileContextSprites(Vector2Int pos)
        {
            ContextSpritesTemp.Clear();

            foreach (var dir in Vector2IntUtils.Get8DirectionalVectors())
            {
                var tilePos = pos + dir;
                var neighborTileData = SbGridMapSystem.Instance.ReadTile(tilePos);
                if (neighborTileData == null) continue;

                // Influence for neighbors
                if (neighborTileData.BuildingRuntime != null
                    && neighborTileData.BuildingRuntime.currentPreset.InfluenceEffects.ContainsKey(currentBuildingPreset
                        .tileLayer))
                {
                    ContextSpritesTemp[dir] = SbGridMapDataController.Instance.influenceContextSprite;
                    continue; // one sprite can set.
                }

                // Impacted by neighbors
                if (currentBuildingPreset.InfluenceEffects.ContainsKey(neighborTileData.TileLayer))
                {
                    ContextSpritesTemp[dir] = SbGridMapDataController.Instance.impactedContextSprite;
                    continue; // one sprite can set.
                }
            }

            return ContextSpritesTemp;
        }

        private static void DisplayBlueprint(Vector2 screenPos)
        {
            var worldPos = Registry<Camera>.GetFirst()
                .ScreenToWorldPoint(screenPos)
                .With(z: 0);

            var currentTilePos = (Vector2Int)SbGridMapSystem.Instance.gridTilemap.WorldToCell(worldPos);

            if (preTilePos == currentTilePos) return;

            isTileValid = SbGridMapSystem.Instance.ValidForCreate(currentTilePos);

            // display blueprint.

            TileContextDisplay.Display(currentTilePos,
                HandleCenterContext,
                GetTileContextSprites(currentTilePos));

            preTilePos = currentTilePos;
        }

        private static void HandleCenterContext(SpriteRenderer spriteRenderer)
        {
            spriteRenderer.color = isTileValid
                ? Color.blue.With(a: 0.7f)
                : Color.red.With(a: 0.7f);
            spriteRenderer.sprite = currentBuildingPreset.buildingTile.sprite;
        }

        #region CONFIRM HANDLER

        private static Vector3 GetScreenWorldPos()
        {
            Camera workingCamera = Registry<Camera>.GetFirst();
            return workingCamera.ScreenToWorldPoint(Input.mousePosition);
        }

        private static void SpawnBuilding(Vector2Int position, BuildingPresetSo buildingPreset)
        {
            var spawnPos = SbGridMapSystem.Instance.gridTilemap.GetCellCenterWorld(position.ToVector3Int(0));

            var buildingRuntime = InitializeBuildingRuntime(position, buildingPreset, spawnPos);
            SbGridMapSystem.Instance.CreateAndWriteTile(position, buildingPreset.tileLayer, buildingRuntime);

            BLogger.Log($"[SbMapController] spawn {buildingPreset.buildingId} at {position}", category: "SB");
        }

        public static BuildingRuntime SpawnBuildingDirectlyForLoad(Vector2Int position, BuildingPresetSo buildingPreset)
        {
            var spawnPos = SbGridMapSystem.Instance.gridTilemap.GetCellCenterWorld(position.ToVector3Int(0));
            return InitializeBuildingRuntime(position, buildingPreset, spawnPos);
        }

        private static BuildingRuntime InitializeBuildingRuntime(Vector2Int position, BuildingPresetSo buildingPreset,
            Vector3 spawnPos)
        {
            var gObj = Instantiate(Instance.buildingPrefab, spawnPos, Quaternion.identity);
            var buildingRuntime = gObj.GetComponent<BuildingRuntime>();
            buildingRuntime.Setup(buildingPreset, position);
            return buildingRuntime;
        }

        private static bool ConfirmBuildingHandler()
        {
            var worldPos = GetScreenWorldPos();
            var tilePos = (Vector2Int)SbGridMapSystem.Instance.gridTilemap.WorldToCell(worldPos);

            // THAY ĐỔI: Nếu đặt SAI vị trí, không được thoát chế độ Blueprint mà phải cho người chơi đặt lại
            if (!SbGridMapSystem.Instance.ValidForCreate(tilePos))
            {
                return false;
            }

            // Xây dựng hợp lệ => Tiến hành gỡ phím Cancel nếu nó đang được bật
            if (currentCanCancel)
            {
                InputEventManager.Instance.UnRegistryKey(KeyCode.Mouse1); // Đổi từ LeftAlt sang Mouse1
            }

            ExecuteAttachedHandler(); // Xoá blueprint, trả quyền điều khiển map

            // Kiểm tra giới hạn số lượng (trừ MainBuilding vì MainBuilding được xử lý độc quyền ở trên rồi)
            if (!(currentBuildingPreset is MainBuildingPresetSo))
            {
                int currentBuildingCount = Game.Global.GamePropertiesRuntime.Instance.CurrentBuildingNumber.Value;
                if (currentBuildingCount >= Game.Global.GamePropertiesRuntime.Instance.MaxBuildingNumber.Value)
                {
                    // Can display error message here if you want. (Toast)
                    BLogger.Log($"Không thể xây dựng! Đã đạt giới hạn tối đa.", LogLevel.Warning, category: "SB");
                    // Vì đã ExecuteAttachedHandler, người chơi sẽ thoát Blueprint.
                    return true;
                }
            }

            SpawnBuilding(tilePos, currentBuildingPreset);
            SbGameplayController.ApplyCost(currentBuildingPreset.costBuilding.Data);

            return true;
        }

        #endregion

        private static void ExecuteAttachedHandler()
        {
            InputEventManager.Instance.enabled = false;
            InputEventManager.Instance.OnMouseMove -= DisplayBlueprint;

            MapInteractionHandler.Instance.enabled = true;

            if (currentTimeStop)
            {
                GameTimeController.SetGameToPreviousState();
            }
        }

        private static bool CancelBuildingItemEvent()
        {
            InputEventManager.Instance.UnRegistryKey(KeyCode.Mouse0);
            ExecuteAttachedHandler();
            return true;
        }
    }
}