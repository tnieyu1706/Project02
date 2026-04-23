using System.Collections.Generic;
using Game.BuildingGameplay;
using TnieYuPackage.DictionaryUtilities;
using TnieYuPackage.DesignPatterns; // Yêu cầu cho ISaveLoadData
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

namespace Game.StrategyBuilding
{
    public enum BuildingType
    {
    }

    public enum BuildingCategory
    {
        None, // not display in list.
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

        // CỜ XÁC ĐỊNH CÔNG TRÌNH CÓ DÙNG NÔNG DÂN KHÔNG
        public bool requireVillagers = true;

        public BuildingCategory buildingCategory;
        public SbTileLayer tileLayer;
        public Tile buildingTile;
        public SerializableActionCost costBuilding;

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
            buildingRuntime.behaviour = behaviour;

            // GỌI SETUP SAU KHI KHỞI TẠO XONG THAY VÌ ĐỂ TRONG CONSTRUCTOR
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

        // THÊM: Các thuộc tính để UI lấy dữ liệu hiển thị
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
    }

    public interface IBuildingBehaviour : IBuildingUI, IBuildingImpacted, ISaveLoadData<BuildingBehaviourSaveData>
    {
        BuildingPresetSo Preset { get; }
        ActionCost UpgradeCostRuntime { get; }

        void Setup(); // THÊM HÀM SETUP VÀO INTERFACE
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