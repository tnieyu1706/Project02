using System;
using System.Collections.Generic;
using System.Linq;
using EditorAttributes;
using TnieYuPackage.Core;
using TnieYuPackage.CustomAttributes;
using TnieYuPackage.GlobalExtensions;
using UnityEngine;
using UnityEngine.Serialization;
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
        [TagDropdown] public string[] targetTags;

        public virtual void Install(GameObject tdObject)
        {
            var collider = tdObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = attackRange;

            var behaviour = tdObject.AddComponent<TdObjectAttackBehaviour>();
            behaviour.AttackDmg = attackDmg;
            behaviour.AttackDelay = attackDelay;
            behaviour.TrackingLayer = trackingLayer;
            behaviour.TargetTags = targetTags;
        }

        public virtual void UnInstall(GameObject tdObject)
        {
            if (tdObject.TryGetComponent(out CircleCollider2D collider))
            {
                Object.DestroyImmediate(collider);
            }

            if (tdObject.TryGetComponent(out TdObjectAttackBehaviour behaviour))
            {
                Object.DestroyImmediate(behaviour);
            }
        }
    }

    public class TdObjectAttackBehaviour : MonoBehaviour, ITdObjectBehaviour
    {
        private List<GameObject> trackingTargets = new();
        private List<GameObject> TrackingTargets => trackingTargets ??= new();

        private bool canAttack = true;

        #region PROPERTIES

        private bool isAttacking;
        public Action<GameObject> OnBeginAttack;
        public Action<GameObject> OnEndAttack;
        public Action<GameObject> OnEachAttack;

        [field: SerializeField] public float AttackDmg { get; set; }
        [field: SerializeField] public float AttackDelay { get; set; }
        public int TrackingLayer { get; set; }
        public string[] TargetTags { get; set; }

        #endregion

        void Update()
        {
            if (!canAttack) return;

            ValidateTargets();
            if (trackingTargets.Count <= 0) return;

            GameObject target = GetNestedTarget(trackingTargets);
            Attack(target);
        }

        private void Attack(GameObject target)
        {
            OnEachAttack?.Invoke(gameObject);
            HandleAttack(target);

            canAttack = false;
            EventManager.Instance.RegistryDelay(() => canAttack = true, AttackDelay);
        }

        protected virtual void HandleAttack(GameObject target)
        {
            if (!target.TryGetComponent(out IHealthProperty targetHealth) || targetHealth.HealthProperty.IsDead) return;

            targetHealth.Hp -= AttackDmg;
        }

        private GameObject GetNestedTarget(List<GameObject> targets)
        {
            GameObject result = targets.First();
            float minDistance = Vector2.Distance(transform.position, result.transform.position);

            if (targets.Count == 1)
            {
                return result;
            }

            for (var i = 1; i < targets.Count; i++)
            {
                var target = targets[i];
                var distance = Vector2.Distance(transform.position, target.transform.position);
                if (!(distance < minDistance)) continue;

                minDistance = distance;
                result = target;
            }

            return result;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (TrackingLayer.ContainLayer(other.gameObject.layer)
                && other.gameObject.TryGetComponent(out TdObjectAttackInteractable interactable)
                && TargetTags.Contains(interactable.core.gameObject.tag))
            {
                trackingTargets.Add(interactable.core);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (TrackingLayer.ContainLayer(other.gameObject.layer)
                && other.gameObject.TryGetComponent(out TdObjectAttackInteractable interactable)
                && TargetTags.Contains(interactable.core.gameObject.tag))
            {
                trackingTargets.Remove(interactable.core);
            }
        }

        private void ValidateTargets()
        {
            for (int i = TrackingTargets.Count - 1; i >= 0; i--)
            {
                if (!TrackingTargets[i].gameObject.activeSelf)
                {
                    TrackingTargets.RemoveAt(i);
                }
            }

            if (!isAttacking & trackingTargets.Count > 0)
            {
                OnBeginAttack?.Invoke(gameObject);
                isAttacking = true;
            }

            if (isAttacking & trackingTargets.Count == 0)
            {
                OnEndAttack?.Invoke(gameObject);
                isAttacking = false;
            }
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