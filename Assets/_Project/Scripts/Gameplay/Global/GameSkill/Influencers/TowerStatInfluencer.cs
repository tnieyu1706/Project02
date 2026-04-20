using System;
using Game.BaseGameplay;

namespace Game.Global
{
    /// <summary>
    /// Increases power for specific tower types (Archer, Magic, Cannon, etc.)
    /// </summary>
    [Serializable]
    public class TowerStatInfluencer : ISkillInfluencer
    {
        public TowerType towerType;
        public float damageScaleAdd;

        public void ApplyAffect()
        {
            if (GamePropertiesRuntime.Instance.TowerTypeDamageScaleDict.ContainsKey(towerType))
            {
                GamePropertiesRuntime.Instance.TowerTypeDamageScaleDict[towerType] += damageScaleAdd;
            }
        }
    }
}