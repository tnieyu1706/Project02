using System;
using System.Reflection;
using DictionaryUtilities.Runtime;
using UnityEditor;
using UnityEngine;

namespace DictionaryUtilities.Editor
{
    [CustomPropertyDrawer(typeof(BaseSerializableDictionary<,,>), true)]
    public class BaseSerializableDictionaryDrawer : PropertyDrawer
    {
        private const float ButtonHeight = 22f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // ───── Draw default property (Unity vẽ như cũ) ─────
            float defaultHeight = EditorGUI.GetPropertyHeight(property, label, true);

            Rect propertyRect = new Rect(
                position.x,
                position.y,
                position.width,
                defaultHeight
            );

            EditorGUI.PropertyField(propertyRect, property, label, true);

            // ───── Buttons ─────
            Rect buttonRect = new Rect(
                position.x,
                position.y + defaultHeight + EditorGUIUtility.standardVerticalSpacing,
                position.width,
                ButtonHeight
            );

            DrawButtons(buttonRect, property);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return
                EditorGUI.GetPropertyHeight(property, label, true) +
                EditorGUIUtility.standardVerticalSpacing +
                ButtonHeight;
        }

        private void DrawButtons(Rect rect, SerializedProperty property)
        {
            float half = rect.width * 0.5f;

            Rect rebuildRect = new Rect(rect.x, rect.y, half, rect.height);
            Rect reverseRect = new Rect(rect.x + half, rect.y, half, rect.height);

            if (GUI.Button(rebuildRect, "Rebuild"))
            {
                Invoke(property, "RebuildDictionary");
            }

            if (GUI.Button(reverseRect, "Reverse"))
            {
                Invoke(property, "ReverseData");
            }
        }

        private void Invoke(SerializedProperty property, string methodName)
        {
            object instance = GetTargetObjectOfProperty(property);
            if (instance == null) return;

            MethodInfo method = instance.GetType().GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );

            method?.Invoke(instance, null);

            EditorUtility.SetDirty(property.serializedObject.targetObject);
        }

        // Resolve actual object (supports nested fields)
        private object GetTargetObjectOfProperty(SerializedProperty prop)
        {
            object obj = prop.serializedObject.targetObject;
            string path = prop.propertyPath.Replace(".Array.data[", "[");

            foreach (string element in path.Split('.'))
            {
                if (element.Contains("["))
                {
                    string name = element[..element.IndexOf("[")];
                    int index = int.Parse(element[(element.IndexOf("[") + 1)..^1]);
                    obj = GetValue(obj, name, index);
                }
                else
                {
                    obj = GetValue(obj, element);
                }
            }

            return obj;
        }

        private object GetValue(object source, string name)
        {
            if (source == null) return null;

            var field = source.GetType().GetField(
                name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );

            return field?.GetValue(source);
        }

        private object GetValue(object source, string name, int index)
        {
            var enumerable = GetValue(source, name) as System.Collections.IEnumerable;
            if (enumerable == null) return null;

            var enumerator = enumerable.GetEnumerator();
            for (int i = 0; i <= index; i++)
                enumerator.MoveNext();

            return enumerator.Current;
        }
    }
}