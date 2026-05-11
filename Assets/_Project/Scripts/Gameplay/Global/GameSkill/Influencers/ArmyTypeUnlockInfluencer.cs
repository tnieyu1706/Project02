using System;

namespace Game.Global
{
    /// <summary>
    /// Unlocks specific army types in BuildingGameplay
    /// </summary>
    [Serializable]
    public class ArmyTypeUnlockInfluencer : ISkillInfluencer
    {
        public ArmyType armyType;

        public void ApplyAffect()
        {
            // Add new entry if it doesn't exist in the dictionary
            GamePropertiesRuntime.Instance.UnlockArmyTypeDict[armyType] = true;
        }
    }
}