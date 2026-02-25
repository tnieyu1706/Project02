using System;
using UnityEngine;

namespace TnieYuPackage.CustomAttributes.Runtime
{
    [AttributeUsage(AttributeTargets.Field)]
    public class TnieRequiredAttribute : PropertyAttribute
    {
    }
}