using EditorAttributes;
using KBCore.Refs;
using TnieYuPackage.DesignPatterns.Patterns.Singleton;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Scripts.TowerDefense.Gameplay
{
    [DefaultExecutionOrder(-20)]
    [RequireComponent(typeof(Physics2DRaycaster))]
    public class PhysicsInteractController : SingletonBehavior<PhysicsInteractController>
    {
        [SerializeField, Self] private Physics2DRaycaster physics2DRaycaster;

        protected override void Awake()
        {
            base.Awake();

            physics2DRaycaster ??= GetComponent<Physics2DRaycaster>();
        }

        [Button]
        public void TurnOn()
        {
            physics2DRaycaster.enabled = true;
        }

        [Button]
        public void TurnOff()
        {
            physics2DRaycaster.enabled = false;
        }
    }
}