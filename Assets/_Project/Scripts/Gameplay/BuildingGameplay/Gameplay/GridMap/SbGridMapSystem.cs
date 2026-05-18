using System.Collections.Generic;
using EditorAttributes;
using Game.BuildingGameplay;
using TnieYuPackage.DesignPatterns;
using TnieYuPackage.Utils;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reflex.Attributes;
using Reflex.Extensions;
using SoundSystem.Core;

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
        private SbTileLayerDataManager tileLayerDataManager;

        public Vector2Int TilePosition { get; private set; }
        public SbTileLayer TileLayer { get; private set; }
        public Dictionary<Vector2Int, IBuildingImpacted> ImpactedBuildings { get; private set; }

        public BuildingRuntime BuildingRuntime { get; private set; }

        public SbGridTileData(SbTileLayerDataManager tileLayerDataManager)
        {
            this.tileLayerDataManager = tileLayerDataManager;
        }

        public void Setup(Vector2Int tilePosition, SbTileLayer tileLayer, BuildingRuntime buildingRuntime)
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

    public class SbGridMapSystem : Singleton<SbGridMapSystem>, IGridMapSystem<Vector2Int, SbGridTileData>,
        ISaveLoadData<GridMapSaveData>
    {
        [Inject] BuildingPresetManager buildingPresetManager;
        [Inject] SbTileLayerDataManager tileLayerDataManager;
        [Inject] private SfxManager sfxManager;

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

            var container = gameObject.GetClosestContainer();
            var newTile = container.Instantiate<SbGridTileData>();
            newTile.Setup(pos, layer, building);
            WriteTile(pos, newTile);
            return newTile;
        }

        public void WriteTile(Vector2Int pos, SbGridTileData data)
        {
            GridMap[pos] = data;

            // Có thể mở rộng sau này: nếu đang xây dựng thì hiển thị Sprite đang xây (scaffolding)
            gridTilemap.SetTile((Vector3Int)pos,
                data.BuildingRuntime != null
                    ? data.BuildingRuntime.currentPreset.buildingTile
                    : tileLayerDataManager.Refs[data.TileLayer].tile);

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

            if (tileData.BuildingRuntime != null)
            {
                Game.Global.GamePropertiesRuntime.Instance.CurrentBuildingNumber.Value--;
                
                // THÊM: Phát âm thanh phá huỷ (chỉ phát nếu ô bị xoá là công trình)
                if (buildingPresetManager?.destroySfx != null)
                {
                    sfxManager?.PlayVfx(buildingPresetManager.destroySfx).Forget();
                }
            }

            GridMap.Remove(pos);
            HandleInfluenceOnDestroy(tileData);
            gridTilemap.SetTile((Vector3Int)pos, null);

            return true;
        }

        #region Influence Controller

        // THÊM: Hàm này cho phép gọi công khai từ BaseBuildingBehaviour khi hoàn thành xây dựng
        public void UpdateInfluenceForTile(Vector2Int pos)
        {
            if (GridMap.TryGetValue(pos, out var tileData))
            {
                HandleInfluenceOnCreate(tileData);
            }
        }

        private void HandleInfluenceOnCreate(SbGridTileData centerTile)
        {
            var centerBuilding = centerTile.BuildingRuntime?.behaviour;

            // KIỂM TRA: Nếu công trình trung tâm ĐANG XÂY -> Không tính/phát Influence
            if (centerBuilding != null && centerBuilding.IsUnderConstruction) return;

            foreach (var dir in Vector2IntUtils.Get8DirectionalVectors())
            {
                if (!GridMap.TryGetValue(centerTile.TilePosition + dir, out var neighborTile)) continue;

                var neighborBuilding = neighborTile.BuildingRuntime?.behaviour;

                // KIỂM TRA: Nếu công trình kế bên ĐANG XÂY -> Bỏ qua
                if (neighborBuilding != null && neighborBuilding.IsUnderConstruction) continue;

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
                    BuildingId = tileData.BuildingRuntime != null
                        ? tileData.BuildingRuntime.currentPreset.buildingId
                        : string.Empty,
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
                    var preset = buildingPresetManager.Refs[dto.BuildingId];

                    if (preset != null)
                    {
                        buildingRuntime = SbSpawnBuildingSystem.SpawnBuildingDirectlyForLoad(dto.Position, preset);

                        // QUAN TRỌNG: Gọi BindData cho Behaviour TRƯỚC khi ghi Map
                        // Để khi WriteTile -> HandleInfluenceOnCreate có thể check đúng biến 'IsUnderConstruction'
                        if (buildingRuntime.behaviour != null && dto.BehaviourData != null)
                        {
                            var behaviourData =
                                dto.BehaviourData.ToObject<BuildingBehaviourSaveData>(PolymorphicSerializer);
                            if (behaviourData != null)
                            {
                                buildingRuntime.behaviour.BindData(behaviourData);
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning(
                            $"[SbGridMapSystem] Load thất bại: Không tìm thấy Building Preset có ID = {dto.BuildingId}");
                    }
                }

                // Ghi vào Map (Tự động tính toán lại Influence với trạng thái IsUnderConstruction đã chuẩn)
                CreateAndWriteTile(dto.Position, dto.TileLayer, buildingRuntime);
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
            Game.Global.GamePropertiesRuntime.Instance.CurrentBuildingNumber.Value = 0;
        }

        #endregion
    }
}