using System.Collections.Generic;
using UnityEngine;

namespace TnieYuPackage.Utils.Registry
{
    [DefaultExecutionOrder(-100)]
    public class ComponentRegister : MonoBehaviour
    {
        [SerializeField] private List<Component> components;

        void Awake()
        {
            if (components is null || components.Count == 0) return;

            components.ForEach(c => Registry<Component>.TryAdd(c));
        }

        void OnDestroy()
        {
            if (components is null || components.Count == 0) return;

            components.ForEach(c => Registry<Component>.Remove(c));
        }
    }
}