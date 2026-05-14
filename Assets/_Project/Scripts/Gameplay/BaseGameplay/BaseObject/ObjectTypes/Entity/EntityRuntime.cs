using KBCore.Refs;
using SoundSystem.Core;
using TnieYuPackage.Utils;
using UnityEngine;

namespace Game.BaseGameplay
{
    public interface IEntityRuntime : IEntityProperty, IObjectInteractable
    {
        Animator EntityAnimator { get; }

        void SetFaceDir(Vector2 faceDirNormalize);
    }

    public abstract class EntityRuntime<TPreset> : BaseObjectRuntime<TPreset>, IEntityRuntime
        where TPreset : EntityPresetSo
    {
        [SerializeField, Child] protected Physics2dTrigger physicsTrigger;
        [SerializeField] private SoundData getHitSoundData;
        [SerializeField] private SoundData deathSoundData;

        private SfxManager sfxManager;

        [field: SerializeField] public float MaxHp { get; private set; }
        [field: SerializeField] public ObservableValue<float> Hp { get; set; }
        [field: SerializeField] public float Defense { get; set; }

        public Animator EntityAnimator => this.animator;

        public void SetFaceDir(Vector2 faceDirNormalize)
        {
            EntityAnimator.SetFloat(BaseConstant.ENTITY_FACE_DIR_X, faceDirNormalize.x);
            EntityAnimator.SetFloat(BaseConstant.ENTITY_FACE_DIR_Y, faceDirNormalize.y);
        }

        protected virtual void Start()
        {
            sfxManager = BaseGameplayController.Instance.SfxManagerInject;
            Hp.OnValueChanged += OnEntityHealthChanged;
        }

        protected virtual void OnDestroy()
        {
            Hp.OnValueChanged -= OnEntityHealthChanged;
        }

        protected virtual void OnEntityHealthChanged(float changedHealth)
        {
            // animator.SetTrigger(BaseConstant.ENTITY_GET_HIT_TRIGGER);
            sfxManager?.PlayVfx(getHitSoundData).Forget();
            if (changedHealth <= 0)
            {
                //handle dead for entity
                sfxManager?.PlayVfx(deathSoundData).Forget();
                animator.SetTrigger(BaseConstant.ENTITY_DIED_TRIGGER);
                HandleEntityDead();
            }
        }

        protected abstract void HandleEntityDead();

        protected override void SetPresetProperties(TPreset preset)
        {
            base.SetPresetProperties(preset);

            physicsTrigger.SetLayer(preset.entityLayer);

            // init baseValue
            MaxHp = preset.maxHp; //temp test.
            Hp.Value = MaxHp;
            Defense = BaseGameplayCalculator.Instance.CalculateArmyDefense(preset.armyType, preset.def);
        }
    }
}