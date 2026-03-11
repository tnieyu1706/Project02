using UnityEngine;

namespace TnieYuPackage.DesignPatterns
{
    public abstract class BaseSubAsset : ScriptableObject
    {
        public abstract BaseParentAsset Parent { get; set; }
    }
}