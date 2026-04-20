using System;
using System.Collections.Generic;
using Game.BaseGameplay;
using Game.Global;

namespace Game.TowerDefense
{
    public class TdGameplayCalculator : BaseGameplayCalculator
    {
        public override float GlobalDamageScale => GamePropertiesRuntime.Instance.DamageScale;
        public override float GlobalDefenseScale => 1f;
        public override float ReceivedMoneyScale => GamePropertiesRuntime.Instance.ReceiveMoneyScale;
        protected override Dictionary<TowerType, float> TowerDamageScales { get; } = new();
        protected override Dictionary<ArmyType, ArmyPropertyScaler> ArmyPropertyScalerDict { get; } = new();

        protected override void Awake()
        {
            base.Awake();
            
            InitializeProperties();
        }

        private void InitializeProperties()
        {
            TowerDamageScales.Clear();
            foreach (var towerScale in GamePropertiesRuntime.Instance.TowerTypeDamageScaleDict)
            {
                TowerDamageScales[towerScale.Key] = 1f;
            }

            // not get benefit from RuntimeProperties
            ArmyPropertyScalerDict.Clear();
            foreach (var armyType in Enum.GetValues(typeof(ArmyType)))
            {
                ArmyPropertyScalerDict[(ArmyType)armyType] = new ArmyPropertyScaler();
            }
        }
    }
}