using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace TnieYuPackage.Utils.Editor
{
    [CustomPropertyDrawer(typeof(ObservableValue<>), true)]
    public class ObservableValueDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // 1. Cấu hình thông số
            float padding = 6f;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            float lineHeight = EditorGUIUtility.singleLineHeight;

            // 2. Vẽ Border (Box nền)
            // Dùng GUI.skin.box hoặc "helpbox" để có giao diện chuẩn Unity
            GUI.Box(position, "", EditorStyles.helpBox);

            // 3. Tính toán lại vùng vẽ bên trong (đã trừ padding)
            Rect innerRect = new Rect(
                position.x + padding, 
                position.y + padding, 
                position.width - (padding * 2), 
                position.height - (padding * 2)
            );

            // 4. Vẽ trường 'Value'
            SerializedProperty valueProp = property.FindPropertyRelative("value");
            float valueHeight = EditorGUI.GetPropertyHeight(valueProp, label, true);
            Rect valueRect = new Rect(innerRect.x, innerRect.y, innerRect.width, valueHeight);
    
            EditorGUI.PropertyField(valueRect, valueProp, label, true);

            // 5. Vẽ nút 'Refresh' bên dưới Value
            Rect buttonRect = new Rect(
                innerRect.x, 
                valueRect.yMax + spacing, 
                innerRect.width, 
                lineHeight
            );

            if (GUI.Button(buttonRect, "Refresh", EditorStyles.miniButton))
            {
                object target = GetTargetObjectOfProperty(property);
                if (target != null)
                {
                    var method = target.GetType().GetMethod("Refresh");
                    method?.Invoke(target, null);
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty valueProp = property.FindPropertyRelative("value");
            float valueHeight = EditorGUI.GetPropertyHeight(valueProp, label, true);
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            float padding = 6f; // Khoảng cách từ border đến nội dung

            // Tổng: padding trên + chiều cao Value + spacing + chiều cao Button + padding dưới
            return padding + valueHeight + spacing + lineHeight + padding;
        }

        // Helper to get the actual object from a SerializedProperty
        public static object GetTargetObjectOfProperty(SerializedProperty prop)
        {
            if (prop == null) return null;
            string path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    int openIdx = element.IndexOf('[');
                    int closeIdx = element.IndexOf(']');
                    var elementName = element.Substring(0, openIdx);
                    var index = Convert.ToInt32(element.Substring(openIdx + 1, closeIdx - openIdx - 1));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }
            return obj;
        }

        private static object GetValue_Imp(object source, string name)
        {
            if (source == null) return null;
            var type = source.GetType();
            FieldInfo f = null;
            while (type != null)
            {
                f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null) break;
                type = type.BaseType;
            }
            return f?.GetValue(source);
        }

        private static object GetValue_Imp(object source, string name, int index)
        {
            var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
            if (enumerable == null) return null;
            var enm = enumerable.GetEnumerator();
            try
            {
                for (int i = 0; i <= index; i++)
                {
                    if (!enm.MoveNext()) return null;
                }
                return enm.Current;
            }
            finally
            {
                if (enm is IDisposable disp)
                    disp.Dispose();
            }
        }
    }
}
