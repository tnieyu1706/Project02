using System.Collections.Generic;
using EditorAttributes;
using Game.BuildingGameplay;
using TnieYuPackage.DesignPatterns;
using TnieYuPackage.Utils;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq; // Cần thêm namespace này

namespace Game.StrategyBuilding
{
    public enum SbTileLayer
    {
        Tree,
        Stone,
        Water,
        Building,
        Factory,
        Storage
        //... Biome, Building, etc.
    }

    public static class SbTileLayerExtensions
    {
        public static bool HandleInteraction(this SbTileLayer layerTile)
        {
            switch (layerTile)
            {
                case SbTileLayer.Tree:
                    SbGameplayController.AddResourceAndRefresh(ResourceType.Wood, 10);
                    return true;
                case SbTileLayer.Stone:
                    SbGameplayController.AddResourceAndRefresh(ResourceType.Stone, 10);
                    return true;
                default: return false;
            }
        }
    }

    public class SbGridTileData : ITileInfluencer
    {
        public Vector2Int TilePosition { get; }
        public SbTileLayer TileLayer { get; }
        public Dictionary<Vector2Int, IBuildingImpacted> ImpactedBuildings { get; }

        public BuildingRuntime BuildingRuntime { get; }

        public SbGridTileData(Vector2Int tilePosition, SbTileLayer tileLayer, BuildingRuntime buildingRuntime)
        {
            TilePosition = tilePosition;
            TileLayer = tileLayer;
            BuildingRuntime = buildingRuntime;
            ImpactedBuildings = new();
        }

        public void OnDestroyed()
        {
            foreach (var impactedBuilding in ImpactedBuildings.Values)
            {
                if (impactedBuilding is not IBuildingBehaviour buildingBehaviour
                    || !buildingBehaviour.Preset.InfluenceEffects.TryGetValue(TileLayer, out var influenceEffect))
                    continue;

                influenceEffect.RemoveEffect(impactedBuilding);
            }
        }
    }

