using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TnieYuPackage.GlobalExtensions
{
    public static class DictionaryExtensions
    {
        public static void Resize<T, TValue>(this IDictionary<T, TValue> dict, T defaultKey, int newSize)
        {
            if (newSize < 0)
            {
                Debug.LogError($"New size cannot be less than 0");
                return;
            }

            int resizedCount = dict.Count - newSize;
            while (resizedCount != 0)
            {
                if (resizedCount > 0)
                {
                    dict.Remove(dict.Keys.Last());
                    resizedCount--;
                }
                else
                {
                    dict.Add(defaultKey, default(TValue));
                    resizedCount++;
                }
            }
        }

        public static void RemoveIf<TKey, TValue>(this IDictionary<TKey, TValue> dict,
            Predicate<KeyValuePair<TKey, TValue>> predicate)
        {
            List<TKey> removeKeys = new();

            foreach (var kvp in dict)
            {
                if (predicate.Invoke(kvp))
                {
                    removeKeys.Add(kvp.Key);
                }
            }

            foreach (var key in removeKeys)
            {
                dict.Remove(key);
            }
        }

        public static void RemoveIfAction<TKey, TValue>(this IDictionary<TKey, TValue> dict,
            Predicate<KeyValuePair<TKey, TValue>> predicate,
            Action<IDictionary<TKey, TValue>, TKey> onRemove)
        {
            List<TKey> removeKeys = new();

            foreach (var kvp in dict)
            {
                if (predicate.Invoke(kvp))
                {
                    removeKeys.Add(kvp.Key);
                }
            }

            foreach (var key in removeKeys)
            {
                onRemove?.Invoke(dict, key);
            }
        }
    }
}