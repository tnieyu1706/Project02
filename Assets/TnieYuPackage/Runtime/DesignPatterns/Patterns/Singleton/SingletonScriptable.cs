using System.Linq;
using UnityEngine;

namespace TnieYuPackage.DesignPatterns
{
    public abstract class SingletonScriptable<T> : ScriptableObject
        where T : ScriptableObject
    {
        protected static T instance;

        protected static bool HasInstance => instance != null;

        public static bool TryGetInstance(out T result)
        {
            result = Instance;
            return HasInstance;
        }

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.LoadAll<T>("").FirstOrDefault();
                    if (instance == null)
                        Debug.LogError($"[ScriptableSingleton] No instance of {typeof(T).Name} found in Resources.");
                }

                return instance;
            }
        }

        public virtual void OnEnable()
        {
            if (instance != null && instance != this)
            {
                Debug.LogWarning($"[ScriptableSingleton] Have multiple {typeof(T).Name} instances in active.");
            }

            instance = this as T;
        }
    }
}