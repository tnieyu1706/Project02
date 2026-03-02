using System;
using _Project.Scripts.TowerDefense.Gameplay;
using EditorAttributes;
using KBCore.Refs;
using TnieYuPackage.Core;
using UnityEngine;
using UnityEngine.Events;

namespace _Project.Scripts.TowerDefense.Entity
{
    public interface IEntityProperty : IDamageable
    {
        public float Def { get; }
        public float MoveSpeed { get; }
        public int EarningMoney { get; }
    }

    [RequireComponent(typeof(Animator))]
    public class EntityRuntime : MonoBehaviour, IEntityProperty
    {
        [SerializeField, Self] private Animator animator;

        private EntityPresetSo currentPreset;
        public Action OnDead;

        public UnityEvent<float> onHpChanged;

        [SerializeField, ReadOnly] private float hp;

        public float Hp
        {
            get => hp;
            set
            {
                if (value <= 0)
                {
                    OnEntityDead();
                    return;
                }

                hp = value;

                if (MaxHp != 0)
                {
                    onHpChanged?.Invoke(hp / MaxHp);
                }

                Debug.Log("HpChanged to :" + hp);
            }
        }

        [field: SerializeField] public float MaxHp { get; set; }
        [field: SerializeField] public float Def { get; set; }
        [field: SerializeField] public float MoveSpeed { get; set; }
        [field: SerializeField] public int EarningMoney { get; set; }

        private void Awake()
        {
            animator ??= GetComponent<Animator>();
        }

        private void OnDisable()
        {
            OnDead = null;
        }

        public void Setup(EntityPresetSo presetSo)
        {
            SetPreset(presetSo);
        }

        private void SetPreset(EntityPresetSo preset)
        {
            if (currentPreset is not null)
            {
                currentPreset.behaviourInstaller.Uninstall(gameObject);
            }

            Hp = preset.maxHp;
            MaxHp = preset.maxHp;
            Def = preset.def;
            MoveSpeed = preset.moveSpeed;
            EarningMoney = preset.earningMoney;
            //test
            // animator.runtimeAnimatorController = preset.animatorController;

            currentPreset = preset;
            currentPreset.behaviourInstaller.Install(gameObject);
        }

        private void OnEntityDead()
        {
            //Entity Death Handler
            animator.SetTrigger(TdConstant.TD_ENTITY_DEAD_PARAMETER);

            EventManager.Instance.RegistryDelay(HandleEntityDead, TdConstant.TD_ENTITY_DEAD_DELAY);
        }

        private void HandleEntityDead()
        {
            TdGameplayController.Instance.Money += EarningMoney;
            OnDead?.Invoke();
        }

        //test

        public EntityPresetSo setPreset;

        [Button]
        public void SetPresetButton()
        {
            SetPreset(setPreset);
        }

        [Button]
        public void TestChangeHealth(float changedHp)
        {
            Hp += changedHp;
        }
    }
}