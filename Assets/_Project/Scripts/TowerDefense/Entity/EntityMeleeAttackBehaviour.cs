using System;
using System.Collections;
using JetBrains.Annotations;
using KBCore.Refs;
using TnieYuPackage.CustomAttributes.Runtime;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Project.Scripts.TowerDefense.Entity
{
    [Serializable]
    public class EntityMeleeAttackBehaviourInstaller : IEntityBehaviourInstaller
    {
        public float attackDmg;
        public float attackSpeed = 1f;
        public float attackRange = 1f;
        [LayerMaskDropdown] public int layerTarget;

        public void Install(GameObject entity)
        {
            var behaviour = entity.AddComponent<EntityMeleeAttackBehaviour>();
            SetupBehaviour(behaviour);
        }

        private void SetupBehaviour(EntityMeleeAttackBehaviour behaviour)
        {
            behaviour.AttackDmg = attackDmg;
            behaviour.AttackSpeed = attackSpeed;
            behaviour.AttackRange = attackRange;
            behaviour.LayerTarget = layerTarget;
        }

        public void Uninstall(GameObject entity)
        {
            if (entity.TryGetComponent(out EntityMeleeAttackBehaviour behaviour))
            {
                Object.DestroyImmediate(behaviour);
            }
        }
    }

    public class EntityMeleeAttackBehaviour : MonoBehaviour
    {
        [Self] private Animator animator;
        private EnemyMover enemyMover;

        private GameObject currentTarget;
        private WaitForSeconds attackWait;
        private Coroutine attackCoroutine;
        private bool isAttacking;

        public float AttackDmg { get; set; }
        public float AttackSpeed { get; set; }
        public float AttackRange { get; set; }
        public int LayerTarget { get; set; }

        void Awake()
        {
            animator ??= GetComponent<Animator>();
            if (enemyMover is null)
            {
                gameObject.TryGetComponent(out enemyMover);
            }
        }

        void Start()
        {
            attackWait = new WaitForSeconds(TdConstant.ATTACK_CONSTANT / AttackSpeed);
        }

        private void BeginAttack()
        {
            enemyMover?.movementController.Play();

            animator.SetTrigger(TdConstant.TD_ENTITY_ATTACK_PARAMETER);
        }

        private void CloseAttack()
        {
            enemyMover?.movementController.Play();

            animator.SetTrigger(TdConstant.TD_ENTITY_MOVE_PARAMETER);
        }

        private void FixedUpdate()
        {
            GameObject trackingTarget = TrackTarget();

            if (isAttacking && trackingTarget == null)
            {
                CloseAttack();
                if (attackCoroutine is not null)
                {
                    StopAttack();
                }

                isAttacking = false;
                return;
            }

            if (trackingTarget != null && currentTarget != trackingTarget)
            {
                Debug.Log("Tracking different target");
                currentTarget = trackingTarget;

                if (attackCoroutine is not null)
                {
                    StopAttack();
                }

                attackCoroutine = StartCoroutine(ChannelingAttack(currentTarget));
                BeginAttack();
                isAttacking = true;
            }
        }

        private void StopAttack()
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        [CanBeNull]
        private GameObject TrackTarget()
        {
            return Physics2D.OverlapCircle(transform.position, AttackRange, LayerTarget)?.gameObject;
        }

        private IEnumerator ChannelingAttack(GameObject damagedTarget)
        {
            while (true)
            {
                if (damagedTarget.activeSelf && damagedTarget.TryGetComponent(out IDamageable damageable))
                {
                    Attack(damageable);
                }

                yield return attackWait;
            }
        }

        private void Attack(IDamageable damageable)
        {
            damageable.Hp -= AttackDmg;
        }
    }
}