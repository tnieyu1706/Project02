using System;
using System.Reflection;
using UnityEngine;

namespace TnieYuPackage.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class AbstractSupportAttribute : PropertyAttribute
    {
        public Assembly Assembly { get; }
        public Type[] AbstractTypes { get; }
        public Type[] ExcludedTypes { get; }

        public AbstractSupportAttribute(
            Type classAssembly = null,
            Type[] excludedTypes = null,
            params Type[] abstractTypes
        )
        {
            Assembly = classAssembly?.Assembly;
            AbstractTypes = abstractTypes;
            ExcludedTypes = excludedTypes;
        }
    }
}