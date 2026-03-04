using EditorAttributes;
using KBCore.Refs;
using TnieYuPackage.Core;
using UnityEngine;

namespace Game.Td
{
    public interface ISoldierProperty : IHealthProperty
    {
        public float Def { get; }
    }

    [RequireComponent(typeof(Animator))]
    public class SoldierRuntime : MonoBehaviour, ISoldierProperty
    {
        [SerializeField, ReadOnly] private SoldierPresetSo currentPreset;
        [SerializeField, Self] private Animator animator;

        private TowerControlBehaviour towerManager;

        #region Properties

        [SerializeField] private HealthProperty healthProperty;

        public HealthProperty HealthProperty => healthProperty;
        [field: SerializeField] public float Def { get; set; }

        #endregion

        void Start()
        {
            HealthProperty.OnDead += OnSoldierDead;
        }

        void OnDestroy()
        {
            HealthProperty.OnDead -= OnSoldierDead;
        }
        
        private void OnSoldierDead()
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

        private void SetPreset(SoldierPresetSo preset)
        {
            if (currentPreset is not null)
            {
                currentPreset.configurators.ForEach(conf => conf.UnConfig(gameObject));
                currentPreset.behaviourInstaller?.UnInstall(gameObject);
            }
            
            HealthProperty.Hp = preset.maxHp;
            HealthProperty.MaxHp = preset.maxHp;
            Def = preset.def;
            animator.runtimeAnimatorController = preset.animatorController;
            
            currentPreset = preset;
            currentPreset.behaviourInstaller?.Install(gameObject);
            currentPreset.configurators.ForEach(conf => conf.Config(gameObject));
        }
    }
}