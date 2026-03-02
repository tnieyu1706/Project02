using UnityEngine;

namespace TnieYuPackage.Core
{
    public static class TnieYuCoreInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (EventManager.Instance != null) return;

            GameObject eventManagerGo = new GameObject("EventManager");
            eventManagerGo.AddComponent<EventManager>();
        }
    }
}