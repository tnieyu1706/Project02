using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.IO;

public enum SliceMode
{
    Auto,
    SelfDefine
}

public class QuickSliceTextureMultiple : EditorWindow
{
    private List<Texture2D> texturesToProcess = new List<Texture2D>();
    private Vector2 scrollPosition;

    // Các biến cho chế độ Slice
    private SliceMode currentMode = SliceMode.Auto;
    private int defineColumns = 1;
    private int defineRows = 1;

    // Tạo menu trên thanh công cụ của Unity
    [MenuItem("Tools/Quick Slice Texture Multiple")]
    public static void ShowWindow()
    {
        EditorWindow window = GetWindow<QuickSliceTextureMultiple>("Quick Slice");
        window.minSize = new Vector2(300, 450);
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Kéo thả Textures vào ô bên dưới", EditorStyles.boldLabel);

        // --- KHU VỰC DRAG & DROP ---
        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "\nKéo thả Textures vào đây", EditorStyles.helpBox);
        HandleDragAndDrop(dropArea);

        GUILayout.Space(10);

        // --- CÀI ĐẶT CHẾ ĐỘ SLICE ---
        GUILayout.Label("Cài đặt Slice", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        currentMode = (SliceMode)EditorGUILayout.EnumPopup("Chế độ", currentMode);

        if (currentMode == SliceMode.SelfDefine)
        {
            defineColumns = EditorGUILayout.IntField("Số Cột (Columns)", defineColumns);
            defineRows = EditorGUILayout.IntField("Số Hàng (Rows)", defineRows);

            // Giới hạn giá trị tối thiểu là 1 để tránh lỗi chia cho 0
            if (defineColumns < 1) defineColumns = 1;
            if (defineRows < 1) defineRows = 1;
        }
        
        EditorGUILayout.EndVertical();

        GUILayout.Space(10);

        // --- CÁC NÚT ĐIỀU KHIỂN ---
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear List", GUILayout.Height(30)))
        {
            texturesToProcess.Clear();
        }

        GUI.enabled = texturesToProcess.Count > 0;
        if (GUILayout.Button("Bắt đầu Slice", GUILayout.Height(30)))
        {
            ProcessTextures();
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        // --- DANH SÁCH TEXTURE ĐÃ THÊM ---
        GUILayout.Label($"Danh sách Textures ({texturesToProcess.Count}):", EditorStyles.boldLabel);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        for (int i = 0; i < texturesToProcess.Count; i++)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            texturesToProcess[i] = (Texture2D)EditorGUILayout.ObjectField(texturesToProcess[i], typeof(Texture2D), false);
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                texturesToProcess.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }

    // Xử lý logic khi kéo thả file vào cửa sổ
    private void HandleDragAndDrop(Rect dropArea)
    {
        Event currentEvent = Event.current;
        EventType currentEventType = currentEvent.type;

        if (!dropArea.Contains(currentEvent.mousePosition))
            return;

        if (currentEventType == EventType.DragUpdated || currentEventType == EventType.DragPerform)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

            if (currentEventType == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                foreach (Object draggedObject in DragAndDrop.objectReferences)
                {
                    if (draggedObject is Texture2D texture)
                    {
                        // 1. FILTER: Chỉ nhận file nếu nó là Sprite và có mode là Multiple
                        string path = AssetDatabase.GetAssetPath(texture);
                        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                        
                        if (importer != null && 
                            importer.textureType == TextureImporterType.Sprite && 
                            importer.spriteImportMode == SpriteImportMode.Multiple)
                        {
                            if (!texturesToProcess.Contains(texture))
                            {
                                texturesToProcess.Add(texture);
                            }
                        }
                    }
                }
            }
            currentEvent.Use();
        }
    }

