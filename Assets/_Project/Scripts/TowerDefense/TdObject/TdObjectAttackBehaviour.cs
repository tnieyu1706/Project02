using System;
using System.Collections;
using TnieYuPackage.CustomAttributes;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Td
{
    [Serializable]
    public class TdObjectAttackBehaviourInstaller : ITdObjectBehaviourInstaller
    {
        [LayerMaskDropdown] public int trackingLayer;
        public float attackDmg;
        public float attackRange;
        public float attackDelay;

        public virtual void Install(GameObject tdObject)
        {
            var behaviour = tdObject.AddComponent<TdObjectAttackBehaviour>();
            SetupBehaviour(behaviour);
        }

        protected virtual void SetupBehaviour(TdObjectAttackBehaviour behaviour)
        {
            behaviour.AttackDmg = attackDmg;
            behaviour.AttackDelay = attackDelay;
            behaviour.AttackWait = new WaitForSeconds(attackDelay);
            behaviour.AttackRange = attackRange;
            behaviour.TrackingLayer = trackingLayer;
        }

        public virtual void UnInstall(GameObject tdObject)
        {
            if (tdObject.TryGetComponent(out TdObjectAttackBehaviour behaviour))
            {
                Object.DestroyImmediate(behaviour);
            }
        }
    }

    public class TdObjectAttackBehaviour : MonoBehaviour, ITdObjectBehaviour
    {
        #region PROPERTIES

        private bool isAttacking;
        public Action<GameObject> OnBeginAttack;
        public Action<GameObject> OnEndAttack;
        public Action<GameObject> OnEachAttack;

        public WaitForSeconds AttackWait;
        [field: SerializeField] public float AttackDmg { get; set; }
        [field: SerializeField] public float AttackDelay { get; set; }
        [field: SerializeField] public float AttackRange { get; set; }
        public int TrackingLayer { get; set; }

        #endregion

        private void OnEnable()
        {
            StartCoroutine(AttackRoutine());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        IEnumerator AttackRoutine()
        {
            while (true)
            {
                //tracking: using raycast
                var targetCollider = Physics2D.OverlapCircle(transform.position, AttackRange, TrackingLayer);
                if (CanAttackTarget(targetCollider, out var interactable))
                {
                    if (!isAttacking)
                    {
                        OnBeginAttack?.Invoke(gameObject);
                        isAttacking = true;
                    }

                    OnEachAttack?.Invoke(gameObject);
                    HandleAttack(interactable.core);
                }
                else
                {
                    if (isAttacking)
                    {
                        OnEndAttack?.Invoke(gameObject);
                        isAttacking = false;
                    }
                }

                yield return AttackWait;
            }
        }

        private static bool CanAttackTarget(Collider2D targetCollider, out EntityAttackInteractable interactable)
        {
            interactable = null;
            return targetCollider != null
                   && targetCollider.gameObject.TryGetComponent(out interactable)
                   && interactable.core.TryGetComponent(out IHealthProperty healthProperty)
                   && !healthProperty.HealthProperty.IsDead;
        }

        protected virtual void HandleAttack(GameObject target)
        {
            if (!target.TryGetComponent(out IHealthProperty healthProperty) ||
                healthProperty.HealthProperty.IsDead) return;
            CauseDamage(healthProperty);
        }

        protected void CauseDamage(IHealthProperty targetHealthProperty)
        {
            targetHealthProperty.Hp -= AttackDmg;
        }
    }

    [Serializable]
    public class TdObjectEachAttackConfigurator : ITdObjectConfigurator
    {
        private void OnAttack(GameObject tdObject)
        {
            if (tdObject.TryGetComponent(out Animator animator))
            {
                animator.SetTrigger(TdConstant.TD_OBJECT_ATTACK_PARAMETER);
            }
        }

        public void Config(GameObject tdObject)
        {
            if (tdObject.TryGetComponent(out TdObjectAttackBehaviour attackBehaviour))
            {
                attackBehaviour.OnEachAttack += OnAttack;
            }
        }

        public void UnConfig(GameObject tdObject)
        {
            if (tdObject.TryGetComponent(out TdObjectAttackBehaviour attackBehaviour))
            {
                attackBehaviour.OnEachAttack -= OnAttack;
            }
        }
    }
}