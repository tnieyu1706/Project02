using UnityEngine;

namespace TnieYuPackage.DesignPatterns
{
    public abstract class SingletonBehavior<T> : MonoBehaviour
        where T : Component
    {
        [SerializeField] protected bool dontDestroyOnLoad;

        private static T instance;

        /// <summary>
        /// Only using for get available Instance
        /// Noted when SingletonBehavior in Disable.
        /// When game Stop/Close, it can stop Singleton before disable call
        /// so Instance will be null.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (instance != null) return instance;

                return instance = Object.FindFirstObjectByType<T>(FindObjectsInactive.Include);

                // auto-create singleton
                // GameObject go = new GameObject(typeof(T).Name + " (Singleton)");
                // instance = go.AddComponent<T>();
            }
        }

        protected virtual void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this as T;

            InitializeSingleton();

            if (dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);
        }

        protected virtual void InitializeSingleton()
        {
            gameObject.SetActive(true);
        }
    }
}