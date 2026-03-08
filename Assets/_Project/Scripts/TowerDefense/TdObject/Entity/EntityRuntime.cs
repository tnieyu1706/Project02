using KBCore.Refs;
using UnityEngine;

namespace Game.Td
{
    public abstract class EntityRuntime<TPreset> : TdObjectRuntime<TPreset>, IHealthProperty
        where TPreset : EntityPresetSo
    {
        [SerializeField, Child] protected EntityAttackInteractable attackInteractable;

        [SerializeField] private HealthProperty healthProperty;
        public HealthProperty HealthProperty => healthProperty;
        [field: SerializeField] public float Def { get; set; }

        void Start()
        {
            HealthProperty.OnDead += OnEntityDead;
        }

        void OnDestroy()
        {
            HealthProperty.OnDead -= OnEntityDead;
        }

        protected abstract void OnEntityDead();

        protected override void SetPresetProperties(TPreset preset)
        {
            base.SetPresetProperties(preset);
            
            attackInteractable.gameObject.layer = LayerMask.NameToLayer(preset.entityLayer);
            
            HealthProperty.IsDead = false;
            HealthProperty.Hp = preset.maxHp;
            HealthProperty.MaxHp = preset.maxHp;
            Def = preset.def;
            
            animator.SetBool(TdConstant.TD_ENTITY_MOVE_PARAMETER, true);
        }
    }
}