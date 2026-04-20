using System.Collections.Generic;
using System.Linq;
using TnieYuPackage.DesignPatterns;

namespace Game.BaseGameplay
{
    public class BaseObjectInteractSystem : SingletonBehavior<BaseObjectInteractSystem>
    {
        public readonly List<IBaseObjectRuntime> ObjectRuntimes = new();

        private void FixedUpdate()
        {
            foreach (var objRuntime in ObjectRuntimes.ToList())
            {
                foreach (var interact in objRuntime.InteractStrategyList.ToList().Where(interact => interact.CanUse))
                {
                    if (interact.TrackTarget(objRuntime.CurrentPosition, out var target))
                    {
                        interact.Interact(target);
                    }
                }
            }
        }
    }
}