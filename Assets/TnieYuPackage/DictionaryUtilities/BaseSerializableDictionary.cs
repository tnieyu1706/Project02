using System;
using System.Collections.Generic;
using UnityEngine;

namespace TnieYuPackage.DictionaryUtilities
{
    /// <summary>
    /// Dictionary wrapper for Unity serialization. Use List to store data, and build & change Dictionary on runtime.
    /// </summary>
    /// <typeparam name="TKeyPair"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    [Serializable]
    public abstract class BaseSerializableDictionary<TKeyPair, TKey, TValue>
        where TKeyPair : struct, ISerializableKeyPair<TKey, TValue>
    {
        /// <summary>
        /// Editor data. Only change on editor | Rebuild
        /// </summary>
        [SerializeField] private List<TKeyPair> data = new();

        private Dictionary<TKey, TValue> dictionary;

        /// <summary>
        /// Runtime data. Core Dictionary for runtime
        /// </summary>
        public Dictionary<TKey, TValue> Dictionary
        {
            get
            {
                if (dictionary == null)
                {
                    BuildDictionary();
                }

                return dictionary;
            }
        }

        public TValue this[TKey key]
        {
            get => Dictionary[key];
            set => Dictionary[key] = value;
        }

        /// <summary>
        /// Build data.List => data.Dictionary
        /// </summary>
        public void BuildDictionary()
        {
            data ??= new();

            dictionary = new Dictionary<TKey, TValue>();

            foreach (var kvp in data)
            {
                dictionary[kvp.Key] = kvp.Value;
            }
        }

        /// <summary>
        /// Build data.Dictionary => data.List
        /// </summary>
        public void RewriteData()
        {
            data.Clear();
            if (dictionary == null || dictionary.Count == 0) return;

            foreach (var kvp in dictionary)
            {
                data.Add(new TKeyPair() { Key = kvp.Key, Value = kvp.Value });
            }
        }
        
    }
}