#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace TnieYuPackage.CustomAttributes
{
    [CustomPropertyDrawer(typeof(AbstractSupportAttribute))]
    public class AbstractSupportDrawer : PropertyDrawer
    {
        static readonly Dictionary<int, List<Type>> cache = new();

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                var tempRoot = new VisualElement();

                var field = new PropertyField(property);
                field.Bind(property.serializedObject);

                var warning = new HelpBox(
                    "[AbstractSupport] only works with [SerializeReference].",
                    HelpBoxMessageType.Warning);

                warning.style.marginTop = 2;

                tempRoot.Add(field);
                tempRoot.Add(warning);

                return tempRoot;
            }

            var attr = (AbstractSupportAttribute)attribute;

            Type[] abstractTypes = attr.AbstractTypes?.Length > 0
                ? attr.AbstractTypes
                : new[] { fieldInfo.FieldType };

            Type[] excludedTypes = attr.ExcludedTypes ?? Type.EmptyTypes;

            var implementations = GetImplementations(attr.Assembly, abstractTypes, excludedTypes);

            var root = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    marginBottom = 2
                }
            };

            var header = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    position = Position.Relative,
                    minHeight = 20
                }
            };

            var label = new Label(property.displayName)
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginRight = 4
                }
            };

            var fieldContainer = new VisualElement();

            root.Add(header);
            root.Add(fieldContainer);

            void BuildPropertyField()
            {
                fieldContainer.Clear();

                if (property.managedReferenceValue != null)
                {
                    var field = new PropertyField(property, "");
                    field.Bind(property.serializedObject);
                    fieldContainer.Add(field);
                }
            }

            void SetManagedReferenceForAll(Type type)
            {
                foreach (var target in property.serializedObject.targetObjects)
                {
                    var so = new SerializedObject(target);
                    var sp = so.FindProperty(property.propertyPath);

                    sp.managedReferenceValue = Activator.CreateInstance(type);
                    so.ApplyModifiedProperties();
                }
            }

            void ClearManagedReferenceForAll()
            {
                foreach (var target in property.serializedObject.targetObjects)
                {
                    var so = new SerializedObject(target);
                    var sp = so.FindProperty(property.propertyPath);

                    sp.managedReferenceValue = null;
                    so.ApplyModifiedProperties();
                }
            }

            void EnsureSerializableInstance()
            {
                if (property.propertyType != SerializedPropertyType.Generic)
                    return;

                if (property.managedReferenceValue != null)
                    return;

                var type = fieldInfo.FieldType;

                if (type.IsClass && !type.IsAbstract)
                {
                    foreach (var target in property.serializedObject.targetObjects)
                    {
                        var so = new SerializedObject(target);
                        var sp = so.FindProperty(property.propertyPath);

                        if (sp.managedReferenceValue == null)
                        {
                            var instance = Activator.CreateInstance(type);
                            sp.managedReferenceValue = instance;
                        }

                        so.ApplyModifiedProperties();
                    }
                }
            }

            void RefreshUI()
            {
                header.Clear();

                header.Add(label);

                if (property.propertyType == SerializedPropertyType.ManagedReference &&
                    property.managedReferenceValue != null)
                {
                    var typeName = property.managedReferenceFullTypename
                        .Split(' ')
                        .Last();

                    var shortTypeName = typeName
                        .Split('.')
                        .Last();

                    var typeLabel = new Label($"({shortTypeName})")
                    {
                        style =
                        {
                            color = new Color(0.6f, 0.8f, 0.9f),
                            unityFontStyleAndWeight = FontStyle.Italic,
                            marginLeft = 4,
                            marginRight = 28
                        }
                    };

                    header.Add(typeLabel);
                }

                Button actionButton;

                if (property.propertyType == SerializedPropertyType.ManagedReference)
                {
                    if (property.managedReferenceValue == null)
                    {
                        actionButton = new Button(() =>
                            {
                                var menu = new GenericMenu();

                                foreach (var type in implementations)
                                {
                                    menu.AddItem(new GUIContent(type.Name), false, () =>
                                    {
                                        SetManagedReferenceForAll(type);

                                        RefreshUI();
                                        EditorApplication.delayCall +=
                                            InternalEditorUtility.RepaintAllViews;
                                    });
                                }

                                menu.ShowAsContext();
                            })
                            { text = "+", tooltip = "Select type" };
                    }
                    else
                    {
                        actionButton = new Button(() =>
                            {
                                ClearManagedReferenceForAll();

                                RefreshUI();
                                EditorApplication.delayCall +=
                                    InternalEditorUtility.RepaintAllViews;
                            })
                            { text = "X", tooltip = "Reset value" };
                    }
                }
                else
                {
                    actionButton = new Button(() =>
                        {
                            EnsureSerializableInstance();
                            RefreshUI();
                        })
                        { text = "Create", tooltip = "Create instance" };
                }

                actionButton.style.position = Position.Absolute;
                actionButton.style.right = 0;
                actionButton.style.top = 0;
                actionButton.style.width = 24;
                actionButton.style.height = 18;

                header.Add(actionButton);

                BuildPropertyField();
            }

            RefreshUI();

            return root;
        }

        static List<Type> GetImplementations(Assembly assembly, Type[] abstractTypes, Type[] excludedTypes)
        {
            int key = HashCode.Combine(
                assembly?.GetHashCode() ?? 0,
                abstractTypes.Aggregate(0, (a, b) => a ^ b.GetHashCode()),
                excludedTypes.Aggregate(0, (a, b) => a ^ b.GetHashCode())
            );

            if (cache.TryGetValue(key, out var list))
                return list;

            list = new List<Type>();

            var assemblies = assembly != null
                ? new[] { assembly }
                : AppDomain.CurrentDomain.GetAssemblies();

            foreach (var asm in assemblies)
            {
                Type[] types;

                try
                {
                    types = asm.GetTypes();
                }
                catch
                {
                    continue;
                }

                foreach (var t in types)
                {
                    if (t.IsAbstract || t.IsInterface)
                        continue;

                    if (t.GetCustomAttribute<SerializableAttribute>() == null)
                        continue;

                    if (!abstractTypes.All(a => a.IsAssignableFrom(t)))
                        continue;

                    if (excludedTypes.Any(e => e.IsAssignableFrom(t)))
                        continue;

                    list.Add(t);
                }
            }

            list.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));

            cache[key] = list;
            return list;
        }
    }
}
#endif