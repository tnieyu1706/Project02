using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EditorAttributes;
using Game.BaseGameplay;
using Game.Global;
using Game.StrategyBuilding;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TnieYuPackage.DesignPatterns;
using TnieYuPackage.Utils;
using UnityEngine;

namespace Game.BuildingGameplay
{
    [DefaultExecutionOrder(-10)]
    public class SbGameplayController : SingletonBehavior<SbGameplayController>
    {
        public const float RATIO_VILLAGER_FOOD = 1f;

        public BuildingGameplayLevel currentLevel;

        [Header("Starting Settings")]
        [Tooltip("Gán Scriptable Object của Nhà Chính vào đây để tự động đặt ra khi bắt đầu game")]
        public MainBuildingPresetSo startMainBuildingPreset;

        #region PROPERTIES

        public VillagerDataManager VillagerData = new();

        private Dictionary<ResourceType, ObservableValue<float>> ResourceStorage { get; } = new()
        {
            { ResourceType.Coin, new(60) },
            { ResourceType.Wood, new(35) },
            { ResourceType.Stone, new(25) },
            { ResourceType.Food, new(25) }
        };

        public Dictionary<ResourceType, ObservableValue<float>> IncrementResources { get; } =
            new Dictionary<ResourceType, ObservableValue<float>>()
            {
                { ResourceType.Coin, new ObservableValue<float>(1f) },
                { ResourceType.Wood, new ObservableValue<float>(0.6f) },
                { ResourceType.Stone, new ObservableValue<float>(0.3f) },
                { ResourceType.Food, new ObservableValue<float>(0.4f) },
            };

        public Dictionary<LimitResourceType, ObservableValue<int>> LimitResourceStorage { get; } = new()
        {
            { LimitResourceType.MaxWood, new(40) },
            { LimitResourceType.MaxStone, new(30) },
            { LimitResourceType.MaxFood, new(40) }
        };

        private Dictionary<ArmyType, ObservableValue<int>> ArmyStorage { get; } = new()
        {
            { ArmyType.Melee, new ObservableValue<int>(0) },
            { ArmyType.Range, new ObservableValue<int>(0) },
            { ArmyType.Strong, new ObservableValue<int>(0) },
            { ArmyType.Quick, new ObservableValue<int>(0) }
        };

        public static event Action OnResourceChanged;
        public static event Action OnActiveBuildingApplyResource;

        #endregion

        [Serializable]
        public class VillagerDataManager : ISaveLoadData<JObject>
        {
            [field: SerializeField] public ObservableValue<int> MaxVillagers { get; set; } = new(10);
            [field: SerializeField] public ObservableValue<int> CurrentVillagers { get; set; } = new(4);
            [field: SerializeField] public ObservableValue<int> UsedVillagers { get; set; } = new(0);

            public int RemainingVillagers => CurrentVillagers.Value - UsedVillagers.Value;

            public int AddVillagers(int amount)
            {
                var next = Mathf.Clamp(CurrentVillagers.Value + amount, 0, MaxVillagers.Value);
                if (next != CurrentVillagers.Value)
                {
                    // using building decrease food instead of depending on current villager exists.
                    // var preVillagers = CurrentVillagers.Value;
                    CurrentVillagers.Value = next;
                    // Instance.ResourceStorage[ResourceType.Food].Value -= (next - preVillagers) * RATIO_VILLAGER_FOOD;
                }

                return next - CurrentVillagers.Value;
            }

            public int UseVillagers(int amount)
            {
                var canUse = Mathf.Clamp(amount, 0, RemainingVillagers);
                if (canUse > 0)
                    UsedVillagers.Value += canUse;

                return canUse;
            }

            public int RefundVillagers(int amount)
            {
                var canRefund = Mathf.Clamp(amount, 0, UsedVillagers.Value);
                if (canRefund > 0)
                    UsedVillagers.Value -= canRefund;

                return canRefund;
            }

