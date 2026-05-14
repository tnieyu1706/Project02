using Game.BaseGameplay.Strategies;
using Game.TowerDefense;
using Reflex.Extensions;
using SoundSystem.Core;
using TnieYuPackage.Handlers;
using TnieYuPackage.DesignPatterns;
using TnieYuPackage.GlobalExtensions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.BaseGameplay.TowerEvents
{
    [CreateAssetMenu(fileName = "SetFlagTowerEvent", menuName = "Game/BaseGameplay/TowerEvent/SetFlagTowerEvent")]
    public class SetFlagTowerEvent : TowerEvent
    {
        private static TowerRuntime runtimeTemp;

        [SerializeField] private SoundData commandSoundData;

        public override void OnCall(TowerRuntime towerRuntime)
        {
            runtimeTemp = towerRuntime;

            if (TdInteractSystem.HasInstance)
                TdInteractSystem.Instance.enabled = false;
            InputEventManager.Instance.enabled = true;
            InputEventManager.Instance.RegistryOnce(KeyCode.Mouse0, OnLeftMouseClick);
            InputEventManager.Instance.RegistryOnce(KeyCode.Mouse1, OnRightMouseClick);
        }

        private bool OnLeftMouseClick()
        {
            ExecuteInputEventHandler();

            // play sfx
            // Refactor: need refactor if can be.
            var container = SceneManager.GetActiveScene().GetSceneContainer();
            if (container != null)
            {
                var sfxManager = container.Resolve<SfxManager>();
                sfxManager.PlayVfx(commandSoundData).Forget();
            }

            // handler
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
            if (TdInteractSystem.HasInstance)
                TdInteractSystem.Instance.enabled = true;

            InputEventManager.Instance.enabled = false;
        }
    }
}