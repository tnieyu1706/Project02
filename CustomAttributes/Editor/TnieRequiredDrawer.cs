using TnieYuPackage.CustomAttributes.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace TnieYuPackage.CustomAttributes.Editor
{
    [CustomPropertyDrawer(typeof(TnieRequiredAttribute))]
    public class TnieRequiredDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column
                }
            };

            // Base ObjectField
            var field = new PropertyField(property, property.displayName);

            // HelpBox error
            var help = new HelpBox("Missing Reference!", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None
                }
            };

            root.Add(field);
            root.Add(help);

            // Registry Callback
            field.RegisterValueChangeCallback(evt =>
            {
                Validate(property, field, help);
            });

            // First Validate
            Validate(property, field, help);

            return root;
        }

        private void Validate(SerializedProperty property, VisualElement field, VisualElement help)
        {
            bool missing = property.propertyType == SerializedPropertyType.ObjectReference &&
                           property.objectReferenceValue == null;

            help.style.display = missing ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
