using System.Collections.Generic;
using EditorAttributes;
using UnityEditor;
using UnityEngine;
using Void = EditorAttributes.Void;

namespace TnieYuPackage.DesignPatterns
{
    public abstract class BaseParentAsset<TSub> : ScriptableObject
        where TSub : ScriptableObject
    {
        [SerializeField]
        [PropertyOrder(5)]
        [FoldoutGroup(
            "Sub Assets",
            nameof(subAssetName),
            nameof(generateSubAssetButton),
            nameof(deletingSubAsset),
            nameof(deleteSubAssetButton)
        )]
        private Void baseParentAssetFoldout;

        protected abstract List<TSub> SubAssets { get; }

        [SerializeField] [HideProperty] protected string subAssetName;

        protected virtual TSub CreateSubAsset()
        {
            return ScriptableObject.CreateInstance<TSub>();
        }

        [SerializeField] [HideProperty, ButtonField(nameof(GenerateSubAsset), "Generate SubAsset")]
        protected Void generateSubAssetButton;

        private void GenerateSubAsset()
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(subAssetName))
            {
                Debug.Log("SubObjectName is null or empty");
                return;
            }

            var subAsset = CreateSubAsset();
            subAsset.name = subAssetName;
            HandleSubAssetWhenGenerating(subAsset);

            //add sub-asset
            AssetDatabase.AddObjectToAsset(subAsset, this);

            //add to list
            SubAssets.Add(subAsset);

            EditorUtility.SetDirty(this);
            EditorUtility.SetDirty(subAsset);
            AssetDatabase.SaveAssets();

            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(this));
#endif
        }

        protected virtual void HandleSubAssetWhenGenerating(TSub subAsset)
        {
        }

        [SerializeField] [HideProperty] protected TSub deletingSubAsset;

        [SerializeField] [HideProperty, ButtonField(nameof(DeleteSubAsset), "Delete SubAsset")]
        protected Void deleteSubAssetButton;

        private void DeleteSubAsset()
        {
#if UNITY_EDITOR
            if (deletingSubAsset == null)
            {
                Debug.Log("Current Delete_Sub_Asset is null");
                return;
            }

            if (SubAssets.Contains(deletingSubAsset))
                SubAssets.Remove(deletingSubAsset);
            else
            {
                Debug.LogWarning($"Current Delete_Sub_Asset is not in children of this parent");
                return;
            }

            HandleSubAssetBeforeDelete(deletingSubAsset);
            DestroyImmediate(deletingSubAsset, true);

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(this));
#endif
        }

        protected virtual void HandleSubAssetBeforeDelete(TSub subAsset)
        {
        }
    }
}