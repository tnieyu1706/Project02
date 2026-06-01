#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Game.Global;

namespace Game.Editor
{
    public class GameSkillTreeGraphWindow : EditorWindow
    {
        // Cấu hình Layout
        private GameSkillData rootSkill;
        private float radiusStep = 150f;
        private float angleSpread = 360f;
        private float rotation = -90f; // Mặc định -90 để rễ hướng lên trên/xuống dưới tùy chỉnh

        // Dữ liệu nội bộ
        private GameSkillTree currentTree;
        private GameSkillTreeRadialLayout layoutCalculator;
        private Dictionary<string, GameSkillTreeRadialLayout.NodeLayoutData> layoutData;
        private List<GameSkillData> allSkills;

        // Viewport & Controls
        private Vector2 panOffset = Vector2.zero;
        private float zoom = 1.0f;
        private Vector2 dragStartPos;

        // Kích thước hiển thị Node (đã được điều chỉnh rộng và cao hơn)
        private const float NODE_WIDTH = 140f;
        private const float NODE_HEIGHT = 65f;
        private const float ICON_SIZE = 32f;

        [MenuItem("Window/Game/Skill Tree Graph")]
        public static void ShowWindow()
        {
            var window = GetWindow<GameSkillTreeGraphWindow>("Skill Tree Graph");
            window.minSize = new Vector2(600, 400);
            window.Show();
        }

        private void OnEnable()
        {
            layoutCalculator = new GameSkillTreeRadialLayout(radiusStep, angleSpread, rotation);
            LoadAndBuildTree();
        }

        private void OnGUI()
        {
            DrawToolbar();

            // Khu vực vẽ đồ thị
            Rect graphRect = new Rect(0, 50, position.width, position.height - 50);
            GUI.Box(graphRect, "", new GUIStyle("window"));

            HandleEvents(graphRect);

            if (currentTree != null && layoutData != null)
            {
                // Bắt đầu ma trận transform cho Pan và Zoom
                GUI.EndGroup(); // Kết thúc group mặc định của EditorWindow

                Matrix4x4 oldMatrix = GUI.matrix;

                // Tính toán điểm gốc ở giữa màn hình
                Vector2 origin = new Vector2(position.width / 2f, position.height / 2f) + panOffset;

                // Áp dụng scale (zoom) và translation (pan)
                Matrix4x4 translation = Matrix4x4.Translate(new Vector3(origin.x, origin.y, 0));
                Matrix4x4 scale = Matrix4x4.Scale(new Vector3(zoom, zoom, 1));
                GUI.matrix = translation * scale * Matrix4x4.Translate(new Vector3(-origin.x, -origin.y, 0)) *
                             Matrix4x4.Translate(new Vector3(origin.x, origin.y, 0));

                DrawEdges(currentTree.Root);
                DrawNodes(currentTree.Root);

                GUI.matrix = oldMatrix;
                GUI.BeginGroup(new Rect(0, 0, position.width, position.height));
            }
            else
            {
                GUI.Label(new Rect(position.width / 2 - 100, position.height / 2, 200, 30),
                    "Vui lòng chọn Root Skill và nhấn Rebuild", EditorStyles.boldLabel);
            }
        }

        private void DrawToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            EditorGUI.BeginChangeCheck();
            rootSkill = (GameSkillData)EditorGUILayout.ObjectField("Root Skill", rootSkill, typeof(GameSkillData),
                false, GUILayout.Width(250));
            if (EditorGUI.EndChangeCheck() && rootSkill != null)
            {
                LoadAndBuildTree();
            }

            if (GUILayout.Button("Rebuild Graph", EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                LoadAndBuildTree();
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Center View", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                panOffset = Vector2.zero;
                zoom = 1.0f;
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            EditorGUI.BeginChangeCheck();
            radiusStep = EditorGUILayout.Slider("Radius Step", radiusStep, 50f, 500f, GUILayout.Width(250));
            angleSpread = EditorGUILayout.Slider("Angle Spread", angleSpread, 10f, 360f, GUILayout.Width(250));
            rotation = EditorGUILayout.Slider("Rotation", rotation, -360f, 360f, GUILayout.Width(250));

            if (EditorGUI.EndChangeCheck())
            {
                if (layoutCalculator != null)
                {
                    layoutCalculator.RadiusStep = radiusStep;
                    layoutCalculator.AngleSpread = angleSpread;
                    layoutCalculator.Rotation = rotation;
                    if (currentTree != null)
                    {
                        layoutData = layoutCalculator.CalculateLayout(currentTree);
                    }
                }
            }

            GUILayout.EndHorizontal();
        }

        private void LoadAndBuildTree()
        {
            if (rootSkill == null) return;

            // Tìm tất cả GameSkillData trong project
            string[] guids = AssetDatabase.FindAssets("t:GameSkillData");
            allSkills = new List<GameSkillData>();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                allSkills.Add(AssetDatabase.LoadAssetAtPath<GameSkillData>(path));
            }

            // Khởi tạo cây và tính toán layout
            currentTree = new GameSkillTree(allSkills, rootSkill);
            layoutData = layoutCalculator.CalculateLayout(currentTree);

            Repaint();
        }

