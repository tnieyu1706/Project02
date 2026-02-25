using System;
using UnityEngine;

namespace TnieYuPackage.CustomAttributes.Runtime
{
    [AttributeUsage(AttributeTargets.Field)]
    public class AbstractSupportAttribute : PropertyAttribute
    {
        public Type AbstractType { get; }

        public AbstractSupportAttribute(Type abstractType = null)
        {
            AbstractType = abstractType;
        }
    }
}