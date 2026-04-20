using System;
using System.Collections.Generic;
using Game.BaseGameplay;
using TnieYuPackage.CustomAttributes;

namespace Game.Global
{
    public enum ArmyType
    {
        Melee,
        Range,
        Strong,
        Quick
    }

    [Serializable]
    public class ArmyTypeData
    {
        [AddressableKey(type: typeof(EntityPresetSo))]
        public string entityId;

        public EntityPresetSo entityPreset;
        public float spawnInterval;

        // public float timeWeight;
        public float pathScore;
    }

    public static class ArmyTypeExtensions
    {
        private static Dictionary<ArmyType, ArmyTypeData> ArmyRefs =>
            ArmyTypeDataManager.Instance.Refs;

        public static ArmyTypeData GetArmyTypeData(this ArmyType armyType)
        {
            return armyType switch
            {
                ArmyType.Melee => ArmyRefs[ArmyType.Melee],
                ArmyType.Range => ArmyRefs[ArmyType.Range],
                ArmyType.Strong => ArmyRefs[ArmyType.Strong],
                ArmyType.Quick => ArmyRefs[ArmyType.Quick],
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}