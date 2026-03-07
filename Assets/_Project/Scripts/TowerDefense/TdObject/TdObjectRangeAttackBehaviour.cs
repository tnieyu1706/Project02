using System;
using Game.Td;
using PrimeTween;
using TnieYuPackage.Core;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace Game.Td
{
    [Serializable]
    public class TdObjectRangeAttackBehaviourInstaller : TdObjectAttackBehaviourInstaller
    {
        public Sprite projectileSprite;
        public float projectileDelay;

        public override void Install(GameObject tdObject)
        {
            var collider = tdObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = attackRange;

            var behaviour = tdObject.AddComponent<TdObjectRangeAttackBehaviour>();
            behaviour.AttackDmg = attackDmg;
            behaviour.AttackDelay = attackDelay;
            behaviour.TrackingLayer = trackingLayer;
            behaviour.TargetTags = targetTags;
            behaviour.ProjectileSprite = projectileSprite;
            behaviour.ProjectileDelay = projectileDelay;
        }

        public override void UnInstall(GameObject tdObject)
        {
            if (tdObject.TryGetComponent(out CircleCollider2D collider))
            {
                Object.DestroyImmediate(collider);
            }

            if (tdObject.TryGetComponent(out TdObjectRangeAttackBehaviour behaviour))
            {
                Object.DestroyImmediate(behaviour);
            }
        }
    }

    public class TdObjectRangeAttackBehaviour : TdObjectAttackBehaviour
    {
        public Sprite ProjectileSprite { get; set; }
        [field: SerializeField] public float ProjectileDelay { get; set; }

        private ObjectPool<GameObject> ProjectilePool => TdSpawnManager.Instance.RuntimePools[TdSpawnKey.Projectile];

        protected override void HandleAttack(GameObject target)
        {
            if (!target.TryGetComponent(out IHealthProperty targetHealth) || targetHealth.HealthProperty.IsDead) return;

            GameObject projectile = ProjectilePool.Get();
            //setup
            if (projectile.TryGetComponent(out SpriteRenderer spriteRenderer))
            {
                spriteRenderer.sprite = ProjectileSprite;
            }

            Tween.LocalPosition(
                projectile.transform,
                transform.position,
                target.transform.position,
                ProjectileDelay
            );

            EventManager.Instance.RegistryDelay(
                () =>
                {
                    targetHealth.Hp -= AttackDmg;
                    ProjectilePool.Release(projectile);
                }, ProjectileDelay);
        }
    }
}