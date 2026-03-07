using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TnieYuPackage.Tools
{
    public class SpriteSliceExtractor : EditorWindow
    {
        private Texture2D sourceTexture;
        private DefaultAsset outputFolder;

        private SpriteMetaData[] sprites;
        private string[] spriteNames;

        private int selectedIndex;

        private Vector2 scroll;

        [MenuItem("Tools/TnieYu/Texture/Extract Slide In Sprite")]
        public static void Open()
        {
            GetWindow<SpriteSliceExtractor>("Sprite Extractor");
        }

        private void OnGUI()
        {
            GUILayout.Space(5);

            EditorGUI.BeginChangeCheck();

            sourceTexture = (Texture2D)EditorGUILayout.ObjectField(
                "Texture Atlas",
                sourceTexture,
                typeof(Texture2D),
                false);

            if (EditorGUI.EndChangeCheck())
            {
                LoadSprites();
            }

            outputFolder = (DefaultAsset)EditorGUILayout.ObjectField(
                "Output Folder",
                outputFolder,
                typeof(DefaultAsset),
                false);

            GUILayout.Space(10);

            if (sprites == null || sprites.Length == 0)
            {
                EditorGUILayout.HelpBox(
                    "No sliced sprites found.",
                    MessageType.Info);
                return;
            }

            selectedIndex = EditorGUILayout.Popup(
                "Slice Sprite",
                selectedIndex,
                spriteNames);

            GUILayout.Space(10);

            DrawPreview();

            GUILayout.Space(10);

            if (GUILayout.Button("Extract Selected Sprite"))
            {
                ExtractSprite(sprites[selectedIndex]);
            }

            if (GUILayout.Button("Extract ALL Sprites"))
            {
                foreach (var sprite in sprites)
                {
                    ExtractSprite(sprite);
                }

                Debug.Log("All sprites exported");
            }
        }

        private void LoadSprites()
        {
            if (sourceTexture == null)
                return;

            string path = AssetDatabase.GetAssetPath(sourceTexture);

            TextureImporter importer =
                (TextureImporter)AssetImporter.GetAtPath(path);

            if (importer.spriteImportMode != SpriteImportMode.Multiple)
            {
                Debug.LogWarning("Texture is not sliced.");
                return;
            }

            sprites = importer.spritesheet;

            spriteNames = sprites
                .Select(s => s.name)
                .ToArray();

            selectedIndex = 0;
        }

        private void DrawPreview()
        {
            if (sourceTexture == null)
                return;

            if (sprites == null || sprites.Length == 0)
                return;

            Rect rect = sprites[selectedIndex].rect;

            float size = 150;

            Rect previewRect = GUILayoutUtility.GetRect(size, size);

            Rect texCoords = new Rect(
                rect.x / sourceTexture.width,
                rect.y / sourceTexture.height,
                rect.width / sourceTexture.width,
                rect.height / sourceTexture.height);

            GUI.DrawTextureWithTexCoords(
                previewRect,
                sourceTexture,
                texCoords);
        }

        private void ExtractSprite(SpriteMetaData meta)
        {
            if (outputFolder == null)
            {
                Debug.LogError("Output folder not selected");
                return;
            }

            Rect rect = meta.rect;

            Texture2D newTex = new Texture2D(
                (int)rect.width,
                (int)rect.height,
                TextureFormat.RGBA32,
                false);

            Color[] pixels = sourceTexture.GetPixels(
                (int)rect.x,
                (int)rect.y,
                (int)rect.width,
                (int)rect.height);

            newTex.SetPixels(pixels);
            newTex.Apply();

            byte[] png = newTex.EncodeToPNG();

            string folder = AssetDatabase.GetAssetPath(outputFolder);

            string path = Path.Combine(folder, meta.name + ".png");

            File.WriteAllBytes(path, png);

            AssetDatabase.Refresh();

            Debug.Log("Exported: " + path);
        }
    }
}