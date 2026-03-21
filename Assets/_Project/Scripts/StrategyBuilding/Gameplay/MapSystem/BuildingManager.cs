using System;
using System.Collections.Generic;
using BackboneLogger;
using EditorAttributes;
using TnieYuPackage.Core;
using TnieYuPackage.GlobalExtensions;
using TnieYuPackage.Utils;
using UnityEngine;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

namespace Game.StrategyBuilding
{
    public interface IBuildingTile
    {
        Vector2Int TilePosition { get; }
    }

    [Serializable]
    public class BuildingManager
    {
        [SerializeField] private GameObject buildingPrefab;
        [SerializeField] private int zValue;

        private readonly Dictionary<Vector2Int, BuildingRuntime> buildings = new();

        private static BuildingManager ThisStatic => SbMapController.Instance.buildingManager;
        private static MapData MapDataSource => SbMapController.Instance.mapData;
        private static Tilemap TileMapSource => SbMapController.Instance.buildingTilemap;

        private BuildingPresetSo currentBuildingPreset;

        private Vector3Int GetVector3Pos(Vector2Int pos) => pos.ToVector3Int(zValue);

        public BuildingRuntime Get(Vector2Int position) => buildings[position];

        public bool TryGetBuilding(Vector2Int position, out BuildingRuntime building)
        {
            return buildings.TryGetValue(position, out building);
        }

        public bool CheckExists(Vector2Int position) => buildings.ContainsKey(position);

        #region METHODS

        public BuildingRuntime SpawnBuilding(Vector2Int position, BuildingPresetSo buildingPreset)
        {
            var spawnPos = TileMapSource.GetCellCenterWorld(GetVector3Pos(position));

            //initialize & setup object runtime
            var gObj = Object.Instantiate(buildingPrefab, spawnPos, Quaternion.identity);
            var buildingRuntime = gObj.GetComponent<BuildingRuntime>();
            buildingRuntime.Setup(buildingPreset, position);

            //setup: building & env context
            var buildingBehaviour = buildingRuntime.buildingBehaviour;

            var convenientNeighborTiles =
                SbMapController.Instance.mapData
                    .GetNeighbors(buildingRuntime.TilePosition, buildingBehaviour.ConvenientEnv);
            buildingBehaviour.ApplyConvenientNeighborTiles(convenientNeighborTiles);

            var adverseNeighborTiles = SbMapController.Instance.mapData
                .GetNeighbors(buildingRuntime.TilePosition, buildingBehaviour.AdverseEnv);
            buildingBehaviour.ApplyAdverseNeighborTiles(adverseNeighborTiles);

            //setup: tilemap
            TileMapSource.SetTile(GetVector3Pos(position), buildingRuntime.currentPreset.buildingTile);
            buildings[position] = buildingRuntime;

            BLogger.Log($"[SbMapController] spawn {buildingPreset.buildingId} at {position}", category: "SB");
            return buildingRuntime;
        }

        public void DestroyBuilding(Vector2Int position)
        {
            if (!TryGetBuilding(position, out BuildingRuntime building)) return;

            //pre-handlers
            foreach (var conPos in building.buildingBehaviour.ConvenientTiles)
            {
                MapDataSource.GetTileRef(conPos).BuildingTiles.Remove(position);
            }

            foreach (var adPos in building.buildingBehaviour.AdverseTiles)
            {
                MapDataSource.GetTileRef(adPos).BuildingTiles.Remove(position);
            }

            //handler: delete tile in tilemap
            TileMapSource.SetTile(position.ToVector3Int(zValue), null); //defaultTile

            //handler: delete object runtime
            buildings.Remove(position);
            Object.DestroyImmediate(building.gameObject);

            BLogger.Log($"[SbMapController] destroy building at {position}", category: "SB");
        }

        public void StartBuilding(BuildingPresetSo preset)
        {
            currentBuildingPreset = preset;

            InputEventManager.Instance.enabled = true;
            InputEventManager.Instance.RegistryOnce(KeyCode.Mouse0, ConfirmBuildingHandler);
            InputEventManager.Instance.RegistryOnce(KeyCode.LeftAlt, CancelBuildingItemEvent);
        }

        private static Vector3 GetScreenWorldPos()
        {
            Camera workingCamera = Registry<Camera>.GetFirst();
            return workingCamera.ScreenToWorldPoint(Input.mousePosition);
        }

        private static void ConfirmBuildingHandler()
        {
            InputEventManager.Instance.UnRegistryKey(KeyCode.LeftAlt);
            InputEventManager.Instance.enabled = false;

            var worldPos = GetScreenWorldPos();
            var tilePos = (Vector2Int)SbMapController.Instance.GetCellFromWorld(worldPos);

            if (!MapDataSource.Validate(tilePos) || MapDataSource.CheckExists(tilePos))
            {
                //current: only Log
                BLogger.Log($"Pos {tilePos} is exists in env map", LogLevel.Warning, category: "SB");
                return;
            }

            if (ThisStatic.CheckExists(tilePos))
            {
                Debug.LogWarning($"{tilePos} has exist building");
                return;
            }

            ThisStatic.SpawnBuilding(tilePos, ThisStatic.currentBuildingPreset);
            ThisStatic.currentBuildingPreset.costBuilding.CollectCost();
        }

        private static void CancelBuildingItemEvent()
        {
            InputEventManager.Instance.UnRegistryKey(KeyCode.Mouse0);
            InputEventManager.Instance.enabled = false;
        }

        #endregion

        [Button]
        private void TestDestroy(Vector2Int position)
        {
            DestroyBuilding(position);
        }
    }
}