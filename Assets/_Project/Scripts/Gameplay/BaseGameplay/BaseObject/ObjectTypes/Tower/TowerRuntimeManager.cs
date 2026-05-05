using System.Collections.Generic;
using TnieYuPackage.DesignPatterns;

namespace Game.BaseGameplay
{
    /// <summary>
    /// Singleton Object exists each Base Levels
    /// </summary>
    public class TowerRuntimeManager : Singleton<TowerRuntimeManager>
    {
        public List<TowerRuntime> towerRuntimeList = new List<TowerRuntime>();
    }
}