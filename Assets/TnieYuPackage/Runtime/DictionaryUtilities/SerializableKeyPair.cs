using System;
using TnieYuPackage.CustomAttributes;
using UnityEngine;

namespace TnieYuPackage.DictionaryUtilities
{
    public interface ISerializableKeyPair<TKey, TValue>
    {
        public TKey Key { get; set; }
        public TValue Value { get; set; }
    }

    [Serializable]
    public struct SerializableKeyPair<TKey, TValue> : ISerializableKeyPair<TKey, TValue>
    {
        [SerializeField] private TKey key;

        public TKey Key
        {
            get => key;
            set => key = value;
        }

        public TValue value;

        public TValue Value
        {
            get => value;
            set => this.value = value;
        }

        public SerializableKeyPair(TKey key, TValue value)
        {
            this.key = key;
            this.value = value;
        }
    }

    [Serializable]
    public struct SerializableKeyPairAbstract<TKey, TValue> : ISerializableKeyPair<TKey, TValue>
    {
        [SerializeField] private TKey key;
        [SerializeReference] [AbstractSupport] public TValue value;

        public TKey Key
        {
            get => key;
            set => key = value;
        }

        public TValue Value
        {
            get => value;
            set => this.value = value;
        }

        public SerializableKeyPairAbstract(TKey key, TValue value)
        {
            this.key = key;
            this.value = value;
        }
    }
}