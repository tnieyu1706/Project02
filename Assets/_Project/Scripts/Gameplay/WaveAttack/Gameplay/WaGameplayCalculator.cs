using System;
using System.Collections.Generic;
using System.Linq;
using Game.BaseGameplay;
using Game.Global;

namespace Game.WaveAttack
{
    public class WaGameplayCalculator : BaseGameplayCalculator
    {
        public override float GlobalDamageScale => GamePropertiesRuntime.Instance.DamageScale;
        public override float GlobalDefenseScale => 1f;
        public override float ReceivedMoneyScale => 1f;
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

            // not get benefit from RuntimeProperties
            foreach (var towerType in Enum.GetValues(typeof(TowerType)))
            {
                TowerDamageScales[(TowerType)towerType] = 1f;
            }

            ArmyPropertyScalerDict.Clear();
            var armyTypes = Enum.GetValues(typeof(ArmyType))
                .OfType<ArmyType>();

            var p = GamePropertiesRuntime.Instance;

            foreach (var armyType in armyTypes)
            {
                var propertyScaler = new ArmyPropertyScaler(
                    damageScale: p.ArmyDamageScaleDict[armyType],
                    defenseScale: p.ArmyDefenseScaleDict[armyType],
                    healthScale: p.ArmyHealthScaleDict[armyType],
                    speedScale: p.ArmySpeedScaleDict[armyType]
                );
                ArmyPropertyScalerDict[armyType] = propertyScaler;
            }
        }
    }
}