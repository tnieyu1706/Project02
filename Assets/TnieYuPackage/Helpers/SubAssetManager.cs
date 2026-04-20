using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TnieYuPackage.Helpers
{
    /// <summary>
    /// Provides drag-and-drop functionality within the Unity Project window 
    /// to manage sub-assets. Includes options to automatically remap missing references 
    /// when packing or unpacking assets.
    /// </summary>
    [InitializeOnLoad]
    public static class SubAssetManager
    {
        static SubAssetManager()
        {
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
        }

        private static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
        {
            Event evt = Event.current;

            if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform) return;
            if (!selectionRect.Contains(evt.mousePosition)) return;

            string targetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(targetPath)) return;

            Object targetMainAsset = AssetDatabase.LoadMainAssetAtPath(targetPath);
            bool isTargetFolder = AssetDatabase.IsValidFolder(targetPath);

            Object[] draggedObjects = DragAndDrop.objectReferences;
            if (draggedObjects == null || draggedObjects.Length == 0) return;

            bool canPackIn = !isTargetFolder && targetMainAsset != null && CanPackInto(draggedObjects, targetMainAsset);
            bool canPackOut = isTargetFolder && CanPackOut(draggedObjects);

            if (!canPackIn && !canPackOut) return;

            if (evt.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                evt.Use();
            }
            else if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();

                if (canPackIn)
                {
                    string msg =
                        $"Do you want to pack {draggedObjects.Length} asset(s) into '{targetMainAsset.name}'?\n\n" +
                        "You can choose to automatically scan the project and fix missing references.";

                    // DisplayDialogComplex returns: 0 = ok (btn 1), 1 = cancel (btn 2), 2 = alt (btn 3)
                    int choice = EditorUtility.DisplayDialogComplex("Pack Asset(s)", msg, "Pack (Fix Refs)", "Cancel",
                        "Pack (No Refs)");

                    if (choice == 0) PackAssets(draggedObjects, targetMainAsset, true);
                    else if (choice == 2) PackAssets(draggedObjects, targetMainAsset, false);
                }
                else if (canPackOut)
                {
                    string msg =
                        $"Do you want to unpack {draggedObjects.Length} sub-asset(s) into the folder '{Path.GetFileName(targetPath)}'?\n\n" +
                        "You can choose to automatically scan the project and fix missing references.";

                    int choice = EditorUtility.DisplayDialogComplex("Unpack Asset(s)", msg, "Unpack (Fix Refs)",
                        "Cancel", "Unpack (No Refs)");

                    if (choice == 0) UnpackAssets(draggedObjects, targetPath, true);
                    else if (choice == 2) UnpackAssets(draggedObjects, targetPath, false);
                }

                evt.Use();
            }
        }

        #region Validation Logic

        private static bool CanPackInto(Object[] draggedObjects, Object targetAsset)
        {
            foreach (var obj in draggedObjects)
            {
                if (obj is GameObject || obj is SceneAsset ||
                    AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(obj)))
                    return false;

                if (AssetDatabase.GetAssetPath(obj) == AssetDatabase.GetAssetPath(targetAsset))
                    return false;
            }

            return true;
        }

        private static bool CanPackOut(Object[] draggedObjects)
        {
            foreach (var obj in draggedObjects)
            {
                if (AssetDatabase.IsMainAsset(obj) || AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(obj)))
                    return false;
            }

            return true;
        }

        #endregion

        #region Drag & Drop Processing (Pack / Unpack)

        /// <summary>
        /// Adds the source assets as sub-assets inside the target main asset.
        /// Can optionally replace missing references across the project.
        /// </summary>
        private static void PackAssets(Object[] sources, Object targetMain, bool fixReferences)
        {
            Dictionary<Object, Object> referenceMap = new Dictionary<Object, Object>();
            List<string> mainAssetsToDelete = new List<string>();
            List<Object> subAssetsToDestroy = new List<Object>();

            foreach (var source in sources)
            {
                string oldPath = AssetDatabase.GetAssetPath(source);

                Object clone = Object.Instantiate(source);
                clone.name = source.name;

                AssetDatabase.AddObjectToAsset(clone, targetMain);
                referenceMap.Add(source, clone); // Record the mapping for reference fixing

                if (AssetDatabase.IsMainAsset(source))
                {
                    mainAssetsToDelete.Add(oldPath);
                }
                else
                {
                    subAssetsToDestroy.Add(source);
                }
            }

            // Save assets so the clones are fully registered before we remap
            AssetDatabase.SaveAssets();

            if (fixReferences)
            {
                ReplaceReferencesInProject(referenceMap);
            }

            // Now safely delete the old assets
            foreach (var path in mainAssetsToDelete)
            {
                AssetDatabase.DeleteAsset(path);
            }

            foreach (var obj in subAssetsToDestroy)
            {
                Object.DestroyImmediate(obj, true);
            }

            SaveAndRefresh(targetMain);
            Debug.Log($"[SubAssetManager] Successfully packed {sources.Length} asset(s)!");
        }

        /// <summary>
        /// Extracts sub-assets into a specified folder using cloning.
        /// Can optionally replace missing references across the project.
        /// </summary>
        private static void UnpackAssets(Object[] subAssets, string targetFolderPath, bool fixReferences)
        {
            Dictionary<Object, Object> referenceMap = new Dictionary<Object, Object>();
            List<Object> subAssetsToDestroy = new List<Object>();

            foreach (var subAsset in subAssets)
            {
                string extension = subAsset is AnimationClip ? ".anim" : ".asset";
                string newPath = Path.Combine(targetFolderPath, subAsset.name + extension).Replace("\\", "/");
                newPath = AssetDatabase.GenerateUniqueAssetPath(newPath);

                Object clone = Object.Instantiate(subAsset);
                clone.name = subAsset.name;

                AssetDatabase.CreateAsset(clone, newPath);

                referenceMap.Add(subAsset, clone); // Record mapping
                subAssetsToDestroy.Add(subAsset); // Delay destruction until after reference fix
            }

            AssetDatabase.SaveAssets();

            if (fixReferences)
            {
                ReplaceReferencesInProject(referenceMap);
            }

            // Clean up original sub-assets
            foreach (var obj in subAssetsToDestroy)
            {
                AssetDatabase.RemoveObjectFromAsset(obj);
                Object.DestroyImmediate(obj, true);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[SubAssetManager] Successfully unpacked {subAssets.Length} asset(s) into {targetFolderPath}");
        }

        #endregion

        #region Reference Fixing Logic

        /// <summary>
        /// Scans the project and replaces any serialized property pointing to an old asset with the new asset.
        /// </summary>
        private static void ReplaceReferencesInProject(Dictionary<Object, Object> referenceMap)
        {
            if (referenceMap == null || referenceMap.Count == 0) return;

            string[] allPaths = AssetDatabase.GetAllAssetPaths();
            float total = allPaths.Length;

            for (int i = 0; i < allPaths.Length; i++)
            {
                string path = allPaths[i];

                // Update Progress Bar
                if (i % 50 == 0)
                {
                    if (EditorUtility.DisplayCancelableProgressBar("Updating References", $"Scanning project: {path}",
                            i / total))
                        break;
                }

                // Filter out non-project files and files that cannot hold Unity references to speed up
                if (!path.StartsWith("Assets/")) continue;
                string ext = Path.GetExtension(path).ToLower();
                if (ext == ".cs" || ext == ".png" || ext == ".jpg" || ext == ".wav" ||
                    ext == ".mp3" || ext == ".fbx" || ext == ".obj" || ext == ".unity" || ext == ".txt" ||
                    ext == ".json")
                {
                    // Note: .unity (Scenes) are skipped because LoadAllAssetsAtPath doesn't work well on unopened scenes.
                    continue;
                }

                Object[] assetsInPath = AssetDatabase.LoadAllAssetsAtPath(path);
                bool modified = false;

                foreach (var asset in assetsInPath)
                {
                    if (asset == null) continue;

                    SerializedObject so = new SerializedObject(asset);
                    SerializedProperty prop = so.GetIterator();
                    bool enterChildren = true;

                    // Iterate through every single property inside the asset
                    while (prop.Next(enterChildren))
                    {
                        enterChildren = true;
                        if (prop.propertyType == SerializedPropertyType.ObjectReference)
                        {
                            Object refObj = prop.objectReferenceValue;
                            if (refObj != null && referenceMap.TryGetValue(refObj, out Object newObj))
                            {
                                // Overwrite the old missing reference with the new clone
                                prop.objectReferenceValue = newObj;
                                modified = true;
                            }
                        }
                    }

                    if (modified)
                    {
                        so.ApplyModifiedPropertiesWithoutUndo();
                        EditorUtility.SetDirty(asset);
                    }
                }
            }

            EditorUtility.ClearProgressBar();
        }

        #endregion

        #region Context Menu Actions

        [MenuItem("Assets/Sub-Asset Manager/Unpack All Sub-assets", true)]
        [MenuItem("Assets/Sub-Asset Manager/Delete All Sub-assets", true)]
        private static bool ValidateContextMenu()
        {
            return Selection.activeObject != null && AssetDatabase.IsMainAsset(Selection.activeObject);
        }

        [MenuItem("Assets/Sub-Asset Manager/Unpack All Sub-assets", false, 20)]
        private static void ContextMenu_UnpackAll()
        {
            Object mainAsset = Selection.activeObject;
            string assetPath = AssetDatabase.GetAssetPath(mainAsset);
            string folderPath = Path.GetDirectoryName(assetPath);

            Object[] allObjects = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            List<Object> subAssetsToUnpack = new List<Object>();

            foreach (var obj in allObjects)
            {
                if (obj == null || AssetDatabase.IsMainAsset(obj)) continue;
                subAssetsToUnpack.Add(obj);
            }

            if (subAssetsToUnpack.Count == 0)
            {
                Debug.Log("[SubAssetManager] No sub-assets found to unpack.");
                return;
            }

            string msg = $"Found {subAssetsToUnpack.Count} sub-asset(s) inside '{mainAsset.name}'.\n" +
                         $"Do you want to unpack them into the folder '{Path.GetFileName(folderPath)}'?\n\n" +
                         "You can choose to automatically scan the project and fix missing references.";

            int choice = EditorUtility.DisplayDialogComplex("Unpack All Sub-assets", msg, "Unpack (Fix Refs)", "Cancel",
                "Unpack (No Refs)");

            if (choice == 1) return; // User clicked Cancel

            // Run unpack logic with the collected list of sub-assets
            UnpackAssets(subAssetsToUnpack.ToArray(), folderPath, choice == 0);
        }

        [MenuItem("Assets/Sub-Asset Manager/Delete All Sub-assets", false, 21)]
        private static void ContextMenu_DeleteAll()
        {
            Object mainAsset = Selection.activeObject;
            string assetPath = AssetDatabase.GetAssetPath(mainAsset);
            Object[] allObjects = AssetDatabase.LoadAllAssetsAtPath(assetPath);

            if (EditorUtility.DisplayDialog("Warning",
                    $"Are you sure you want to DELETE ALL sub-assets inside '{mainAsset.name}'?",
                    "Delete", "Cancel"))
            {
                int count = 0;
                foreach (var obj in allObjects)
                {
                    if (obj == null || AssetDatabase.IsMainAsset(obj)) continue;

                    Object.DestroyImmediate(obj, true);
                    count++;
                }

                if (count > 0)
                {
                    SaveAndRefresh(mainAsset);
                    Debug.Log($"[SubAssetManager] Deleted {count} sub-assets from {mainAsset.name}");
                }
            }
        }

        #endregion

        #region Helper Methods

        private static void SaveAndRefresh(Object asset)
        {
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(asset));
            AssetDatabase.Refresh();
        }

        #endregion
    }
}