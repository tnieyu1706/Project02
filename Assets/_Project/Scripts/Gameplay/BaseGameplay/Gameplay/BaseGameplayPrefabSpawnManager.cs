using TnieYuPackage.Utils;
using UnityEngine;

namespace Game.BaseGameplay
{
    public enum PrefabType
    {
        BaseEnemy,
        BaseSoldier,
        BaseProjectile
    }

    [DefaultExecutionOrder(-20)]
    public class BaseGameplayPrefabSpawnManager : PrefabSpawnManager<PrefabType, BaseGameplayPrefabSpawnManager>
    {
    }
}