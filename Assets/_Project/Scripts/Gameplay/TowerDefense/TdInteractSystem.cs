using Game.BaseGameplay;
using TnieYuPackage.CustomAttributes;
using TnieYuPackage.DesignPatterns;
using TnieYuPackage.Utils;
using UnityEngine;

namespace Game.TowerDefense
{
    public class TdInteractSystem : SingletonBehavior<TdInteractSystem>
    {
        [SerializeField, LayerMaskDropdown] public int interactLayerMask;

        protected override void Awake()
        {
            dontDestroyOnLoad = false;
            base.Awake();
        }

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0)) return;

            var screenPoint = Input.mousePosition;
            var worldPos = Registry<Camera>.GetFirst().ScreenToWorldPoint(screenPoint);
            worldPos.z = 0;

            // raycast: get tower
            var ray = Physics2D.OverlapPoint(worldPos, interactLayerMask);

            if (ray == null) return;

            if (ray.TryGetComponent(out Physics2dTrigger physics2dTrigger)
                && physics2dTrigger.core.TryGetComponent(out TowerRuntime towerRuntime))
            {
                TdTowerContextGUI.Instance.Display(towerRuntime);
            }
        }
    }
}