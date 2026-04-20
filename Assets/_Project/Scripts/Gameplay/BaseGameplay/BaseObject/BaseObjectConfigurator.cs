using UnityEngine;

namespace Game.BaseGameplay
{
    public abstract class BaseObjectConfigurator : ScriptableObject
    {
        public abstract void Configure(IBaseObjectRuntime runtime);
        public abstract void UnConfigure(IBaseObjectRuntime runtime);
    }
}