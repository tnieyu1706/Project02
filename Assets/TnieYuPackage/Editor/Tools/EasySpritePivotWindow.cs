using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TnieYuPackage.Tools
{
    public class EasySpritePivotWindow : EditorWindow
    {
        enum PivotMode
        {
            Normalized,
            Pixel
        }

        class SpritePivotItem
        {
            public Sprite Sprite;
            public string AssetPath;

            public Vector2 OldPivotNormalized;
            public Vector2 NewPivotNormalized;
        }

        readonly List<SpritePivotItem> spriteItems = new();

        PivotMode pivotMode = PivotMode.Normalized;

        Vector2 pivotNormalized = new Vector2(0.5f, 0.5f);
        Vector2 pivotPixel;

        int gridColumns = 4;
        float previewSize = 128;

        Vector2 scroll;

        [MenuItem("Tools/TnieYu/Texture/Easy Sprite Pivot")]
        static void Open()
        {
            GetWindow<EasySpritePivotWindow>("Easy Sprite Pivot");
        }

        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();

            DrawLeftPanel();
            DrawRightPanel();

            EditorGUILayout.EndHorizontal();

            HandleDragAndDrop();
        }

        void DrawLeftPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(260));

            GUILayout.Label("Pivot Settings", EditorStyles.boldLabel);

            pivotMode = (PivotMode)EditorGUILayout.EnumPopup("Pivot Mode", pivotMode);

            float refWidth = 100;
            float refHeight = 100;

            if (spriteItems.Count > 0)
            {
                refWidth = spriteItems[0].Sprite.rect.width;
                refHeight = spriteItems[0].Sprite.rect.height;
            }

            if (pivotMode == PivotMode.Normalized)
            {
                pivotNormalized.x = EditorGUILayout.Slider("Pivot X", pivotNormalized.x, 0, 1);
                pivotNormalized.y = EditorGUILayout.Slider("Pivot Y", pivotNormalized.y, 0, 1);

                pivotPixel.x = pivotNormalized.x * refWidth;
                pivotPixel.y = pivotNormalized.y * refHeight;
            }
            else
            {
                pivotPixel.x = EditorGUILayout.FloatField("Pivot X (px)", pivotPixel.x);
                pivotPixel.y = EditorGUILayout.FloatField("Pivot Y (px)", pivotPixel.y);

                pivotNormalized.x = pivotPixel.x / refWidth;
                pivotNormalized.y = pivotPixel.y / refHeight;
            }

            GUILayout.Space(10);

            GUILayout.Label(
                $"Normalized: ({pivotNormalized.x:F2}, {pivotNormalized.y:F2})",
                EditorStyles.miniLabel
            );

            GUILayout.Space(10);

            GUILayout.Label("Preset");

            if (GUILayout.Button("Top Left")) SetPivotPreset(0, 1);
            if (GUILayout.Button("Top")) SetPivotPreset(0.5f, 1);
            if (GUILayout.Button("Top Right")) SetPivotPreset(1, 1);

            if (GUILayout.Button("Left")) SetPivotPreset(0, 0.5f);
            if (GUILayout.Button("Center")) SetPivotPreset(0.5f, 0.5f);
            if (GUILayout.Button("Right")) SetPivotPreset(1, 0.5f);

            if (GUILayout.Button("Bottom Left")) SetPivotPreset(0, 0);
            if (GUILayout.Button("Bottom")) SetPivotPreset(0.5f, 0);
            if (GUILayout.Button("Bottom Right")) SetPivotPreset(1, 0);

            GUILayout.Space(20);

            GUILayout.Label("Preview Settings", EditorStyles.boldLabel);

            gridColumns = EditorGUILayout.IntSlider("Grid Columns", gridColumns, 1, 10);
            previewSize = EditorGUILayout.Slider("Preview Size", previewSize, 64, 256);

            GUILayout.Space(10);

            GUILayout.Label($"Selected Sprites: {spriteItems.Count}");

            GUILayout.Space(10);

            if (GUILayout.Button("Add Selected From Project"))
            {
                AddSelectedSprites();
            }

            if (GUILayout.Button("Clear"))
            {
                spriteItems.Clear();
            }

            if (GUILayout.Button("Refresh"))
            {
                RefreshSprites();
            }

            GUILayout.Space(20);

            if (GUILayout.Button("Apply Pivot", GUILayout.Height(40)))
            {
                ApplyPivot();
            }

            EditorGUILayout.EndVertical();
        }

        void DrawRightPanel()
        {
            EditorGUILayout.BeginVertical();

            scroll = EditorGUILayout.BeginScrollView(scroll);

            int col = 0;

            EditorGUILayout.BeginHorizontal();

            foreach (var spriteItem in spriteItems)
            {
                DrawSpritePreview(spriteItem);

                col++;

                if (col >= gridColumns)
                {
                    col = 0;
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }

        void DrawSpritePreview(SpritePivotItem spriteItem)
        {
            Rect rect = GUILayoutUtility.GetRect(previewSize, previewSize);

            Texture2D preview = AssetPreview.GetAssetPreview(spriteItem.Sprite);

            if (preview != null)
                GUI.DrawTexture(rect, preview, ScaleMode.ScaleToFit);

            DrawPivot(rect, spriteItem.OldPivotNormalized, Color.red);
            DrawPivot(rect, pivotNormalized, Color.green);

            spriteItem.NewPivotNormalized = pivotNormalized;
        }

        void DrawPivot(Rect rect, Vector2 normalizedPivot, Color color)
        {
            Vector2 pos = new Vector2(
                rect.x + rect.width * normalizedPivot.x,
                rect.y + rect.height * (1 - normalizedPivot.y)
            );

            Handles.color = color;
            Handles.DrawSolidDisc(pos, Vector3.forward, 4);
        }

        void SetPivotPreset(float x, float y)
        {
            pivotNormalized = new Vector2(x, y);

            if (spriteItems.Count > 0)
            {
                float w = spriteItems[0].Sprite.rect.width;
                float h = spriteItems[0].Sprite.rect.height;

                pivotPixel = new Vector2(x * w, y * h);
            }
        }

        void HandleDragAndDrop()
        {
            Event evt = Event.current;

            Rect dropArea = new Rect(0, 0, position.width, position.height);

            if (!dropArea.Contains(evt.mousePosition))
                return;

            if (evt.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                evt.Use();
            }

            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();

                foreach (var draggedObject in DragAndDrop.objectReferences)
                {
                    if (draggedObject is Sprite sprite)
                    {
                        AddSprite(sprite);
                    }

                    if (draggedObject is Texture2D texture)
                    {
                        string path = AssetDatabase.GetAssetPath(texture);

                        Object[] subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);

                        if (subAssets.Length == 0)
                        {
                            Sprite singleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

                            if (singleSprite != null)
                                AddSprite(singleSprite);
                        }
                        else
                        {
                            foreach (var sub in subAssets)
                            {
                                if (sub is Sprite sp)
                                    AddSprite(sp);
                            }
                        }
                    }
                }

                evt.Use();
            }
        }

        void AddSelectedSprites()
        {
            foreach (var selected in Selection.objects)
            {
                if (selected is Sprite sprite)
                {
                    AddSprite(sprite);
                }

                if (selected is Texture2D texture)
                {
                    string path = AssetDatabase.GetAssetPath(texture);

                    Object[] subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);

                    if (subAssets.Length == 0)
                    {
                        Sprite singleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

                        if (singleSprite != null)
                            AddSprite(singleSprite);
                    }
                    else
                    {
                        foreach (var sub in subAssets)
                        {
                            if (sub is Sprite sp)
                                AddSprite(sp);
                        }
                    }
                }
            }
        }

        void AddSprite(Sprite sprite)
        {
            string path = AssetDatabase.GetAssetPath(sprite);

            SpritePivotItem newItem = new()
            {
                Sprite = sprite,
                AssetPath = path,
                OldPivotNormalized = new Vector2(
                    sprite.pivot.x / sprite.rect.width,
                    sprite.pivot.y / sprite.rect.height
                ),
                NewPivotNormalized = pivotNormalized
            };

            spriteItems.Add(newItem);
        }

        void ApplyPivot()
        {
            Dictionary<string, List<SpritePivotItem>> groups = new();

            foreach (var spriteItem in spriteItems)
            {
                if (!groups.ContainsKey(spriteItem.AssetPath))
                    groups[spriteItem.AssetPath] = new List<SpritePivotItem>();

                groups[spriteItem.AssetPath].Add(spriteItem);
            }

            foreach (var group in groups)
            {
                string path = group.Key;

                TextureImporter importer =
                    (TextureImporter)AssetImporter.GetAtPath(path);

                if (importer == null)
                    continue;

                // -------- Single Sprite --------
                if (importer.spriteImportMode == SpriteImportMode.Single)
                {
                    SpritePivotItem first = group.Value[0];

                    SerializedObject so = new SerializedObject(importer);
                    SerializedProperty alignmentProp = so.FindProperty("m_Alignment");

                    if (alignmentProp != null)
                        alignmentProp.intValue = (int)SpriteAlignment.Custom;

                    so.ApplyModifiedPropertiesWithoutUndo();

                    importer.spritePivot = first.NewPivotNormalized;
                }

                // -------- Multiple Sprite --------
                else if (importer.spriteImportMode == SpriteImportMode.Multiple)
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    var metas = importer.spritesheet;
#pragma warning restore CS0618 // Type or member is obsolete

                    foreach (var pivotItem in group.Value)
                    {
                        for (int i = 0; i < metas.Length; i++)
                        {
                            if (metas[i].name == pivotItem.Sprite.name)
                            {
                                metas[i].alignment = (int)SpriteAlignment.Custom;
                                metas[i].pivot = pivotItem.NewPivotNormalized;
                            }
                        }
                    }

#pragma warning disable CS0618 // Type or member is obsolete
                    importer.spritesheet = metas;
#pragma warning restore CS0618 // Type or member is obsolete
                }

                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
            }

            RefreshSprites();

            Debug.Log("Pivot Applied!");
        }

        void RefreshSprites()
        {
            foreach (var spriteItem in spriteItems)
            {
                Object[] subAssets =
                    AssetDatabase.LoadAllAssetRepresentationsAtPath(spriteItem.AssetPath);

                foreach (var sub in subAssets)
                {
                    if (sub is Sprite sp && sp.name == spriteItem.Sprite.name)
                    {
                        spriteItem.Sprite = sp;

                        spriteItem.OldPivotNormalized = new Vector2(
                            sp.pivot.x / sp.rect.width,
                            sp.pivot.y / sp.rect.height
                        );
                    }
                }
            }

            Repaint();
        }
    }
}