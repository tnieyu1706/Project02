using System.Collections.Generic;
using EditorAttributes;
using TnieYuPackage.DesignPatterns;
using TnieYuPackage.DictionaryUtilities;
using UnityEditor;
using UnityEngine;
using ZLinq;

namespace Game.Td
{
    [CreateAssetMenu(fileName = "EnemyPresetManager", menuName = "Game/TD/Entity/Enemy/Manager")]
    public class EnemyPresetManager : SingletonScriptable<EnemyPresetManager>
    {
        public SerializableDictionary<string, EnemyPresetSo> data = new();

#if UNITY_EDITOR

        [Button]
        private void LoadData()
        {
            List<EnemyPresetSo> presetData = data.Dictionary.Values.AsValueEnumerable().ToList();
            
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(EnemyPresetSo)}");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                EnemyPresetSo preset = AssetDatabase.LoadAssetAtPath<EnemyPresetSo>(path);

                if (preset != null && !presetData.Contains(preset))
                {
                    data[preset.enemyId] = preset;
                }
            }

            data.RewriteData();
        }

        [Button]
        private void ClearData()
        {
            data.Dictionary.Clear();
            data.RewriteData();
        }

#endif
    }
}