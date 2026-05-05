using System;
using System.Collections.Generic;
using Game.BaseGameplay;
using TnieYuPackage.CustomAttributes;
using TnieYuPackage.DesignPatterns;
using TnieYuPackage.DictionaryUtilities;
using UnityEngine;

namespace Game.Global
{
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

    [CreateAssetMenu(fileName = "ArmyTypeDataManager", menuName = "Game/Global/ArmyTypeDataManager")]
    public class ArmyTypeDataManager : SingletonScriptable<ArmyTypeDataManager>
    {
        [SerializeField] private SerializableDictionary<ArmyType, ArmyTypeData> refs;
        public Dictionary<ArmyType, ArmyTypeData> Refs => refs.Dictionary;
    }
}