using System.Collections.Generic;
using Game.BaseGameplay;
using Game.BuildingGameplay;
using Game.StrategyBuilding;
using TnieYuPackage.DesignPatterns;
using TnieYuPackage.Utils;
using UnityEngine;

namespace Game.Global
{
    public class GamePropertiesRuntime : SingletonBehavior<GamePropertiesRuntime>
    {
        #region BuildingGameplay Properties

        public float GeneralResourceReceivedScale { get; set; } = 1f;

        public Dictionary<ResourceType, float> ResourceReceivedScaleDict { get; } =
            new Dictionary<ResourceType, float>()
            {
                { ResourceType.Coin, 1f },
                { ResourceType.Wood, 1f },
                { ResourceType.Stone, 1f },
                { ResourceType.Food, 1f }
            };

        public Dictionary<BuildingType, bool> UnlockBuildingTypeDict { get; } = new Dictionary<BuildingType, bool>()
        {
        };

        [field: SerializeField]
        public ObservableValue<int> MaxBuildingNumber { get; set; } = new ObservableValue<int>(5);

        [field: SerializeField]
        public ObservableValue<int> CurrentBuildingNumber { get; set; } = new ObservableValue<int>(0);

        #endregion

        //need handle defense | attack data handler.

        #region BaseGameplay Properties

        public float DamageScale { get; set; } = 1f;
        public float DefenseScale { get; set; } = 1f;

        #endregion

        #region Defense Properties

        public float ReceiveMoneyScale { get; set; } = 1f;
        public float RefundScale { get; set; } = 1f;

        public Dictionary<TowerType, float> TowerTypeDamageScaleDict { get; } = new Dictionary<TowerType, float>()
        {
            { TowerType.Archer, 1f },
            { TowerType.Barrack, 1f },
            { TowerType.Magic, 1f },
            { TowerType.Cannon, 1f }
        };

        public Dictionary<TowerLevel, bool> UnlockTowerLevelDict { get; } = new Dictionary<TowerLevel, bool>()
        {
            { TowerLevel.Level0, true },
            { TowerLevel.Level1, true },
            { TowerLevel.Level2, false },
            { TowerLevel.Level3, false },
        };

        public Dictionary<TowerType, bool> UnlockTowerTypeDict { get; } = new Dictionary<TowerType, bool>()
        {
            { TowerType.Archer, true },
            { TowerType.Barrack, true },
            { TowerType.Magic, false },
            { TowerType.Cannon, false }
        };

        #endregion

        #region Attack Properties

        public int MaxEntityPerWave { get; set; } = 10;

        public Dictionary<ArmyType, float> ArmyDamageScaleDict { get; } = new Dictionary<ArmyType, float>()
        {
            { ArmyType.Melee, 1f },
            { ArmyType.Range, 1f },
            { ArmyType.Strong, 1f },
            { ArmyType.Quick, 1f }
        };

        public Dictionary<ArmyType, float> ArmyDefenseScaleDict { get; } = new Dictionary<ArmyType, float>()
        {
            { ArmyType.Melee, 1f },
            { ArmyType.Range, 1f },
            { ArmyType.Strong, 1f },
            { ArmyType.Quick, 1f }
        };

        public Dictionary<ArmyType, float> ArmyHealthScaleDict { get; } = new Dictionary<ArmyType, float>()
        {
            { ArmyType.Melee, 1f },
            { ArmyType.Range, 1f },
            { ArmyType.Strong, 1f },
            { ArmyType.Quick, 1f }
        };

        public Dictionary<ArmyType, float> ArmySpeedScaleDict { get; } = new Dictionary<ArmyType, float>()
        {
            { ArmyType.Melee, 1f },
            { ArmyType.Range, 1f },
            { ArmyType.Strong, 1f },
            { ArmyType.Quick, 1f }
        };

        #endregion

        [field: SerializeField] public ObservableValue<int> SkillPoints { get; set; } = new ObservableValue<int>(4);
    }
}