using System;
using Game.BaseGameplay;

namespace Game.Global
{
    /// <summary>
    /// Unlocks tower types or higher tower levels
    /// </summary>
    [Serializable]
    public class TowerUnlockInfluencer : ISkillInfluencer
    {
        public enum UnlockTarget
        {
            TowerType,
            TowerLevel
        }

        public UnlockTarget target;

        public TowerType towerType;
        public TowerLevel towerLevel;

        public void ApplyAffect()
        {
            var p = GamePropertiesRuntime.Instance;
            if (target == UnlockTarget.TowerType)
            {
                if (p.UnlockTowerTypeDict.ContainsKey(towerType))
                    p.UnlockTowerTypeDict[towerType] = true;
            }
            else
            {
                if (p.UnlockTowerLevelDict.ContainsKey(towerLevel))
                    p.UnlockTowerLevelDict[towerLevel] = true;
            }
        }
    }
}