#if UNITY_EDITOR
using System;
using System.Linq;
using TnieYuPackage.GlobalExtensions;
using UnityEditor;
using UnityEngine;

// 👈 để dùng GetShortAssemblyName

namespace TnieYuPackage.CustomAttributes
{
    [CustomPropertyDrawer(typeof(SubTypeNameSelectedAttribute))]
    public class SubTypeNameSelectedDrawer : PropertyDrawer
    {
        private Type[] _cachedTypes;
        private string[] _displayNames;

        private void Init(SubTypeNameSelectedAttribute attr)
        {
            if (_cachedTypes != null) return;

            var parentType = attr.ParentType;
            var assembly = attr.AssemblyType != null
                ? attr.AssemblyType.Assembly
                : null;

            var types = assembly != null
                ? assembly.GetTypes()
                : AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a =>
                    {
                        try { return a.GetTypes(); }
                        catch { return Type.EmptyTypes; } // tránh crash reflection
                    });

            _cachedTypes = types
                .Where(t =>
                    t != null &&
                    !t.IsAbstract &&
                    !t.IsInterface &&
                    parentType.IsAssignableFrom(t))
                .OrderBy(t => t.Name)
                .ToArray();

            _displayNames = _cachedTypes
                .Select(t => t.Name)
                .ToArray();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = (SubTypeNameSelectedAttribute)attribute;
            Init(attr);

            EditorGUI.BeginProperty(position, label, property);

            // ❗ chỉ support string
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "Use string field");
                EditorGUI.EndProperty();
                return;
            }

            string currentValue = property.stringValue;

            int currentIndex = -1;

            if (!string.IsNullOrEmpty(currentValue))
            {
                for (int i = 0; i < _cachedTypes.Length; i++)
                {
                    if (_cachedTypes[i].GetShortAssemblyName() == currentValue)
                    {
                        currentIndex = i;
                        break;
                    }
                }
            }

            int newIndex = EditorGUI.Popup(position, label.text, currentIndex, _displayNames);

            if (newIndex >= 0 && newIndex < _cachedTypes.Length)
            {
                var selectedType = _cachedTypes[newIndex];
                property.stringValue = selectedType.GetShortAssemblyName();
            }

            EditorGUI.EndProperty();
        }
    }
}
#endif