using System;
using TnieYuPackage.DictionaryUtilities;
using TnieYuPackage.SOAP;
using TnieYuPackage.Utils;
using UnityEngine;

namespace Game.Global
{
    [Serializable]
    public class ArmyStorageSoapData : ISoapData<SerializableDictionary<ArmyType, ObservableValue<int>>>
    {
        [SerializeField] private SerializableDictionary<ArmyType, ObservableValue<int>> value;

        public SerializableDictionary<ArmyType, ObservableValue<int>> Value
        {
            get => value;
            set => this.value = value;
        }

        public Action<SerializableDictionary<ArmyType, ObservableValue<int>>> OnValueChange { get; set; }
    }

    [CreateAssetMenu(fileName = "ArmyStorage", menuName = "Game/Soap/ArmyStorage")]
    public class ArmyStorageSoapDataSo :
        SoapDataSo<ArmyStorageSoapData, SerializableDictionary<ArmyType, ObservableValue<int>>>
    {
    }
}