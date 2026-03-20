using UnityEngine;

namespace TnieYuPackage.Core
{
    public static class TnieYuCoreInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize()
        {
            GameObject tnieyuCoreGo = new GameObject("TnieYuCore");
            tnieyuCoreGo.AddComponent<EventManager>();
            tnieyuCoreGo.AddComponent<InputEventManager>();
        }
    }
}