            public JObject SaveData()
            {
                return new JObject
                {
                    ["Max"] = MaxVillagers.Value,
                    ["Current"] = CurrentVillagers.Value,
                    ["Used"] = UsedVillagers.Value
                };
            }

            public void BindData(JObject data)
            {
                if (data == null) return;

                if (data.TryGetValue("Max", out var max)) MaxVillagers.Value = max.Value<int>();
                if (data.TryGetValue("Current", out var current)) CurrentVillagers.Value = current.Value<int>();
                if (data.TryGetValue("Used", out var used)) UsedVillagers.Value = used.Value<int>();
            }
        }

        protected override void Awake()
        {
            dontDestroyOnLoad = false;
            base.Awake();
            VillagerData ??= new VillagerDataManager();
        }

        public void CreateGameplay(BuildingGameplayLevel level)
        {
            SbTimeController.Instance.Init();
            SetupGameplay(level);
            SbGridMapRegister.Instance.RegisterEnvironmentMaps();

            // THÊM ĐOẠN NÀY ĐỂ ÉP NGƯỜI CHƠI XÂY NHÀ CHÍNH KHÔNG ĐƯỢC HUỶ
            if (startMainBuildingPreset != null)
            {
                SbSpawnBuildingSystem.StartBuilding(startMainBuildingPreset, canCancel: false, timeStop: true);
            }
        }

        public void SetupGameplay(BuildingGameplayLevel level)
        {
            currentLevel = level;
        }

        #region SUPPORTS

        public static void ApplyResourceIncrement()
        {
            foreach (var increment in Instance.IncrementResources)
            {
                AddResource(increment.Key, CalculateResourceAmount(increment.Key, increment.Value.Value));
            }

            Instance.VillagerData.AddVillagers(1);
            OnResourceChanged?.Invoke();
            OnActiveBuildingApplyResource?.Invoke();
        }

        private static float CalculateResourceAmount(ResourceType resourceType, float amount)
        {
            return Mathf.RoundToInt(
                amount
                * GamePropertiesRuntime.Instance.GeneralResourceReceivedScale
                * GamePropertiesRuntime.Instance.ResourceReceivedScaleDict[resourceType]
                * 10) / 10f;
        }

        private static void AddResource(ResourceType type, float value)
        {
            var observable = Instance.ResourceStorage[type];
            var current = observable.Value;
            var max = type.GetLimitResourceType() is var limitType && limitType != LimitResourceType.None
                ? Instance.LimitResourceStorage[limitType].Value
                : int.MaxValue;

            var next = Mathf.Clamp(current + value, 0, max);
            if (!Mathf.Approximately(current, next))
                observable.Value = next;
        }

        public static void AddResourceAndRefresh(ResourceType resourceType, float value)
        {
            AddResource(resourceType, value);
            OnResourceChanged?.Invoke();
        }

        public static ObservableValue<int> GetObservableArmy(ArmyType armyType) => Instance.ArmyStorage[armyType];

        public static ObservableValue<float> GetObservableResource(ResourceType resourceType) =>
            Instance.ResourceStorage[resourceType];

        public static bool AddArmy(ArmyType type, int value)
        {
            if (value < 0) return false;
            Instance.ArmyStorage[type].Value += value;
            return true;
        }

        public static bool ValidateCost(ActionCost cost)
        {
            foreach (var resourceCost in cost.ResourceCosts)
            {
                if (Instance.ResourceStorage[resourceCost.Key].Value < resourceCost.Value) return false;
            }

            return true;
        }

        public static void ApplyCost(ActionCost cost)
        {
            foreach (var resourceCost in cost.ResourceCosts)
            {
                Instance.ResourceStorage[resourceCost.Key].Value -= resourceCost.Value;
            }

            OnResourceChanged?.Invoke();
        }

        public static void RefundCost(ActionCost cost)
        {
            foreach (var resourceCost in cost.ResourceCosts)
            {
                Instance.ResourceStorage[resourceCost.Key].Value += resourceCost.Value;
            }

            OnResourceChanged?.Invoke();
        }

