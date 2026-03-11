using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using ZLinq;

namespace TnieYuPackage.CustomAttributes
{
    [CustomPropertyDrawer(typeof(AbstractSupportAttribute))]
    public class AbstractSupportDrawer : PropertyDrawer
    {
        private readonly List<Type> implementations = new();

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var attr = (AbstractSupportAttribute)attribute;
            Type[] abstractTypes = attr.AbstractTypes ?? new[] { fieldInfo.FieldType };
            Type[] excludedTypes = attr.ExcludedTypes ?? Type.EmptyTypes;

            var root = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    marginBottom = 2
                }
            };

            if (implementations.Count == 0)
            {
                if (attr.Assembly != null)
                    implementations.AddRange(GetImplementationsOfAssembly(attr.Assembly, abstractTypes, excludedTypes));
                else
                    CacheFullImplementations(abstractTypes, excludedTypes);
            }

            // ================= HEADER =================
            var header = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    position = Position.Relative, // 🔥 QUAN TRỌNG
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
                    var propertyField = new PropertyField(property, "");
                    propertyField.Bind(property.serializedObject);
                    fieldContainer.Add(propertyField);
                }
            }

            void RefreshUI()
            {
                header.Clear();

                // ===== Left side (flow layout)
                header.Add(label);

                if (property.managedReferenceValue != null)
                {
                    var typeName = property.managedReferenceFullTypename
                        .Split(' ')
                        .Last();

                    var typeLabel = new Label($"({typeName})")
                    {
                        style =
                        {
                            color = new Color(0.6f, 0.8f, 0.9f),
                            unityFontStyleAndWeight = FontStyle.Italic,
                            marginLeft = 4,
                            marginRight = 28 // 🔥 chừa chỗ cho button
                        }
                    };

                    header.Add(typeLabel);
                }

                // ===== Button overlay (absolute)
                Button actionButton;

                if (property.managedReferenceValue == null)
                {
                    actionButton = new Button(() =>
                    {
                        var menu = new GenericMenu();

                        foreach (var type in implementations)
                        {
                            menu.AddItem(new GUIContent(type.Name), false, () =>
                            {
                                var instance = Activator.CreateInstance(type);

                                property.serializedObject.Update();
                                property.managedReferenceValue = instance;
                                property.serializedObject.ApplyModifiedProperties();

                                RefreshUI();
                                EditorApplication.delayCall +=
                                    InternalEditorUtility.RepaintAllViews;
                            });
                        }

                        menu.ShowAsContext();
                    })
                    {
                        text = "+",
                        tooltip = "Select type"
                    };
                }
                else
                {
                    actionButton = new Button(() =>
                    {
                        property.serializedObject.Update();
                        property.managedReferenceValue = null;
                        property.serializedObject.ApplyModifiedProperties();

                        RefreshUI();
                        EditorApplication.delayCall +=
                            InternalEditorUtility.RepaintAllViews;
                    })
                    {
                        text = "X",
                        tooltip = "Reset value"
                    };
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

        private void CacheFullImplementations(Type[] abstractTypes, Type[] excludedTypes)
        {
            implementations.Clear();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                implementations.AddRange(GetImplementationsOfAssembly(assembly, abstractTypes, excludedTypes));
            }
        }

        private Type[] GetImplementationsOfAssembly(Assembly assembly, Type[] abstractTypes, Type[] excludedTypes)
        {
            try
            {
                return assembly.GetTypes()
                    .AsValueEnumerable()
                    .Where(t =>
                        !t.IsAbstract
                        && !t.IsInterface
                        && t.GetCustomAttribute<SerializableAttribute>() != null
                        && abstractTypes.All(abs => abs.IsAssignableFrom(t))
                        && !excludedTypes.Any(ex => ex.IsAssignableFrom(t)))
                    .OrderBy(t => t.Name)
                    .ToArray();
            }
            catch
            {
                return Array.Empty<Type>();
            }
        }
    }
}