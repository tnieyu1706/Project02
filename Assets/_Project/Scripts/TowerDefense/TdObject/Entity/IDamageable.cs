using System;
using EditorAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Td
{
    public interface IDamageable
    {
        public float Hp { get; set; }
    }

    [Serializable]
    public class HealthProperty
    {
        /// <summary>
        /// Is property ensure for dead & multi-call
        /// </summary>
        public bool IsDead { get; set; }
        
        //temp using Class. ===> Action only.
        public UnityEvent<float> onHpChanged;
        public event Action OnDead;
        [SerializeField, ReadOnly] private float hp;

        public float Hp
        {
            get => hp;
            set
            {
                if (!IsDead && value <= 0)
                {
                    // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                    OnDead?.Invoke();
                    IsDead = true;
                }

                hp = value;

                if (MaxHp != 0)
                {
                    onHpChanged?.Invoke(hp / MaxHp);
                }
            }
        }
        
        [field: SerializeField] public float MaxHp { get; set; }
    }

    public interface IHealthProperty : IDamageable
    {
        public HealthProperty HealthProperty { get; }

        float IDamageable.Hp
        {
            get => HealthProperty.Hp;
            set => HealthProperty.Hp = value;
        }
    }
}