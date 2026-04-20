using System;

namespace Game.Global
{
    /// <summary>
    /// Influences individual float properties (Damage, Defense, Refund, etc.)
    /// </summary>
    [Serializable]
    public class GlobalStatInfluencer : ISkillInfluencer
    {
        public enum GlobalStatType
        {
            DamageScale,
            DefenseScale,
            ReceiveMoneyScale,
            RefundScale,
            MaxEntityPerWave,
            GlobalResourceScale
        }

        public GlobalStatType statType;
        public float value;

        public void ApplyAffect()
        {
            var p = GamePropertiesRuntime.Instance;
            switch (statType)
            {
                case GlobalStatType.DamageScale: p.DamageScale += value; break;
                case GlobalStatType.DefenseScale: p.DefenseScale += value; break;
                case GlobalStatType.ReceiveMoneyScale: p.ReceiveMoneyScale += value; break;
                case GlobalStatType.RefundScale: p.RefundScale += value; break;
                case GlobalStatType.MaxEntityPerWave: p.MaxEntityPerWave += (int)value; break;
                case GlobalStatType.GlobalResourceScale: p.GeneralResourceReceivedScale += value; break;
            }
        }
    }
}