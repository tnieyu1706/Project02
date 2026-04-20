using System.Collections.Generic;
using Game.Global;
using TnieYuPackage.DesignPatterns;

namespace Game.BaseGameplay
{
    public readonly struct ArmyPropertyScaler
    {
        public float DamageScale { get; }
        public float DefenseScale { get; }
        public float HealthScale { get; }
        public float SpeedScale { get; }

        public ArmyPropertyScaler(
            float damageScale = 1f,
            float defenseScale = 1f,
            float healthScale = 1f,
            float speedScale = 1f
        )
        {
            DamageScale = damageScale;
            DefenseScale = defenseScale;
            HealthScale = healthScale;
            SpeedScale = speedScale;
        }
    }

    public abstract class BaseGameplayCalculator : SingletonBehavior<BaseGameplayCalculator>
    {
        public abstract float GlobalDamageScale { get; }
        public abstract float GlobalDefenseScale { get; }
        public abstract float ReceivedMoneyScale { get; }

        protected abstract Dictionary<TowerType, float> TowerDamageScales { get; }

        protected abstract Dictionary<ArmyType, ArmyPropertyScaler> ArmyPropertyScalerDict { get; }

        #region Tower Calculators
        public float CalculateTowerDamage(TowerType towerType, float baseDamage)
        {
            var towerDamageScale = TowerDamageScales.ContainsKey(towerType) ? TowerDamageScales[towerType] : 1f;
            return baseDamage * GlobalDamageScale * towerDamageScale;
        }
        
        #endregion
        
        #region Army Calculators

        public float CalculateArmyDamage(ArmyType armyType, float baseDamage)
        {
            var armyDamageScale = ArmyPropertyScalerDict.ContainsKey(armyType)
                ? ArmyPropertyScalerDict[armyType].DamageScale
                : 1f;
            return baseDamage * GlobalDamageScale * armyDamageScale;
        }

        public float CalculateArmyDefense(ArmyType armyType, float baseDefense)
        {
            var armyDefenseScale = ArmyPropertyScalerDict.ContainsKey(armyType)
                ? ArmyPropertyScalerDict[armyType].DefenseScale
                : 1f;
            return baseDefense * GlobalDefenseScale * armyDefenseScale;
        }

        public float CalculateArmyHealth(ArmyType armyType, float baseHealth)
        {
            var armyHealthScale = ArmyPropertyScalerDict.ContainsKey(armyType)
                ? ArmyPropertyScalerDict[armyType].HealthScale
                : 1f;
            return baseHealth * armyHealthScale;
        }

        public float CalculateArmySpeed(ArmyType armyType, float baseSpeed)
        {
            var armySpeedScale = ArmyPropertyScalerDict.ContainsKey(armyType)
                ? ArmyPropertyScalerDict[armyType].SpeedScale
                : 1f;
            return baseSpeed * armySpeedScale;
        }
        
        #endregion

        public float CalculateTotalDamageCause(float damage, float defense)
        {
            return damage - defense;
        }
    }
}