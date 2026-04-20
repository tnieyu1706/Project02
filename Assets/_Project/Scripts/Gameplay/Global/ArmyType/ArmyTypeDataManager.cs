using System.Collections.Generic;
using TnieYuPackage.DesignPatterns;
using TnieYuPackage.DictionaryUtilities;
using UnityEngine;

namespace Game.Global
{
    [CreateAssetMenu(fileName = "ArmyTypeDataManager", menuName = "Game/Global/ArmyTypeDataManager")]
    public class ArmyTypeDataManager : SingletonScriptable<ArmyTypeDataManager>
    {
        [SerializeField] private SerializableDictionary<ArmyType, ArmyTypeData> refs;
        public Dictionary<ArmyType, ArmyTypeData> Refs => refs.Dictionary;
    }
}