    // Hàm thực thi logic Slice
    private void ProcessTextures()
    {
        int total = texturesToProcess.Count;
        for (int i = 0; i < total; i++)
        {
            Texture2D tex = texturesToProcess[i];
            if (tex == null) continue;

            string path = AssetDatabase.GetAssetPath(tex);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer == null) continue;

            EditorUtility.DisplayProgressBar("Đang xử lý...", $"Đang slice texture: {tex.name}", (float)i / total);

            // Lưu lại cấu hình cũ để trả lại sau khi xử lý xong
            bool wasReadable = importer.isReadable;
            TextureImporterCompression oldCompression = importer.textureCompression;

            // Đảm bảo có thể đọc được pixel chính xác nhất:
            // Bật isReadable và tạm thời tắt nén ảnh
            importer.isReadable = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();

            // QUAN TRỌNG NHẤT: Load lại dữ liệu Texture sau khi Reimport để cập nhật bộ nhớ
            tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

            if (currentMode == SliceMode.Auto)
            {
                ProcessAutoSlice(tex, importer);
            }
            else if (currentMode == SliceMode.SelfDefine)
            {
                ProcessSelfDefineSlice(tex, importer);
            }

            // Trả lại trạng thái cũ để tối ưu bộ nhớ / dung lượng
            importer.isReadable = wasReadable;
            importer.textureCompression = oldCompression;
            
            // Lưu và Reimport để áp dụng thay đổi
            importer.SaveAndReimport();
        }

        EditorUtility.ClearProgressBar();
        Debug.Log($"[Quick Slice] Đã hoàn thành xử lý {total} textures!");
    }

    private void ProcessAutoSlice(Texture2D tex, TextureImporter importer)
    {
        // Dùng cơ chế Automatic Slice để đếm số lượng sprite
        Rect[] autoRects = InternalSpriteUtility.GenerateAutomaticSpriteRectangles(tex, 4, 0);
        int spriteCount = autoRects.Length;

        if (spriteCount > 0)
        {
            // Grid by Cell Count (Row = 1, Column = spriteCount)
            float cellWidth = (float)tex.width / spriteCount;
            float cellHeight = tex.height; // Chỉ có 1 hàng nên chiều cao bằng luôn chiều cao texture

            SpriteMetaData[] newSpriteMetaData = new SpriteMetaData[spriteCount];

            for (int col = 0; col < spriteCount; col++)
            {
                SpriteMetaData smd = new SpriteMetaData();
                smd.name = $"{tex.name}_{col}";
                
                // Tính toán tọa độ cắt (Grid: x dịch theo column, y luôn = 0)
                smd.rect = new Rect(col * cellWidth, 0, cellWidth, cellHeight);
                
                // Thiết lập Pivot ở chính giữa
                smd.alignment = (int)SpriteAlignment.Center;
                smd.pivot = new Vector2(0.5f, 0.5f);
                
                newSpriteMetaData[col] = smd;
            }

            importer.spritesheet = newSpriteMetaData;
        }
        else
        {
            Debug.LogWarning($"[Quick Slice] Texture '{tex.name}' không tìm thấy phần tử nào để tự động cắt. Hãy đảm bảo ảnh có nền trong suốt (Alpha Is Transparency).");
        }
    }

    private void ProcessSelfDefineSlice(Texture2D tex, TextureImporter importer)
    {
        int totalSprites = defineColumns * defineRows;
        float cellWidth = (float)tex.width / defineColumns;
        float cellHeight = (float)tex.height / defineRows;

        SpriteMetaData[] newSpriteMetaData = new SpriteMetaData[totalSprites];
        int spriteIndex = 0;

        // Trục Y của Unity bắt đầu từ dưới cùng (bottom), nhưng ta thường đọc từ trên xuống (top to bottom)
        // Nên vòng lặp row sẽ chạy lùi từ hàng cao nhất xuống hàng thấp nhất
        for (int row = defineRows - 1; row >= 0; row--)
        {
            for (int col = 0; col < defineColumns; col++)
            {
                SpriteMetaData smd = new SpriteMetaData();
                smd.name = $"{tex.name}_{spriteIndex}";
                
                // Tính toán tọa độ cắt
                smd.rect = new Rect(col * cellWidth, row * cellHeight, cellWidth, cellHeight);
                
                // Thiết lập Pivot ở chính giữa
                smd.alignment = (int)SpriteAlignment.Center;
                smd.pivot = new Vector2(0.5f, 0.5f);
                
                newSpriteMetaData[spriteIndex] = smd;
                spriteIndex++;
            }
        }

        importer.spritesheet = newSpriteMetaData;
    }
}