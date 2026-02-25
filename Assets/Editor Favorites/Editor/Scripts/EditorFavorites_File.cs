#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EditorFavorites
{
    internal static class EditorFavorites_File
    {
        private static string GetOrCreateDirectory(string subFolderName)
        {
            string rootPath = GetEditorPocketRootPath();
            string fullPath = Path.Combine(rootPath, EditorFavorites_Constants.EditorFolderName, subFolderName);

            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
                AssetDatabase.Refresh();
            }

            return fullPath;
        }

        private static string GetEditorPocketRootPath()
        {
            string[] guids = AssetDatabase.FindAssets(EditorFavorites_Constants.AssetName);
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (Directory.Exists(path) && path.EndsWith(EditorFavorites_Constants.AssetName))
                {
                    return path;
                }
            }

            Debug.LogWarning($"Editor Favorites root path was not found. Defaulting to {Path.Combine(Application.dataPath, EditorFavorites_Constants.AssetName)}.");
            return Path.Combine(Application.dataPath, EditorFavorites_Constants.AssetName);
        }

        public static string GetSavedDataFilePath(string fileName)
        {
            string fullPath = GetOrCreateDirectory(EditorFavorites_Constants.SavedDataFolderName);
            return Path.Combine(fullPath, string.IsNullOrWhiteSpace(fileName) ? EditorFavorites_Constants.DefaultDataFileName : fileName);
        }
    }
}
#endif