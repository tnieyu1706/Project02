using System;
using UnityEngine;

namespace TnieYuPackage.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class AddressableKeyAttribute : PropertyAttribute
    {
        public readonly string GroupName;
        public readonly Type Type;
        public readonly string[] Labels;
        
        public AddressableKeyAttribute(string groupName = null, Type type = null, params string[] labels)
        {
            GroupName = groupName;
            Type = type;
            
            Labels = labels;
        }
    }
}