    public class SbGridMapSystem : SingletonBehavior<SbGridMapSystem>, IGridMapSystem<Vector2Int, SbGridTileData>,
        ISaveLoadData<GridMapSaveData>
    {
        // Khai báo một Serializer đặc biệt hỗ trợ Đa Hình (Polymorphism)
        // Nó sẽ tự động nhúng thêm thuộc tính "$type" vào chuỗi JSON để giữ lại class con.
        private static readonly JsonSerializer PolymorphicSerializer = new JsonSerializer
        {
            TypeNameHandling = TypeNameHandling.Objects,
            Formatting = Formatting.Indented
        };

        public Tilemap gridTilemap;
        public Dictionary<Vector2Int, SbGridTileData> GridMap { get; } = new();
        public List<Vector2Int> BlockMap { get; } = new();

        public bool ValidForCreate(Vector2Int pos) => !BlockMap.Contains(pos) && !GridMap.ContainsKey(pos);

        public SbGridTileData CreateAndWriteTile(Vector2Int pos, SbTileLayer layer, BuildingRuntime building = null)
        {
            if (!ValidForCreate(pos)) return null;

            var newTile = new SbGridTileData(pos, layer, building);
            WriteTile(pos, newTile);
            return newTile;
        }

        public void WriteTile(Vector2Int pos, SbGridTileData data)
        {
            GridMap[pos] = data;
            gridTilemap.SetTile((Vector3Int)pos,
                data.BuildingRuntime != null
                    ? data.BuildingRuntime.currentPreset.buildingTile
                    : SbTileLayerDataManager.Refs[data.TileLayer].tile);

            if (data.BuildingRuntime != null)
            {
                Game.Global.GamePropertiesRuntime.Instance.CurrentBuildingNumber.Value++;
            }

            HandleInfluenceOnCreate(data);
        }

        public SbGridTileData ReadTile(Vector2Int pos)
        {
            return GridMap.GetValueOrDefault(pos);
        }

        public bool DeleteTile(Vector2Int pos)
        {
            if (!GridMap.TryGetValue(pos, out var tileData)) return false;

            // Giảm số lượng công trình khi bị xoá
            if (tileData.BuildingRuntime != null)
            {
                Game.Global.GamePropertiesRuntime.Instance.CurrentBuildingNumber.Value--;
            }

            GridMap.Remove(pos);

            HandleInfluenceOnDestroy(tileData);

            gridTilemap.SetTile((Vector3Int)pos, null);

            return true;
        }

        #region Influence Controller

        private void HandleInfluenceOnCreate(SbGridTileData centerTile)
        {
            var centerBuilding = centerTile.BuildingRuntime?.behaviour;

            foreach (var dir in Vector2IntUtils.Get8DirectionalVectors())
            {
                if (!GridMap.TryGetValue(centerTile.TilePosition + dir, out var neighborTile)) continue;

                var neighborBuilding = neighborTile.BuildingRuntime?.behaviour;

                if (neighborBuilding != null &&
                    neighborBuilding.Preset.InfluenceEffects.TryGetValue(centerTile.TileLayer, out var neighborEffect))
                {
                    LinkInfluence(influencer: centerTile, impacted: neighborBuilding, dirToImpacted: dir);
                    neighborEffect.ApplyEffect(neighborBuilding);
                }

                if (centerBuilding != null &&
                    centerBuilding.Preset.InfluenceEffects.TryGetValue(neighborTile.TileLayer, out var centerEffect))
                {
                    LinkInfluence(influencer: neighborTile, impacted: centerBuilding, dirToImpacted: -dir);
                    centerEffect.ApplyEffect(centerBuilding);
                }
            }
        }

        private void LinkInfluence(ITileInfluencer influencer, IBuildingBehaviour impacted, Vector2Int dirToImpacted)
        {
            if (!influencer.ImpactedBuildings.ContainsKey(dirToImpacted))
            {
                influencer.ImpactedBuildings[dirToImpacted] = impacted;
                impacted.TileInfluencers[-dirToImpacted] = influencer;
            }
        }

        private void HandleInfluenceOnDestroy(SbGridTileData destroyedTile)
        {
            foreach (var kvp in destroyedTile.ImpactedBuildings.ToList())
            {
                var dirToImpacted = kvp.Key;
                var impactedBuilding = kvp.Value;
                if (impactedBuilding is IBuildingBehaviour buildingBehaviour
                    && buildingBehaviour.Preset.InfluenceEffects
                        .TryGetValue(destroyedTile.TileLayer, out var influenceEffect))
                {
                    influenceEffect.RemoveEffect(impactedBuilding);
                }

                UnlinkInfluence(influencer: destroyedTile, impacted: impactedBuilding, dirToImpacted: dirToImpacted);
            }

            var destroyedBuilding = destroyedTile.BuildingRuntime?.behaviour;
            if (destroyedBuilding == null) return;

            foreach (var kvp in destroyedBuilding.TileInfluencers.ToList())
            {
                var dirFromImpactedToInfluencer = kvp.Key;
                var influencerTile = kvp.Value;
                UnlinkInfluence(influencer: influencerTile, impacted: destroyedBuilding,
                    dirToImpacted: -dirFromImpactedToInfluencer);
            }

            destroyedBuilding.DestroyBehaviour();
            DestroyImmediate(destroyedTile.BuildingRuntime.gameObject);
        }

        private void UnlinkInfluence(ITileInfluencer influencer, IBuildingImpacted impacted, Vector2Int dirToImpacted)
        {
            if (influencer.ImpactedBuildings.ContainsKey(dirToImpacted))
            {
                influencer.ImpactedBuildings.Remove(dirToImpacted);
                impacted.TileInfluencers.Remove(-dirToImpacted);
            }
        }

        #endregion

        #region Save / Load System

        public GridMapSaveData SaveData()
        {
            var saveData = new GridMapSaveData();

            foreach (var kvp in GridMap)
            {
                var tileData = kvp.Value;
                var dto = new GridTileDTO
                {
                    Position = tileData.TilePosition,
                    TileLayer = tileData.TileLayer,
                    // Chỉ lưu Building ID nếu tile này là công trình (có BuildingRuntime)
                    BuildingId = tileData.BuildingRuntime != null
                        ? tileData.BuildingRuntime.currentPreset.buildingId
                        : string.Empty,

                    // Lấy SaveData từ behaviour và convert thẳng sang JObject (sử dụng PolymorphicSerializer)
                    BehaviourData = tileData.BuildingRuntime != null && tileData.BuildingRuntime.behaviour != null
                        ? JObject.FromObject(tileData.BuildingRuntime.behaviour.SaveData(),
                            PolymorphicSerializer)
                        : null
                };
                saveData.GridTiles.Add(dto);
            }

            return saveData;
        }

        public void BindData(GridMapSaveData saveData)
        {
            if (saveData == null) return;

            ClearMap();

            foreach (var dto in saveData.GridTiles)
            {
                BuildingRuntime buildingRuntime = null;

                if (!string.IsNullOrEmpty(dto.BuildingId))
                {
                    var preset = BuildingPresetManager.Instance.Refs[dto.BuildingId];

                    if (preset != null)
                    {
                        buildingRuntime = SbSpawnBuildingSystem.SpawnBuildingDirectlyForLoad(dto.Position, preset);
                    }
                    else
                    {
                        Debug.LogWarning(
                            $"[SbGridMapSystem] Load thất bại: Không tìm thấy Building Preset có ID = {dto.BuildingId}");
                    }
                }

                // Ghi vào Map (Tự động tính toán lại Influence)
                CreateAndWriteTile(dto.Position, dto.TileLayer, buildingRuntime);

                // Sau khi đã add vào Map, tiến hành Bind data cho Behaviour
                if (buildingRuntime != null && buildingRuntime.behaviour != null && dto.BehaviourData != null)
                {
                    // Truyền PolymorphicSerializer vào để nó đọc field "$type" và khởi tạo đúng instance của class con
                    var behaviourData = dto.BehaviourData.ToObject<BuildingBehaviourSaveData>(PolymorphicSerializer);
                    if (behaviourData != null)
                    {
                        buildingRuntime.behaviour.BindData(behaviourData);
                    }
                }
            }
        }

        public void ClearMap()
        {
            foreach (var tile in GridMap.Values)
            {
                if (tile.BuildingRuntime != null && tile.BuildingRuntime.gameObject != null)
                {
                    Destroy(tile.BuildingRuntime.gameObject);
                }

                gridTilemap.SetTile((Vector3Int)tile.TilePosition, null);
            }

            GridMap.Clear();

            // Reset số lượng công trình về 0 khi dọn dẹp map (chuẩn bị Load Map hoặc Restart)
            Game.Global.GamePropertiesRuntime.Instance.CurrentBuildingNumber.Value = 0;
        }

        #endregion
    }
}