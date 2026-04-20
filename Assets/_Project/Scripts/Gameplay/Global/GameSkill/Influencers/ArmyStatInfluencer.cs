using System;

namespace Game.Global
{
    /// <summary>
    /// Influences Army stats (Attacking units)
    /// </summary>
    [Serializable]
    public class ArmyStatInfluencer : ISkillInfluencer
    {
        public enum ArmyStat
        {
            Health,
            Speed,
            Defense,
            Damage
        }

        public ArmyType armyType;
        public ArmyStat stat;
        public float addValue;

        public void ApplyAffect()
        {
            var p = GamePropertiesRuntime.Instance;
            switch (stat)
            {
                case ArmyStat.Health:
                {
                    if (p.ArmyHealthScaleDict.ContainsKey(armyType))
                        p.ArmyHealthScaleDict[armyType] += addValue;
                    break;
                }
                case ArmyStat.Speed:
                {
                    if (p.ArmySpeedScaleDict.ContainsKey(armyType))
                        p.ArmySpeedScaleDict[armyType] += addValue;
                    break;
                }
                case ArmyStat.Defense:
                {
                    if (p.ArmyDefenseScaleDict.ContainsKey(armyType))
                        p.ArmyDefenseScaleDict[armyType] += addValue;
                    break;
                }
                case ArmyStat.Damage:
                {
                    if (p.ArmyDamageScaleDict.ContainsKey(armyType))
                        p.ArmyDamageScaleDict[armyType] += addValue;
                    break;
                }
            }
        }
    }
}