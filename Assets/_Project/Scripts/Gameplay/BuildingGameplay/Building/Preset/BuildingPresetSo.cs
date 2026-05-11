using System.Collections.Generic;
using Game.BuildingGameplay;
using Reflex.Extensions;
using SoundSystem.Core;
using TnieYuPackage.DictionaryUtilities;
using TnieYuPackage.DesignPatterns; 
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

namespace Game.StrategyBuilding
{
    public enum BuildingType { }

    public enum BuildingCategory
    {
        None,
        WareHouse,
        Civilian,
        Military,
        ResourceProduction,
        ResourceStorage,
        Research,
    }

    public abstract class BuildingPresetSo : ScriptableObject
    {
        [Header("Basic Info")] public string buildingId;
        public int defaultMaxVillagersCanUse = 1;
        public bool requireVillagers = true;

        // THÊM: Thời gian chờ xây dựng (tính bằng giây). 0 = Xây xong ngay lập tức
        [Header("Construction")]
        public float buildWaitingTime = 5f; 

        public BuildingCategory buildingCategory;
        public SbTileLayer tileLayer;
        public Tile buildingTile;
        public SerializableActionCost costBuilding;

        [Header("Effect")] public SoundData sfxData;
        [Header("UI")] public List<StyleSheet> styleSheets;

        [Header("InfluenceEffects")] [SerializeField]
        private SerializableDictionaryAbstract<SbTileLayer, IBuildingInfluenceEffect> influenceEffects;

        [Header("Upgrade")] [SerializeField] private SerializableActionCost defaultUpgradeCost;
        [SerializeField] private SerializableActionCost incrementUpgradeCost;

        [SerializeField] private SerializableDictionary<ResourceType, float> consumedResources;

        public Dictionary<SbTileLayer, IBuildingInfluenceEffect> InfluenceEffects => influenceEffects.Dictionary;
        public ActionCost DefaultUpgradeCost => defaultUpgradeCost.CloneData;
        public ActionCost IncrementUpgradeCost => incrementUpgradeCost.CloneData;
        public Dictionary<ResourceType, float> ConsumedResources => consumedResources.Dictionary;

        public void InitBehaviour(BuildingRuntime buildingRuntime, Vector2Int pos)
        {
            var behaviour = CreateBehaviour(pos);
            var container = buildingRuntime.gameObject.GetClosestContainer();
            container.InjectObject(behaviour);
            buildingRuntime.behaviour = behaviour;

            behaviour.Setup();
            behaviour.RefreshBehaviour();
        }

        protected abstract IBuildingBehaviour CreateBehaviour(Vector2Int pos);

        public void DestroyBehaviour(BuildingRuntime buildingRuntime)
        {
            buildingRuntime.behaviour.DestroyBehaviour();
            buildingRuntime.behaviour = null;
        }
    }

    public interface IBuildingInfluenceEffect
    {
        void ApplyEffect(IBuildingImpacted impacted);
        void RemoveEffect(IBuildingImpacted impacted);
        string EffectName { get; }
        Color EffectColor { get; }
        string GetEffectValue();
    }

    public interface IBuildingUI
    {
        void AttachUIToPanel(VisualElement root);
        void DetachUIFromPanel(VisualElement root);
    }

    [System.Serializable]
    public class BuildingBehaviourSaveData
    {
        public int CurrentUpgradeLevel;
        public int UsedVillagers;
        public int MaxVillagersCanUse;
        
        // THÊM: Lưu trữ trạng thái xây dựng
        public bool IsUnderConstruction;
        public float RemainingBuildTime;
    }

    public interface IBuildingBehaviour : IBuildingUI, IBuildingImpacted, ISaveLoadData<BuildingBehaviourSaveData>
    {
        BuildingPresetSo Preset { get; }
        ActionCost UpgradeCostRuntime { get; }
        
        // THÊM: Để GridMap kiểm tra xem công trình đã hoạt động chưa
        bool IsUnderConstruction { get; } 

        void Setup(); 
        void RefreshBehaviour();
        void DestroyBehaviour();
    }

    public interface IBuildingBehaviour<out TPreset> : IBuildingBehaviour
        where TPreset : BuildingPresetSo
    {
        BuildingPresetSo IBuildingBehaviour.Preset => ActualPreset;
        TPreset ActualPreset { get; }
    }
}