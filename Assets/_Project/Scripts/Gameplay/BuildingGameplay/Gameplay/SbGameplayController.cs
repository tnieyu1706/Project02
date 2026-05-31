using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using _Project.Scripts.Gameplay.Global.UI.WorldMap;
using Cysharp.Threading.Tasks;
using EditorAttributes;
using Game.BaseGameplay;
using Game.Global;
using Game.StrategyBuilding;
using Gameplay.Global;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reflex.Attributes;
using TnieYuPackage.DesignPatterns;
using TnieYuPackage.Utils;
using UnityEngine;

namespace Game.BuildingGameplay
{
    [DefaultExecutionOrder(-10)]
    public class SbGameplayController : Singleton<SbGameplayController>
    {
        public const float RATIO_VILLAGER_FOOD = 1f;
        public const int MAX_HEALTH = 3;

        [Inject] private GameplayTransition transition;

        [ReadOnly] public BuildingGameplayLevel currentLevel;
        private bool isCompleted;
        public ObservableValue<int> currentHealth;

        public static event Action OnLoseGame;
        public static event Action OnWinGame;

        [Header("Starting Settings")]
        [Tooltip("Gán Scriptable Object của Nhà Chính vào đây để tự động đặt ra khi bắt đầu game")]
        public MainBuildingPresetSo startMainBuildingPreset;

        public event Action OnPreMainBuildingSpawn;
        public event Action OnPostMainBuildingSpawn;

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
                { ResourceType.Coin, new ObservableValue<float>(1.6f) },
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

