using Game.BaseGameplay.Strategies;
using Game.TowerDefense;
using TnieYuPackage.Core;
using TnieYuPackage.DesignPatterns;
using TnieYuPackage.GlobalExtensions;
using UnityEngine;

namespace Game.BaseGameplay.TowerEvents
{
    [CreateAssetMenu(fileName = "SetFlagTowerEvent", menuName = "Game/BaseGameplay/TowerEvent/SetFlagTowerEvent")]
    public class SetFlagTowerEvent : TowerEvent
    {
        private static TowerRuntime runtimeTemp;

        public override void OnCall(TowerRuntime towerRuntime)
        {
            runtimeTemp = towerRuntime;

            if (TdInteractSystem.Instance != null)
                TdInteractSystem.Instance.enabled = false;
            InputEventManager.Instance.enabled = true;
            InputEventManager.Instance.RegistryOnce(KeyCode.Mouse0, OnLeftMouseClick);
            InputEventManager.Instance.RegistryOnce(KeyCode.Mouse1, OnRightMouseClick);
        }

        private bool OnLeftMouseClick()
        {
            ExecuteInputEventHandler();
            InputEventManager.Instance.UnRegistryKey(KeyCode.Mouse1);
            var screenPoint = Input.mousePosition.With(z: 0);
            Vector2 worldPos = Registry<Camera>.GetFirst().ScreenToWorldPoint(screenPoint);
            foreach (var strategy in runtimeTemp.InteractStrategyList)
            {
                if (strategy is RallyTowerSpawnInteractStrategy rallyStrategy)
                {
                    rallyStrategy.SetFlagPosition(worldPos);
                }
            }

            return true;
        }

        private bool OnRightMouseClick()
        {
            ExecuteInputEventHandler();
            InputEventManager.Instance.UnRegistryKey(KeyCode.Mouse0);
            return true;
        }

        private void ExecuteInputEventHandler()
        {
            if (TdInteractSystem.Instance != null)
                TdInteractSystem.Instance.enabled = true;

            InputEventManager.Instance.enabled = false;
        }
    }
}