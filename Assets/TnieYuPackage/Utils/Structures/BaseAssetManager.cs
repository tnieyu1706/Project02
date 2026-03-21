using System.Collections.Generic;
using EditorAttributes;
using TnieYuPackage.DesignPatterns;
using TnieYuPackage.DictionaryUtilities;
using UnityEditor;
using UnityEngine;
using ZLinq;

namespace TnieYuPackage.Utils.Structures
{
    public abstract class BaseAssetManager<TIdentify, TAsset, TSingleton> : SingletonScriptable<TSingleton>
        where TAsset : ScriptableObject
        where TSingleton : BaseAssetManager<TIdentify, TAsset, TSingleton>
    {
        public SerializableDictionary<TIdentify, TAsset> data = new();

#if UNITY_EDITOR

        [Button]
        private void LoadData()
        {
            List<TAsset> existData = data.Dictionary.Values.AsValueEnumerable().ToList();

            string[] guids = AssetDatabase.FindAssets($"t:{typeof(TAsset).Name}");
            int loadedCount = 0;
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TAsset asset = AssetDatabase.LoadAssetAtPath<TAsset>(path);

                if (asset != null && !existData.Contains(asset))
                {
                    data[GetAssetIdentify(asset)] = asset;
                    loadedCount++;
                }
            }

            data.RewriteData();

            Debug.Log($"Loaded {loadedCount}/{guids.Length} assets");
        }

        protected abstract TIdentify GetAssetIdentify(TAsset asset);

        [Button]
        private void ClearData()
        {
            data.Dictionary.Clear();
            data.RewriteData();

            Debug.Log($"Cleared assets");
        }

#endif
    }
}