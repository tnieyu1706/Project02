using UnityEngine;

namespace TnieYuPackage.DesignPatterns
{
    public enum RegistryType
    {
        OnStart,
        OnEnable
    }

    public abstract class RegistryBehaviour<T> : MonoBehaviour
        where T : UnityEngine.Component
    {
        [SerializeField] private RegistryType registryType;

        protected virtual void Start()
        {
            if (registryType == RegistryType.OnStart)
            {
                Registry<T>.TryAdd(this as T);
            }
        }

        protected virtual void OnEnable()
        {
            if (registryType == RegistryType.OnEnable)
            {
                Registry<T>.TryAdd(this as T);
            }
        }

        protected virtual void OnDisable()
        {
            if (registryType == RegistryType.OnEnable)
            {
                Registry<T>.Remove(this as T);
            }
        }

        protected virtual void OnDestroy()
        {
            if (registryType == RegistryType.OnEnable)
            {
                Registry<T>.Remove(this as T);
            }
        }
    }
}