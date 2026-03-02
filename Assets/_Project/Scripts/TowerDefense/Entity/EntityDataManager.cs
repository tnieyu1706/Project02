using EditorAttributes;
using TnieYuPackage.DesignPatterns.Patterns.Singleton;
using TnieYuPackage.DictionaryUtilities;
using UnityEditor;
using UnityEngine;

namespace _Project.Scripts.TowerDefense.Entity
{
    [CreateAssetMenu(fileName = "EntityDataManager", menuName = "Game/TD/Entity/Manager")]
    public class EntityDataManager : SingletonScriptable<EntityDataManager>
    {
        public SerializableDictionary<string, EntityPresetSo> data = new();

#if UNITY_EDITOR

        [Button]
        private void LoadData()
        {
            string[] guids = AssetDatabase.FindAssets("t:EntityPresetSo");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                EntityPresetSo preset = AssetDatabase.LoadAssetAtPath<EntityPresetSo>(path);

                if (preset != null)
                {
                    data[preset.entityId] = preset;
                }
            }

            data.RebuildData();
        }

        [Button]
        private void ClearData()
        {
            data.Dictionary.Clear();
            data.RebuildData();
        }

#endif
    }
}