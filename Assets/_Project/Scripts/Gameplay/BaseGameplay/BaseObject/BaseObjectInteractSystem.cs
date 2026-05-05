using System.Collections.Generic;
using System.Linq;
using TnieYuPackage.DesignPatterns;

namespace Game.BaseGameplay
{
    public class BaseObjectInteractSystem : Singleton<BaseObjectInteractSystem>
    {
        public readonly List<IBaseObjectRuntime> ObjectRuntimes = new();

        private void FixedUpdate()
        {
            foreach (var objRuntime in ObjectRuntimes.ToList())
            {
                foreach (var interact in objRuntime.InteractStrategyList.ToList().Where(interact => interact.CanUse))
                {
                    if (interact.CanUse && interact.TrackTarget(objRuntime.CurrentPosition, out var target))
                    {
                        interact.Interact(target);
                    }
                }
            }
        }
    }
}