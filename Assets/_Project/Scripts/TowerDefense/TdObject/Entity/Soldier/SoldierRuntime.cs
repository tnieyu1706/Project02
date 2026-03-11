using TnieYuPackage.Core;
using UnityEngine;

namespace Game.Td
{
    public interface ISoldierProperty : IHealthProperty
    {
        public float Def { get; }
    }

    [RequireComponent(typeof(Animator))]
    public class SoldierRuntime : EntityRuntime<SoldierPresetSo>, ISoldierProperty
    {
        private TowerControlBehaviour towerManager;

        protected override void OnEntityDead()
        {
            //Entity Death Handler
            animator.SetTrigger(TdConstant.TD_ENTITY_DEAD_PARAMETER);

            EventManager.Instance.RegistryDelay(HandleSoldierDead, TdConstant.TD_ENTITY_DEAD_DELAY);
        }

        private void HandleSoldierDead()
        {
            towerManager.Soldiers.Remove(gameObject);

            TdSpawnManager.Instance.RuntimePools[TdSpawnKey.Soldier].Release(gameObject);
        }

        public void Setup(SoldierPresetSo soldierPreset, TowerControlBehaviour manager)
        {
            SetPreset(soldierPreset);

            towerManager = manager;
        }
        
    }
}