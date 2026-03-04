using System.IO;
using UnityEditor;
using UnityEngine;

namespace TnieYuPackage.Tools
{
    public class AdvancedTextureGenerator : EditorWindow
    {
        private int width = 256;
        private int height = 256;

        private enum Mode { SimpleColor, Gradient }
        private enum GradientType { Vertical, Horizontal, Radial }
        private enum Shape { Square, Rectangle, Circle, RoundedRectangle }

        private Mode mode = Mode.SimpleColor;
        private GradientType gradientType = GradientType.Vertical;
        private Shape shape = Shape.Rectangle;

        private Color color1 = Color.white;
        private Color color2 = Color.black;

        private float cornerRadius = 20f;

        private string fileName = "NewTexture";
        private DefaultAsset saveFolder;

        private Texture2D previewTexture;

        [MenuItem("Tools/TnieYu/Advanced Texture Generator")]
        public static void Open()
        {
            GetWindow<AdvancedTextureGenerator>("Texture Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Texture Size", EditorStyles.boldLabel);

            shape = (Shape)EditorGUILayout.EnumPopup("Shape", shape);

            if (shape == Shape.Square)
            {
                width = EditorGUILayout.IntField("Size", width);
                height = width;
            }
            else
            {
                width = EditorGUILayout.IntField("Width", width);
                height = EditorGUILayout.IntField("Height", height);
            }

            if (shape == Shape.RoundedRectangle)
            {
                cornerRadius = EditorGUILayout.FloatField("Corner Radius", cornerRadius);
            }

            GUILayout.Space(10);

            mode = (Mode)EditorGUILayout.EnumPopup("Mode", mode);

            color1 = EditorGUILayout.ColorField("Color 1", color1);

            if (mode == Mode.Gradient)
            {
                color2 = EditorGUILayout.ColorField("Color 2", color2);
                gradientType = (GradientType)EditorGUILayout.EnumPopup("Gradient Type", gradientType);
            }

            GUILayout.Space(15);

            GUILayout.Label("Save Settings", EditorStyles.boldLabel);
            fileName = EditorGUILayout.TextField("File Name", fileName);

            EditorGUILayout.BeginHorizontal();
            saveFolder = (DefaultAsset)EditorGUILayout.ObjectField(
                "Save Folder",
                saveFolder,
                typeof(DefaultAsset),
                false);

            if (GUILayout.Button("Browse", GUILayout.Width(70)))
            {
                string path = EditorUtility.OpenFolderPanel("Select Folder Inside Assets", "Assets", "");
                if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
                {
                    string relative = "Assets" + path.Substring(Application.dataPath.Length);
                    saveFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(relative);
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(15);

            if (GUILayout.Button("Generate Preview"))
                GenerateTexture();

            if (previewTexture != null)
            {
                GUILayout.Space(10);
                GUILayout.Label("Preview");
                GUILayout.Label(previewTexture, GUILayout.Width(256), GUILayout.Height(256));

                if (GUILayout.Button("Export PNG"))
                    SaveTexture();
            }
        }

        private void GenerateTexture()
        {
            previewTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / width;
                    float v = (float)y / height;

                    Color finalColor = color1;

                    if (mode == Mode.Gradient)
                    {
                        switch (gradientType)
                        {
                            case GradientType.Vertical:
                                finalColor = Color.Lerp(color1, color2, v);
                                break;

                            case GradientType.Horizontal:
                                finalColor = Color.Lerp(color1, color2, u);
                                break;

                            case GradientType.Radial:
                                float dx = u - 0.5f;
                                float dy = v - 0.5f;
                                float dist = Mathf.Clamp01(Mathf.Sqrt(dx * dx + dy * dy) * 2f);
                                finalColor = Color.Lerp(color1, color2, dist);
                                break;
                        }
                    }

                    if (!IsInsideShape(x, y))
                        finalColor.a = 0;

                    previewTexture.SetPixel(x, y, finalColor);
                }
            }

            previewTexture.Apply();
        }

        private bool IsInsideShape(int x, int y)
        {
            switch (shape)
            {
                case Shape.Rectangle:
                case Shape.Square:
                    return true;

                case Shape.Circle:
                {
                    float cx = width / 2f;
                    float cy = height / 2f;
                    float dx = x - cx;
                    float dy = y - cy;
                    float radius = Mathf.Min(width, height) / 2f;
                    return dx * dx + dy * dy <= radius * radius;
                }

                case Shape.RoundedRectangle:
                {
                    float r = Mathf.Clamp(cornerRadius, 0, Mathf.Min(width, height) / 2f);

                    // Vùng trung tâm (không phải góc)
                    if (x >= r && x < width - r) return true;
                    if (y >= r && y < height - r) return true;

                    // Góc trên trái
                    if (x < r && y >= height - r)
                    {
                        float dx = x - r;
                        float dy = y - (height - r);
                        return dx * dx + dy * dy <= r * r;
                    }

                    // Góc trên phải
                    if (x >= width - r && y >= height - r)
                    {
                        float dx = x - (width - r);
                        float dy = y - (height - r);
                        return dx * dx + dy * dy <= r * r;
                    }

                    // Góc dưới trái
                    if (x < r && y < r)
                    {
                        float dx = x - r;
                        float dy = y - r;
                        return dx * dx + dy * dy <= r * r;
                    }

                    // Góc dưới phải
                    if (x >= width - r && y < r)
                    {
                        float dx = x - (width - r);
                        float dy = y - r;
                        return dx * dx + dy * dy <= r * r;
                    }

                    return false;
                }
            }

            return true;
        }

        private void SaveTexture()
        {
            if (saveFolder == null || string.IsNullOrEmpty(fileName))
            {
                EditorUtility.DisplayDialog("Error", "Missing folder or filename.", "OK");
                return;
            }

            string folderPath = AssetDatabase.GetAssetPath(saveFolder);
            string fullPath = Path.Combine(folderPath, fileName + ".png");

            File.WriteAllBytes(fullPath, previewTexture.EncodeToPNG());
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success", "Saved to:\n" + fullPath, "OK");
        }
    }
}