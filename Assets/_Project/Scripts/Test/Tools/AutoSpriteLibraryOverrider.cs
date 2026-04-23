using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class AutoSpriteLibraryOverrider : EditorWindow
{
    private SpriteLibraryAsset targetLibrary;
    private string targetFolderPath = "";
    private bool isHoveringDropArea = false;

    // Class phụ trợ chứa thông tin sẽ ghi đè
    private class OverrideInfo
    {
        public Sprite sprite;
        public string category;
        public string label;
    }

    [MenuItem("Tools/Auto Sprite Library Overrider")]
    public static void ShowWindow()
    {
        AutoSpriteLibraryOverrider window = GetWindow<AutoSpriteLibraryOverrider>("Auto Sprite Overrider");
        window.minSize = new Vector2(450, 350);
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("1. Gắn Sprite Library Override cần thao tác", EditorStyles.boldLabel);
        targetLibrary =
            (SpriteLibraryAsset)EditorGUILayout.ObjectField("Target Library", targetLibrary, typeof(SpriteLibraryAsset),
                false);

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("2. Kéo thả Folder chứa source (các Texture2D Multiple)", EditorStyles.boldLabel);

        DrawDragAndDropArea();

        EditorGUILayout.Space(20);

        if (!string.IsNullOrEmpty(targetFolderPath))
        {
            EditorGUILayout.HelpBox($"Folder Source đang chọn:\n{targetFolderPath}", MessageType.Info);

            if (targetLibrary != null)
            {
                GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f);
                if (GUILayout.Button("TIẾN HÀNH GHI ĐÈ (OVERRIDE)", GUILayout.Height(45)))
                {
                    ExecuteOverride();
                }

                GUI.backgroundColor = Color.white;
            }
            else
            {
                EditorGUILayout.HelpBox("Vui lòng gắn Target Library ở bước 1 để tiến hành.", MessageType.Warning);
            }
        }
    }

    private void DrawDragAndDropArea()
    {
        Event evt = Event.current;
        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 100.0f, GUILayout.ExpandWidth(true));

        GUIStyle dropStyle = new GUIStyle(GUI.skin.box);
        dropStyle.alignment = TextAnchor.MiddleCenter;
        dropStyle.fontSize = 14;
        dropStyle.normal.textColor = isHoveringDropArea ? Color.cyan : Color.white;

        GUI.Box(dropArea, isHoveringDropArea ? "THẢ FOLDER VÀO ĐÂY!" : "Kéo / Thả Folder vào khu vực này", dropStyle);

        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!dropArea.Contains(evt.mousePosition))
                {
                    isHoveringDropArea = false;
                    return;
                }

                isHoveringDropArea = true;
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (string path in DragAndDrop.paths)
                    {
                        if (Directory.Exists(path))
                        {
                            targetFolderPath = path;
                            isHoveringDropArea = false;
                            break;
                        }
                    }
                }

                evt.Use();
                break;

            case EventType.DragExited:
                isHoveringDropArea = false;
                break;
        }
    }

    private void ExecuteOverride()
    {
        if (targetLibrary == null || string.IsNullOrEmpty(targetFolderPath)) return;

        var categories = targetLibrary.GetCategoryNames().ToList();
        List<OverrideInfo> overridesToApply = new List<OverrideInfo>();
        int updatedCategoriesCount = 0;

        foreach (string category in categories)
        {
            string[] guids = AssetDatabase.FindAssets($"t:Texture2D", new[] { targetFolderPath });
            string matchingTexturePath = null;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string textureName = Path.GetFileNameWithoutExtension(path);

                if (textureName.Equals(category, StringComparison.OrdinalIgnoreCase))
                {
                    matchingTexturePath = path;
                    break;
                }
            }

            if (string.IsNullOrEmpty(matchingTexturePath)) continue;

            TextureImporter importer = AssetImporter.GetAtPath(matchingTexturePath) as TextureImporter;
            if (importer != null && importer.spriteImportMode != SpriteImportMode.Multiple) continue;

            List<Sprite> sourceSprites = AssetDatabase.LoadAllAssetsAtPath(matchingTexturePath)
                .OfType<Sprite>()
                .OrderBy(s => s.name)
                .ToList();

            if (sourceSprites.Count == 0) continue;

            var labels = targetLibrary.GetCategoryLabelNames(category).ToList();
            if (labels.Count == 0) continue;

            int labelCount = labels.Count;
            int spriteCount = sourceSprites.Count;

            for (int i = 0; i < labelCount; i++)
            {
                string label = labels[i];
                int mappedIndex = Mathf.Clamp(Mathf.FloorToInt((float)i / labelCount * spriteCount), 0,
                    spriteCount - 1);

                overridesToApply.Add(new OverrideInfo
                {
                    sprite = sourceSprites[mappedIndex],
                    category = category,
                    label = label
                });
            }

            updatedCategoriesCount++;
        }

        if (overridesToApply.Count == 0)
        {
            EditorUtility.DisplayDialog("Lỗi",
                "Không tìm thấy dữ liệu hợp lệ để đè. Kiểm tra tên Texture2D hoặc chế độ Sprite Mode Multiple.", "OK");
            return;
        }

        string assetPath = AssetDatabase.GetAssetPath(targetLibrary);
        bool isSpriteLib = assetPath.EndsWith(".spriteLib", StringComparison.OrdinalIgnoreCase);

        if (isSpriteLib)
        {
            SaveToSpriteLibFileWithVessel(assetPath, overridesToApply, updatedCategoriesCount);
        }
        else
        {
            Undo.RecordObject(targetLibrary, "Auto Override Sprite Library");
            foreach (var ov in overridesToApply)
            {
                targetLibrary.AddCategoryLabel(ov.sprite, ov.category, ov.label);
            }

            EditorUtility.SetDirty(targetLibrary);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Hoàn tất",
                $"Ghi đè thành công!\n\n- Đã xử lý {updatedCategoriesCount} Categories.\n- Đã ghi đè {overridesToApply.Count} Labels.\n\nHãy bấm vào file Override để kiểm tra.",
                "Tuyệt vời");
        }
    }

    private void SaveToSpriteLibFileWithVessel(string assetPath, List<OverrideInfo> overrides, int categoriesCount)
    {
        string json = File.ReadAllText(assetPath);

        // Loại bỏ các ký tự ẩn (BOM) sinh ra khi lưu file
        json = json.Trim('\uFEFF', '\u200B', ' ', '\t', '\r', '\n');

        // BẮT LỖI YAML: Đề phòng file của bạn bị Unity lưu dưới chuẩn cũ
        if (json.StartsWith("%YAML"))
        {
            EditorUtility.DisplayDialog("Cảnh báo định dạng",
                "File Sprite Library của bạn đang ở định dạng YAML cũ.\n\nHãy tạo một file Sprite Library Override mới tinh trong cửa sổ Project (Create -> 2D -> Sprite Library Asset) rồi thao tác lại nhé.",
                "OK");
            return;
        }

        SpriteLibSourceVessel vessel = ScriptableObject.CreateInstance<SpriteLibSourceVessel>();

        try
        {
            EditorJsonUtility.FromJsonOverwrite(json, vessel);

            foreach (var ov in overrides)
            {
                var cat = vessel.labels.Find(c => c.name == ov.category);
                if (cat == null)
                {
                    // Ép kiểu uint sang long để đảm bảo Hash không bao giờ bị số âm hoặc tràn bộ nhớ
                    cat = new SpriteLibCategoryData
                    {
                        name = ov.category,
                        hash = (long)(uint)Animator.StringToHash(ov.category),
                        categoryList = new List<SpriteLibEntryData>()
                    };
                    vessel.labels.Add(cat);
                }

                var entry = cat.categoryList.Find(e => e.name == ov.label);
                if (entry == null)
                {
                    entry = new SpriteLibEntryData
                    {
                        name = ov.label,
                        hash = (long)(uint)Animator.StringToHash(ov.label),
                        sprite = new SpriteLibSpriteReference()
                    };
                    cat.categoryList.Add(entry);
                }

                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(ov.sprite, out string guid, out long fileId);

                if (entry.sprite == null) entry.sprite = new SpriteLibSpriteReference();
                entry.sprite.fileID = fileId;
                entry.sprite.guid = guid;
                entry.sprite.type = 3;
            }

            string newJson = EditorJsonUtility.ToJson(vessel, true);
            File.WriteAllText(assetPath, newJson);

            AssetDatabase.ImportAsset(assetPath);
            EditorUtility.DisplayDialog("Hoàn tất",
                $"Ghi đè thành công!\n\n- Đã xử lý {categoriesCount} Categories.\n- Đã ghi đè {overrides.Count} Labels.\n\nHãy bấm vào file Override để kiểm tra thành quả.",
                "Tuyệt vời");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Auto Overrider] Gặp lỗi nghiêm trọng: {ex.Message}");
        }
        finally
        {
            DestroyImmediate(vessel);
        }
    }
}

// =========================================================================
// SỬ DỤNG 'LONG' THAY CHO 'INT' ĐỂ GIẢI QUYẾT TRIỆT ĐỂ LỖI OVERFLOW HASH CỦA UNITY
// =========================================================================
public class SpriteLibSourceVessel : ScriptableObject
{
    public string name;
    public string primaryLibraryGUID;
    public long version;
    public List<SpriteLibCategoryData> labels = new List<SpriteLibCategoryData>();
}

[Serializable]
public class SpriteLibCategoryData
{
    public string name;
    public long hash;
    public List<SpriteLibEntryData> categoryList = new List<SpriteLibEntryData>();
}

[Serializable]
public class SpriteLibEntryData
{
    public string name;
    public long hash;
    public SpriteLibSpriteReference sprite;
}

[Serializable]
public class SpriteLibSpriteReference
{
    public long fileID;
    public string guid;
    public long type;
}