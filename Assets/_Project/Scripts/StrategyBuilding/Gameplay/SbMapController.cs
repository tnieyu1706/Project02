using System.Collections.Generic;
using BackboneLogger;
using EditorAttributes;
using TnieYuPackage.Core;
using TnieYuPackage.DesignPatterns;
using TnieYuPackage.Utils;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.StrategyBuilding
{
    public interface IBuildingTile
    {
        Vector3Int TilePosition { get; }
    }

    public class SbMapController : SingletonBehavior<SbMapController>
    {
        [SerializeField] private Tilemap spawnTilemap;

        //using prefab
        [SerializeField] private GameObject buildingPrefab;

        private readonly Dictionary<Vector3Int, BuildingRuntime> buildings = new();

        private BuildingPresetSo currentBuildingPreset;

        protected override void Awake()
        {
            dontDestroyOnLoad = false;

            base.Awake();
        }

        public BuildingRuntime Get(Vector3Int position) => buildings[position];

        public bool TryGetBuilding(Vector3Int position, out BuildingRuntime building)
        {
            return buildings.TryGetValue(position, out building);
        }

        public bool CheckExists(Vector3Int position) => buildings.ContainsKey(position);

        public Vector3Int GetCellFromWorld(Vector3 worldPos)
        {
            return spawnTilemap.WorldToCell(worldPos);
        }

        public BuildingRuntime SpawnBuilding(Vector3Int position, BuildingPresetSo buildingPreset)
        {
            var spawnPos = spawnTilemap.GetCellCenterWorld(position);

            //initialize & setup object runtime
            var gObj = Instantiate(buildingPrefab, spawnPos, Quaternion.identity);
            var buildingRuntime = gObj.GetComponent<BuildingRuntime>();
            buildingRuntime.SetPreset(buildingPreset);
            buildingRuntime.TilePosition = position;

            //setup tilemap
            spawnTilemap.SetTile(position, buildingRuntime.currentPreset.buildingTile);

            buildings[position] = buildingRuntime;

            BLogger.Log($"[SbMapController] spawn {buildingPreset.buildingId} at {position}", category: "SB");
            return buildingRuntime;
        }

        public void DestroyBuilding(Vector3Int position)
        {
            if (!TryGetBuilding(position, out BuildingRuntime building)) return;

            //delete tile in tilemap
            spawnTilemap.SetTile(position, null); //defaultTile

            //delete object runtime
            buildings.Remove(position);
            DestroyImmediate(building.gameObject);

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
            
            var worldPos = GetScreenWorldPos();
            var tilePos = Instance.GetCellFromWorld(worldPos);
            if (Instance.CheckExists(tilePos))
            {
                Debug.LogWarning($"{tilePos} has exist building");
                return;
            }
            Instance.SpawnBuilding(tilePos, Instance.currentBuildingPreset);
            Instance.currentBuildingPreset.costBuilding.CollectCost();

            InputEventManager.Instance.enabled = false;
        }

        private static void CancelBuildingItemEvent()
        {
            InputEventManager.Instance.UnRegistryKey(KeyCode.Mouse0);
            InputEventManager.Instance.enabled = false;
        }

        //test
        [Button]
        private void TestDestroy(Vector3Int position)
        {
            DestroyBuilding(position);
        }
    }
}