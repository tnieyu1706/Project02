using System;
using System.Reflection;
using InterfaceUtilities.Runtime;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InterfaceUtilities.Editor
{
    [CustomPropertyDrawer(typeof(InterfaceReference<,>), true)]
    [CustomPropertyDrawer(typeof(InterfaceReference<>), true)]
    public class InterfaceReferenceDrawer : PropertyDrawer
    {
        private SerializedProperty objProp;
        private (Type interfaceType, Type objectType) argsType;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            objProp = property.FindPropertyRelative("obj");
            argsType = GetArgsType(fieldInfo);

            float y = position.y;

            DrawObjectField(position, label, y);

            y += EditorGUIUtility.singleLineHeight;
            y += EditorGUIUtility.standardVerticalSpacing;

            DrawHintBox(position, y);

            EditorGUI.EndProperty();
        }

        private void DrawHintBox(Rect position, float y)
        {
            Rect helpRect = GetRectByHeight(position, y);

            EditorGUI.HelpBox(
                helpRect,
                $"Required: {argsType.interfaceType.Name}",
                MessageType.Info
            );
        }

        private void DrawObjectField(Rect position, GUIContent label, float y)
        {
            var fieldRect = GetRectByHeight(position, y);

            Object oldObj = objProp.objectReferenceValue;
            Object newObj = EditorGUI.ObjectField(
                fieldRect,
                label,
                oldObj,
                argsType.objectType,
                true
            );

            if (newObj == oldObj) return;

            SetNewObject(newObj);
        }

        private static Rect GetRectByHeight(Rect position, float y)
        {
            Rect fieldRect = new Rect(
                position.x,
                y,
                position.width,
                EditorGUIUtility.singleLineHeight
            );
            return fieldRect;
        }

        private void SetNewObject(Object newObj)
        {
            if (newObj == null)
            {
                objProp.objectReferenceValue = null;
            }

            else if (argsType.interfaceType.IsAssignableFrom(newObj.GetType()))
            {
                objProp.objectReferenceValue = newObj;
            }
            else
            {
                Debug.LogError(
                    $"Object [{newObj.name}] not implement interface [{argsType.interfaceType.Name}]"
                );
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return
                EditorGUIUtility.singleLineHeight + // ObjectField
                EditorGUIUtility.standardVerticalSpacing +
                EditorGUIUtility.singleLineHeight; // HelpBox
        }

        private (Type interfaceType, Type objectType) GetArgsType(FieldInfo fieldInfoSource)
        {
            Type fieldType = fieldInfoSource.FieldType;
            Type[] genericArgs = fieldType.GetGenericArguments();

            if (genericArgs.Length > 1)
                return (genericArgs[0], genericArgs[1]);

            return (genericArgs[0], typeof(Object));
        }
    }
}