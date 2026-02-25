#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace EditorFavorites
{
    [Serializable]
    internal sealed class EditorFavorites_Data
    {
        [SerializeField] private EditorFavorites_SaveData state;
        [SerializeField] private string dataPath;

        public IReadOnlyList<FavoritesTab> Tabs => state.Tabs;
        public int SelectedTabIndex
        {
            get => state.SelectedTabIndex;
            set => state.SelectedTabIndex = value;
        }

        public float ItemScale01
        {
            get => state.ItemScale01;
            set => state.ItemScale01 = Mathf.Clamp01(value);
        }

        public static EditorFavorites_Data LoadOrCreate()
        {
            EditorFavorites_Data instance = new();
            instance.dataPath = EditorFavorites_File.GetSavedDataFilePath(null);

            if (File.Exists(instance.dataPath))
            {
                string json = File.ReadAllText(instance.dataPath, Encoding.UTF8);
                instance.state = JsonUtility.FromJson<EditorFavorites_SaveData>(json) ?? new EditorFavorites_SaveData();
            }
            else
            {
                instance.state = new EditorFavorites_SaveData();
                instance.state.AddTab("Favorites");
                instance.state.SelectedTabIndex = 0;
                instance.state.ItemScale01 = 1f;
                instance.Save();
            }

            return instance;
        }

        public void AddTab(string name)
        {
            state.AddTab(string.IsNullOrWhiteSpace(name) ? "Favorites" : name.Trim());
            state.SelectedTabIndex = state.Tabs.Count - 1;
            Save();
        }

        public void RemoveSelectedTab()
        {
            if (state.Tabs.Count == 0) return;

            state.RemoveAt(state.SelectedTabIndex);
            state.SelectedTabIndex = Mathf.Clamp(state.SelectedTabIndex, 0, Mathf.Max(0, state.Tabs.Count - 1));
            Save();
        }

        public FavoritesTab GetSelected()
        {
            if (state.Tabs.Count == 0) AddTab("Favorites");
            return state.Tabs[state.SelectedTabIndex];
        }

        public void RefreshSelected()
        {
            if (state.Tabs.Count == 0) return;
            state.Tabs[state.SelectedTabIndex].Refresh();
        }

        public void RenameSelected(string newName)
        {
            if (state.Tabs.Count == 0) return;
            state.Tabs[state.SelectedTabIndex].Rename(newName);
            Save();
        }

        public void MoveTabInsert(int from, int insertBefore)
        {
            if (Tabs.Count == 0) return;

            int before = state.SelectedTabIndex;
            state.MoveTabInsert(from, insertBefore);

            if (before == from)
                state.SelectedTabIndex = insertBefore > from ? insertBefore - 1 : insertBefore;
            else if (from < before && before < insertBefore)
                state.SelectedTabIndex = before - 1;
            else if (insertBefore <= before && before < from)
                state.SelectedTabIndex = before + 1;

            Save();
        }

        public void Save()
        {
            string json = JsonUtility.ToJson(state, true);
            string dir = Path.GetDirectoryName(dataPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(dataPath, json, Encoding.UTF8);

            string normalized = dataPath.Replace("\\", "/");
            if (normalized.StartsWith("Assets/"))
                AssetDatabase.ImportAsset(normalized, ImportAssetOptions.ForceSynchronousImport);
            else
                AssetDatabase.Refresh();
        }
    }

    [Serializable]
    internal sealed class EditorFavorites_SaveData
    {
        [SerializeField] private List<FavoritesTab> tabs = new();
        [SerializeField] private int selectedTabIndex;
        [SerializeField, Range(0f, 1f)] private float itemScale01 = 1f;

        public IReadOnlyList<FavoritesTab> Tabs => tabs;
        public int SelectedTabIndex
        {
            get => Mathf.Clamp(selectedTabIndex, 0, Mathf.Max(0, tabs.Count - 1));
            set => selectedTabIndex = Mathf.Clamp(value, 0, Mathf.Max(0, tabs.Count - 1));
        }

        public float ItemScale01
        {
            get => Mathf.Clamp01(itemScale01);
            set => itemScale01 = Mathf.Clamp01(value);
        }

        public void AddTab(string name) => tabs.Add(new FavoritesTab(string.IsNullOrWhiteSpace(name) ? "Tab" : name.Trim()));
        public void RemoveAt(int index) { if (index >= 0 && index < tabs.Count) tabs.RemoveAt(index); }

        public void MoveTabInsert(int from, int insertBefore)
        {
            if (tabs == null || tabs.Count == 0) return;
            if (from < 0 || from >= tabs.Count) return;
            insertBefore = Mathf.Clamp(insertBefore, 0, tabs.Count);
            if (insertBefore == from || insertBefore == from + 1) return;

            FavoritesTab t = tabs[from];
            tabs.RemoveAt(from);
            if (from < insertBefore) insertBefore--;
            tabs.Insert(insertBefore, t);
        }
    }

    [Serializable]
    internal sealed class FavoritesTab
    {
        [SerializeField] private string name;
        [SerializeField] private List<FavoritesItem> items = new();

        public string Name => name;
        public IReadOnlyList<FavoritesItem> Items => items;

        public FavoritesTab(string tabName) { name = string.IsNullOrWhiteSpace(tabName) ? "Tab" : tabName.Trim(); }
        public void Rename(string newName) { if (!string.IsNullOrWhiteSpace(newName)) name = newName.Trim(); }

        public void Add(UnityEngine.Object obj)
        {
            if (!obj) return;
            string path = AssetDatabase.GetAssetPath(obj);
            string guid = string.IsNullOrEmpty(path) ? string.Empty : AssetDatabase.AssetPathToGUID(path);
            if (!string.IsNullOrEmpty(guid))
            {
                for (int i = 0; i < items.Count; i++)
                    if (items[i].Guid == guid) return;
            }
            items.Add(new FavoritesItem(obj));
        }

        public void RemoveAt(int index) { if (index >= 0 && index < items.Count) items.RemoveAt(index); }
        public void Move(int from, int to)
        {
            if (from == to || from < 0 || to < 0 || from >= items.Count || to >= items.Count) return;
            FavoritesItem it = items[from]; items.RemoveAt(from); items.Insert(to, it);
        }

        public void Refresh() { for (int i = 0; i < items.Count; i++) items[i].RefreshAsset(); }
    }

    [Serializable]
    internal sealed class FavoritesItem
    {
        [SerializeField] private UnityEngine.Object asset;
        [SerializeField] private string guid;

        public UnityEngine.Object Asset => asset;
        public string Guid => guid;

        public FavoritesItem(UnityEngine.Object obj) { Set(obj); }

        public void Set(UnityEngine.Object obj)
        {
            asset = obj;
            string path = AssetDatabase.GetAssetPath(asset);
            guid = string.IsNullOrEmpty(path) ? string.Empty : AssetDatabase.AssetPathToGUID(path);
        }

        public void RefreshAsset()
        {
            if (string.IsNullOrEmpty(guid)) return;
            string path = AssetDatabase.GUIDToAssetPath(guid);
            asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
        }
    }
}
#endif