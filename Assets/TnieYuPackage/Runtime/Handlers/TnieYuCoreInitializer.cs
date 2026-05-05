using UnityEngine;

namespace TnieYuPackage.Handlers
{
    public static class TnieYuCoreInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize()
        {
            Debug.Log("TnieYuCoreInitializer: Initializing TnieYuCore...");
            GameObject tnieyuCoreGo = new GameObject("TnieYuCore");
            tnieyuCoreGo.AddComponent<EventManager>();
            tnieyuCoreGo.AddComponent<InputEventManager>();
        }
    }
}