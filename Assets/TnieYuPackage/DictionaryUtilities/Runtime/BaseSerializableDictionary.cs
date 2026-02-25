using System;
using System.Collections.Generic;
using UnityEngine;

namespace DictionaryUtilities.Runtime
{
    [Serializable]
    public abstract class BaseSerializableDictionary<TKeyPair, TKey, TValue> : ISerializationCallbackReceiver
        where TKeyPair : struct, ISerializableKeyPair<TKey, TValue>
    {
        /// <summary>
        /// Editor Dictionary [List]. Data for RuntimeDictionary.
        /// </summary>
        [SerializeField] private List<TKeyPair> data = new();

        private Dictionary<TKey, TValue> dictionary;

        /// <summary>
        /// Runtime Dictionary. It not affect when Editor.
        /// </summary>
        public Dictionary<TKey, TValue> Dictionary
        {
            get
            {
                if (dictionary == null)
                {
                    RebuildDictionary();
                }

                return dictionary;
            }
        }

        public TValue this[TKey key]
        {
            get => Dictionary[key];
            set => Dictionary[key] = value;
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            dictionary = null;
        }

        public void RebuildDictionary()
        {
            data ??= new();

            dictionary = new Dictionary<TKey, TValue>();

            foreach (var kvp in data)
            {
                dictionary[kvp.Key] = kvp.Value;
            }
        }

        public void ReverseData()
        {
            if (dictionary == null) return;

            data.Clear();
            foreach (var kvp in dictionary)
            {
                data.Add(
                    ISerializableKeyPair<TKey, TValue>
                        .CreateKeyPair<TKeyPair>(kvp.Key, kvp.Value)
                );
            }
        }
    }
}