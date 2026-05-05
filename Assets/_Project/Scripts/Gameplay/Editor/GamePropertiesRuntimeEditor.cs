using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Game.Global.Editor
{
    /// <summary>
    /// Custom Editor cho GamePropertiesRuntime để hiển thị các Auto-Properties 
    /// vốn không xuất hiện mặc định trong Inspector của Unity.
    /// </summary>
    [CustomEditor(typeof(GamePropertiesRuntime))]
    public class GamePropertiesRuntimeEditor : UnityEditor.Editor
    {
        private GUIStyle headerStyle;

        private void OnEnable()
        {
            // Khởi tạo style cho các Header bên trong khung
            headerStyle = null;
        }

        public override void OnInspectorGUI()
        {
            // Khởi tạo style một lần khi cần
            if (headerStyle == null)
            {
                headerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 12,
                    fontStyle = FontStyle.BoldAndItalic
                };
                headerStyle.normal.textColor = new Color(0.3f, 0.7f, 1f); // Màu xanh dương nhẹ
            }

            // 1. Vẽ các field mặc định (Dành cho các field đã [SerializeField] hoặc [field: SerializeField])
            // Điều này đảm bảo SkillPoints và filePath vẫn hoạt động bình thường.
            serializedObject.Update();
            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("RUNTIME PROPERTIES (READ-ONLY)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Dữ liệu dưới đây được lấy từ Properties và Dictionary lúc Runtime.", MessageType.None);

            GamePropertiesRuntime script = (GamePropertiesRuntime)target;

            // 2. Vẽ các nhóm thuộc tính dựa theo logic phân loại trong script của bạn
            
            DrawPropertySection("Building Gameplay", new[] { 
                "ResourceReceivedScaleDict", 
                "UnlockBuildingTypeDict" 
            }, script);

            DrawPropertySection("Base Gameplay", new[] { 
                "DamageScale", 
                "DefenseScale" 
            }, script);

            DrawPropertySection("Defense Properties", new[] { 
                "ReceiveMoneyScale", 
                "RefundScale", 
                "TowerTypeDamageScaleDict", 
                "UnlockTowerLevelDict", 
                "UnlockTowerTypeDict" 
            }, script);

            DrawPropertySection("Attack Properties", new[] { 
                "MaxEntityPerWave", 
                "ArmyHealthScaleDict", 
                "ArmySpeedScaleDict" 
            }, script);
            
            EditorGUILayout.Space(10);
        }

        /// <summary>
        /// Vẽ một nhóm thuộc tính có khung viền và padding
        /// </summary>
        private void DrawPropertySection(string header, string[] propertyNames, GamePropertiesRuntime targetScript)
        {
            // Thêm Margin bên ngoài
            EditorGUILayout.Space(4);
            
            // Bắt đầu vẽ Border bằng HelpBox (đã bao gồm padding nội bộ)
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Vẽ tiêu đề nhóm
            EditorGUILayout.LabelField(header.ToUpper(), headerStyle);
            EditorGUILayout.Space(2);
            
            EditorGUI.indentLevel++;

            foreach (var name in propertyNames)
            {
                PropertyInfo prop = targetScript.GetType().GetProperty(name);
                if (prop == null) continue;

                object value = prop.GetValue(targetScript);

                // Kiểm tra nếu là Dictionary
                if (value is IDictionary dict)
                {
                    DrawDictionaryUI(name, dict);
                }
                else
                {
                    // Hiển thị giá trị đơn lẻ ở dạng Read-only
                    GUI.enabled = false;
                    EditorGUILayout.TextField(ObjectNames.NicifyVariableName(name), value?.ToString() ?? "Null");
                    GUI.enabled = true;
                }
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.Space(4); // Padding dưới cùng của box
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Hiển thị nội dung Dictionary trong Inspector
        /// </summary>
        private void DrawDictionaryUI(string label, IDictionary dict)
        {
            EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(label), EditorStyles.miniBoldLabel);
            
            EditorGUI.indentLevel++;
            if (dict.Count == 0)
            {
                EditorGUILayout.LabelField("Empty", EditorStyles.miniLabel);
            }
            else
            {
                foreach (var key in dict.Keys)
                {
                    // Dùng LabelField để hiển thị cặp Key-Value
                    EditorGUILayout.LabelField(key.ToString(), dict[key]?.ToString());
                }
            }
            EditorGUI.indentLevel--;
            
            // Thêm một khoảng trống nhỏ sau mỗi dictionary để dễ nhìn
            EditorGUILayout.Space(2);
        }
    }
}