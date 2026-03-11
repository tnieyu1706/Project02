using System.Collections.Generic;
using UnityEngine;

namespace TnieYuPackage.Utils
{
    [DefaultExecutionOrder(-100)]
    public class ComponentRegister : ObjectRegister<Component>
    {
        protected override bool AddObject(Component o)
        {
            var registryActualType = typeof(Registry<>).MakeGenericType(o.GetType());
            var addMethod = registryActualType.GetMethod("TryAdd");

            if (addMethod is null)
            {
                Debug.LogError($"Failed to find TryAdd method for type {o.GetType()} in [Registry].");
                return false;
            }

            return (bool)addMethod.Invoke(null, new object[] { o });
        }

        protected override bool RemoveObject(Component o)
        {
            var registryActualType = typeof(Registry<>).MakeGenericType(o.GetType());
            var removeMethod = registryActualType.GetMethod("Remove");

            if (removeMethod is null)
            {
                Debug.LogError($"Failed to find Remove method for type {o.GetType()} in [Registry].");
                return false;
            }

            return (bool)removeMethod.Invoke(null, new object[] { o });
        }
    }

    public abstract class ObjectRegister<T> : MonoBehaviour where T : class
    {
        [SerializeField] private List<T> objects;

        void Awake()
        {
            if (objects is null || objects.Count == 0) return;

            objects.ForEach(o => AddObject(o));
        }

        protected virtual bool AddObject(T o)
        {
            return Registry<T>.TryAdd(o);
        }

        void OnDestroy()
        {
            if (objects is null || objects.Count == 0) return;

            objects.ForEach(o => RemoveObject(o));
        }

        protected virtual bool RemoveObject(T o)
        {
            return Registry<T>.Remove(o);
        }
    }
}