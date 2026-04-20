using System;
using UnityEngine;

namespace TnieYuPackage.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class SubTypeNameSelectedAttribute : PropertyAttribute
    {
        public Type AssemblyType { get; }
        public Type ParentType { get; }

        public SubTypeNameSelectedAttribute(Type assemblyType, Type parentType)
        {
            AssemblyType = assemblyType;
            ParentType = parentType;
        }
    }
}