        public event Action OnResourceChanged;
        public event Action OnActiveBuildingApplyResource;

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
                    CurrentVillagers.Value = next;
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
                    ["Current"] = CurrentVillagers.Value
                    // ĐÃ SỬA: Không lưu UsedVillagers nữa, vì biến này sẽ được các Building tự đăng ký khi Load
                };
            }

            public void BindData(JObject data)
            {
                if (data == null) return;

                // Đảm bảo Reset Used về 0 trước khi load (đề phòng)
                UsedVillagers.Value = 0;

                if (data.TryGetValue("Max", out var max)) MaxVillagers.Value = max.Value<int>();
                if (data.TryGetValue("Current", out var current)) CurrentVillagers.Value = current.Value<int>();
            }
        }

        protected override void Awake()
        {
            base.Awake();
            VillagerData ??= new VillagerDataManager();

            if (GameplayTransition.DataManager != null && GameplayTransition.DataManager.CurrentBuildingLevel != null)
            {
                currentLevel = GameplayTransition.DataManager.CurrentBuildingLevel;
            }
        }

        private void OnEnable()
        {
            currentHealth.OnValueChanged += OnCurrentHealthChanged;
            OnWinGame += HandleOnGameEndDefault;
            OnLoseGame += HandleOnGameEndDefault;
        }

        private void OnCurrentHealthChanged(int changedValue)
        {
            if (changedValue > 0) return;
            OnLoseGame?.Invoke();
        }

        private static void HandleOnGameEndDefault()
        {
            if (GameplayTransition.DataManager != null)
            {
                GameplayTransition.DataManager.SubmitLevelResult(Instance.currentHealth.Value);
            }
        }

        private void OnDisable()
        {
            currentHealth.OnValueChanged -= OnCurrentHealthChanged;
            OnWinGame -= HandleOnGameEndDefault;
            OnLoseGame -= HandleOnGameEndDefault;
        }

        public async void CreateGameplay(BuildingGameplayLevel level)
        {
            currentHealth.Value = MAX_HEALTH;

            SbTimeController.Instance.Init();
            SbGridMapRegister.Instance.RegisterEnvironmentMaps();

            if (startMainBuildingPreset != null)
            {
                try
                {
                    OnPreMainBuildingSpawn?.Invoke();
                    await SbSpawnBuildingSystem.StartBuilding(startMainBuildingPreset, canCancel: false,
                        timeStop: true);
                }
                catch (OperationCanceledException)
                {
                }
                finally
                {
                    OnPostMainBuildingSpawn?.Invoke();
                }
            }

            foreach (var eventData in currentLevel.events)
            {
                EventData.SetupAwardsRandomized(eventData.data);
            }
        }

        public void SetLevel(BuildingGameplayLevel level)
        {
            currentLevel = level;
        }

        #region SUPPORTS

        // THÊM: Xử lý dọn dẹp Lifecycle triệt để
        public void CleanUpGameplay()
        {
            Debug.Log("[SbGameplayController] Tiến hành CleanUp Gameplay trước khi thoát...");
            if (SbGridMapSystem.HasInstance)
            {
                SbGridMapSystem.Instance.ClearMap();
            }
            // Clear các event listener cục bộ nếu cần thiết ở đây để tránh leak
        }

        public static void RefreshEvents()
        {
            foreach (var eventData in Instance.currentLevel.events)
            {
                if (!eventData.data.isCompleted) return;
            }

            OnWinGame?.Invoke();
        }

        public static void ApplyResourceIncrement()
        {
            foreach (var increment in Instance.IncrementResources)
            {
                AddResource(increment.Key, CalculateResourceAmount(increment.Key, increment.Value.Value));
            }

            // ĐÃ SỬA: Logic kiểm tra Food (Thức ăn) để tăng hoặc giảm dân làng (Nạn đói)
            float currentFood = Instance.ResourceStorage[ResourceType.Food].Value;
            if (currentFood > 0)
            {
                Instance.VillagerData.AddVillagers(1);
            }
            else
            {
                HandleStarvation();
            }

            Instance.OnResourceChanged?.Invoke();
            Instance.OnActiveBuildingApplyResource?.Invoke();
        }

        // THÊM: Logic xử lý Nạn đói (Giảm dân làng, rút ngẫu nhiên từ công trình nếu cần)
        private static void HandleStarvation()
        {
            var villagerData = Instance.VillagerData;
            if (villagerData.CurrentVillagers.Value <= 0) return; // Không còn ai để giảm

            // 1. Nếu có dân làng rảnh rỗi (idle), chỉ việc trừ đi 1 dân làng rảnh
            if (villagerData.RemainingVillagers > 0)
            {
                villagerData.AddVillagers(-1);
                return;
            }

            // 2. Nếu tất cả dân làng đều đang làm việc trong công trình, bắt buộc phải ép rút 1 người ra
            if (SbGridMapSystem.HasInstance)
            {
                // Tìm TẤT CẢ các công trình ĐANG CHỨA dân làng 
                // (Điều này tự động gom các loại nhà như IncreaseResource... vì chúng có UsedVillagers > 0)
                var workingBuildings = SbGridMapSystem.Instance.GridMap.Values
                    .Where(t => t.BuildingRuntime != null && t.BuildingRuntime.behaviour != null)
                    .Select(t => t.BuildingRuntime.behaviour)
                    .Where(b => b.UsedVillagers > 0)
                    .ToList();

                if (workingBuildings.Count > 0)
                {
                    // Chọn ngẫu nhiên 1 công trình
                    var randomBuilding = workingBuildings[UnityEngine.Random.Range(0, workingBuildings.Count)];

                    // Ép rút 1 dân làng ra (lúc này dân làng đó sẽ chuyển sang trạng thái rảnh)
                    bool removed = randomBuilding.ForceRemoveOneVillager();

                    if (removed)
                    {
                        // Sau khi rút ra thành công (đã hoàn trả về nhàn rỗi), ta trừ nó đi khỏi tổng
                        villagerData.AddVillagers(-1);
                        Debug.Log(
                            $"[Nạn đói] Food <= 0. Một dân làng đã chết khi đang làm việc tại {randomBuilding.Preset.buildingId}.");
                    }
                }
            }
        }

        private static float CalculateResourceAmount(ResourceType resourceType, float amount)
        {
            return amount
                   * GamePropertiesRuntime.Instance.GeneralResourceReceivedScale
                   * GamePropertiesRuntime.Instance.ResourceReceivedScaleDict[resourceType];
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
            Instance.OnResourceChanged?.Invoke();
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

            Instance.OnResourceChanged?.Invoke();
        }

        public static void RefundCost(ActionCost cost)
        {
            foreach (var resourceCost in cost.ResourceCosts)
            {
                Instance.ResourceStorage[resourceCost.Key].Value += resourceCost.Value;
            }

            Instance.OnResourceChanged?.Invoke();
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
                Instance.OnResourceChanged?.Invoke();
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

        public async void SaveAll()
        {
            JObject data = new JObject();

            JObject properties = new JObject()
            {
                ["CurrentHealth"] = Instance.currentHealth.Value
            };

            data["properties"] = properties;

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

            if (!File.Exists(currentLevel.TempFilePath)) return;

            await File.WriteAllTextAsync(currentLevel.TempFilePath, data.ToString(Formatting.Indented));
        }

        public async UniTask LoadAll()
        {
            Debug.Log($"[LoadAll] Attempting to load data from: {currentLevel.TempFilePath}");

            if (!File.Exists(currentLevel.TempFilePath))
            {
                Debug.LogError($"Temp file not found at path: {currentLevel.TempFilePath}");
                return;
            }

            string json = await File.ReadAllTextAsync(currentLevel.TempFilePath);
            JObject jObject = JObject.Parse(json);

            if (jObject.TryGetValue("properties", out JToken propertiesToken) &&
                propertiesToken is JObject propertiesObj)
            {
                if (propertiesObj.TryGetValue("CurrentHealth", out JToken healthToken))
                {
                    Instance.currentHealth.Value = healthToken.Value<int>();
                }
            }

            await UniTask.CompletedTask;

            // SỬA LỖI ĐẶC BIỆT QUAN TRỌNG: 
            // Đưa việc Load VillagerData và ResourceStorage LÊN TRƯỚC GridMap.
            // Để khi GridMap dựng công trình, quỹ dân và tài nguyên đã sẵn sàng để công trình đăng ký lấy!

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

            // Gắn GridMap SAU CÙNG
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
        }

        #endregion
    }
}