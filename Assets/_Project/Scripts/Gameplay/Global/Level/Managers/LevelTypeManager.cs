using System;
using System.Collections.Generic;
using Game.BaseGameplay;
using TnieYuPackage.DesignPatterns;
using TnieYuPackage.DictionaryUtilities;
using UnityEngine;
using EventType = Game.BaseGameplay.EventType;

namespace Game.Global
{
    [Serializable]
    public class LevelTypeData
    {
        public TdGameplayLevel tdLevel;
        public WaGameplayLevel waLevel;

        public BaseGameplayLevel GetGameplayLevelBy(EventType eventType)
        {
            return eventType switch
            {
                EventType.Attack => waLevel,
                EventType.Defense => tdLevel,
                _ => throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null)
            };
        }
    }

    [CreateAssetMenu(fileName = "LevelTypeManager", menuName = "Game/Global/LevelTypeManager")]
    public class LevelTypeManager : SingletonScriptable<LevelTypeManager>
    {
        [SerializeField] private SerializableDictionary<LevelType, LevelTypeData> dict;

        public static Dictionary<LevelType, LevelTypeData> Refs => Instance.dict.Dictionary;
    }
}