        public static void RevalidateResourceLimits()
        {
            bool isChanged = false;
            foreach (var kvp in Instance.ResourceStorage)
            {
                ResourceType type = kvp.Key;
                LimitResourceType limitType = type.GetLimitResourceType();

                if (limitType != LimitResourceType.None)
                {
                    int max = Instance.LimitResourceStorage[limitType].Value;

                    if (kvp.Value.Value > max)
                    {
                        kvp.Value.Value = max;
                        isChanged = true;
                    }
                }
            }

            if (isChanged)
            {
                OnResourceChanged?.Invoke();
            }
        }

        public Dictionary<ArmyType, int> GetArmyStorageAsUsing()
        {
            var result = Instance.ArmyStorage.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Value);
            foreach (var observableArmy in Instance.ArmyStorage.Values)
            {
                observableArmy.Value = 0;
            }

            return result;
        }

        #endregion

        #region SAVE-LOAD

        // ... Mọi đoạn Save Load bên dưới giữ nguyên y hệt logic hiện tại của bạn

        [Button]
        private void SaveManual() => SaveAll();

        [Button]
        private void LoadManual() => LoadAll();

        public async void SaveAll()
        {
            JObject data = new JObject();

            var gridMapSaveData = SbGridMapSystem.Instance.SaveData();
            data["gridMapSaveData"] = JObject.FromObject(gridMapSaveData, GameJsonSettings.GetJsonSerializer());

            var timeControllerData = SbTimeController.Instance.SaveData();
            data["timeControllerData"] = JObject.FromObject(timeControllerData, GameJsonSettings.GetJsonSerializer());

            data["VillagerData"] = VillagerData.SaveData();

            JObject resourceStorageJObject = new JObject();
            foreach (var resource in ResourceStorage)
            {
                resourceStorageJObject[resource.Key.ToString()] = resource.Value.Value;
            }

            data["ResourceStorage"] = resourceStorageJObject;

            JObject armyStorageJObject = new JObject();
            foreach (var army in ArmyStorage)
            {
                armyStorageJObject[army.Key.ToString()] = army.Value.Value;
            }

            data["ArmyStorage"] = armyStorageJObject;

            if (!File.Exists(currentLevel.saveFilePath)) return;

            await File.WriteAllTextAsync(currentLevel.saveFilePath, data.ToString(Formatting.Indented));
        }

        public async void LoadAll()
        {
            if (!File.Exists(currentLevel.saveFilePath)) return;

            string json = await File.ReadAllTextAsync(currentLevel.saveFilePath);
            JObject jObject = JObject.Parse(json);

            if (jObject.TryGetValue("gridMapSaveData", out JToken gridMapToken))
            {
                var gridMapData =
                    JsonConvert.DeserializeObject<GridMapSaveData>(gridMapToken.ToString(), GameJsonSettings.Create());
                SbGridMapSystem.Instance.BindData(gridMapData);
            }

            if (jObject.TryGetValue("timeControllerData", out JToken timeControllerToken))
            {
                var timeData = JsonConvert.DeserializeObject<TimeControllerSaveData>(timeControllerToken.ToString(),
                    GameJsonSettings.Create());
                SbTimeController.Instance.BindData(timeData);
            }

            if (jObject.TryGetValue("VillagerData", out JToken villagerToken) && villagerToken is JObject vObj)
            {
                VillagerData.BindData(vObj);
            }

            if (jObject.TryGetValue("ResourceStorage", out JToken resourceToken) &&
                resourceToken is JObject resourceObj)
            {
                foreach (var resource in resourceObj.Properties())
                {
                    ResourceStorage[Enum.Parse<ResourceType>(resource.Name)].Value = resource.Value.Value<float>();
                }
            }

            if (jObject.TryGetValue("ArmyStorage", out JToken armyToken) && armyToken is JObject armyObj)
            {
                foreach (var army in armyObj.Properties())
                {
                    ArmyStorage[Enum.Parse<ArmyType>(army.Name)].Value = army.Value.Value<int>();
                }
            }
        }

        #endregion
    }
}