        private void HandleEvents(Rect graphRect)
        {
            Event e = Event.current;

            if (!graphRect.Contains(e.mousePosition)) return;

            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 2 || e.button == 1) // Middle or Right click to pan
                    {
                        dragStartPos = e.mousePosition;
                        e.Use();
                    }

                    break;

                case EventType.MouseDrag:
                    if (e.button == 2 || e.button == 1)
                    {
                        panOffset += (e.mousePosition - dragStartPos) * (1f / zoom);
                        dragStartPos = e.mousePosition;
                        Repaint();
                        e.Use();
                    }

                    break;

                case EventType.ScrollWheel:
                    float zoomDelta = -e.delta.y * 0.05f;
                    zoom = Mathf.Clamp(zoom + zoomDelta, 0.2f, 3.0f);
                    Repaint();
                    e.Use();
                    break;
            }
        }

        private void DrawEdges(GameSkillTreeNode node)
        {
            if (node == null || node.Children == null || layoutData == null) return;

            if (!layoutData.TryGetValue(node.Data.skillId, out var startData)) return;

            foreach (var child in node.Children)
            {
                if (layoutData.TryGetValue(child.Data.skillId, out var endData))
                {
                    // Vẽ đường thẳng hoặc bezier nối các node
                    Handles.color = Color.gray;
                    Handles.DrawAAPolyLine(3f, startData.Position, endData.Position);
                    Handles.color = Color.white;

                    // Đệ quy vẽ tiếp
                    DrawEdges(child);
                }
            }
        }

        private void DrawNodes(GameSkillTreeNode node)
        {
            if (node == null || layoutData == null) return;

            if (layoutData.TryGetValue(node.Data.skillId, out var data))
            {
                // Tính toán hình chữ nhật để vẽ node, căn giữa theo Position
                Rect nodeRect = new Rect(
                    data.Position.x - (NODE_WIDTH / 2),
                    data.Position.y - (NODE_HEIGHT / 2),
                    NODE_WIDTH,
                    NODE_HEIGHT
                );

                // Kiểm tra click vào Node để highlight ScriptableObject
                Event e = Event.current;
                if (e.type == EventType.MouseDown && e.button == 0 && nodeRect.Contains(e.mousePosition))
                {
                    Selection.activeObject = node.Data;
                    EditorGUIUtility.PingObject(node.Data);
                    e.Use();
                }

                // Vẽ nền Node
                GUIStyle nodeStyle = new GUIStyle("window");
                nodeStyle.padding = new RectOffset(5, 5, 5, 5);
                GUI.Box(nodeRect, "", nodeStyle);

                // 1. Vẽ Icon (Hỗ trợ Multiple Sprite / Cắt đúng vùng Texture)
                if (node.Data.skillIcon != null)
                {
                    Sprite sprite = node.Data.skillIcon;
                    Texture2D texture = sprite.texture;
                    Rect iconRect = new Rect(nodeRect.x + 5, nodeRect.y + 5, ICON_SIZE, ICON_SIZE);

                    // Tính toán tọa độ UV của Sprite trên Spritesheet
                    Rect texCoords = new Rect(
                        sprite.textureRect.x / texture.width,
                        sprite.textureRect.y / texture.height,
                        sprite.textureRect.width / texture.width,
                        sprite.textureRect.height / texture.height
                    );

                    GUI.DrawTextureWithTexCoords(iconRect, texture, texCoords);
                }

                // 2. Vẽ Name Skill (In đậm)
                Rect nameRect = new Rect(nodeRect.x + ICON_SIZE + 10, nodeRect.y + 4, NODE_WIDTH - ICON_SIZE - 15, 16);
                GUIStyle nameStyle = new GUIStyle(EditorStyles.boldLabel);
                nameStyle.fontSize = 11;
                string skillName = string.IsNullOrEmpty(node.Data.skillName) ? "Unnamed" : node.Data.skillName;
                GUI.Label(nameRect, skillName, nameStyle);

                // 3. Vẽ ID Skill (Nhỏ hơn, ở dưới Name)
                Rect idRect = new Rect(nodeRect.x + ICON_SIZE + 10, nodeRect.y + 20, NODE_WIDTH - ICON_SIZE - 15, 16);
                GUIStyle idStyle = new GUIStyle(EditorStyles.label);
                idStyle.fontSize = 9;
                GUI.Label(idRect, node.Data.skillId, idStyle);

                // Vẽ đường phân cách nhỏ
                Rect lineRect = new Rect(nodeRect.x + 5, nodeRect.y + 42, NODE_WIDTH - 10, 1);
                EditorGUI.DrawRect(lineRect, new Color(0.5f, 0.5f, 0.5f, 0.5f));

                // 4. Vẽ SP Require
                Rect spRect = new Rect(nodeRect.x + 5, nodeRect.y + 44, NODE_WIDTH - 10, 16);
                GUIStyle spStyle = new GUIStyle(EditorStyles.label);
                spStyle.fontSize = 10;
                spStyle.fontStyle = FontStyle.Bold;
                spStyle.alignment = TextAnchor.MiddleCenter;
                GUI.Label(spRect, $"SP: {node.Data.requiredSkillPoint}", spStyle);

                // Đệ quy vẽ các node con
                if (node.Children != null)
                {
                    foreach (var child in node.Children)
                    {
                        DrawNodes(child);
                    }
                }
            }
        }
    }
}
#endif