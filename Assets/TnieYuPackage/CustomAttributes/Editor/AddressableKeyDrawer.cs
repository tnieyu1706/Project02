#if UNITY_EDITOR
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using UnityEngine;
using ZLinq;

namespace TnieYuPackage.CustomAttributes
{
    [CustomPropertyDrawer(typeof(AddressableKeyAttribute))]
    public class AddressableKeyDrawer : PropertyDrawer
    {
        private string[] _keys;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // ensure string
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.HelpBox(position, $"{nameof(AddressableKeyAttribute)} only supports string.",
                    MessageType.Error);
                return;
            }

            var attr = (AddressableKeyAttribute)attribute;

            if (_keys == null)
                _keys = FetchKeys(attr);

            int index = Array.IndexOf(_keys, property.stringValue);
            if (index < 0) index = 0;

            int newIndex = EditorGUI.Popup(position, label.text, index, _keys);

            if (newIndex >= 0 && newIndex < _keys.Length)
                property.stringValue = _keys[newIndex];
        }

        private string[] FetchKeys(AddressableKeyAttribute attr)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;

            if (settings == null)
                return Array.Empty<string>();

            IEnumerable<AddressableAssetEntry> entries;

            // filter by group
            if (!string.IsNullOrEmpty(attr.GroupName))
            {
                entries = settings.groups
                    .Where(g => g.Name == attr.GroupName)
                    .SelectMany(group => group.entries);
            }
            else
            {
                entries = settings.groups
                    .Where(g => g != null)
                    .SelectMany(g => g.entries);
            }

            // filter by type
            if (attr.Type != null)
            {
                entries = entries.Where(e =>
                {
                    var path = AssetDatabase.GUIDToAssetPath(e.guid);
                    var assetType = AssetDatabase.GetMainAssetTypeAtPath(path);
                    return attr.Type.IsAssignableFrom(assetType);
                });
            }

            // filter by labels
            if (attr.Labels is { Length: > 0 })
            {
                entries = entries.Where(e =>
                    attr.Labels.All(label => e.labels.Contains(label)));
            }

            return entries
                .AsValueEnumerable()
                .Select(e => e.address)
                .Distinct()
                .OrderBy(e => e)
                .ToArray();
        }
    }
}
#endif