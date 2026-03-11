using System;
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
            var behaviour = tdObject.AddComponent<TdObjectRangeAttackBehaviour>();
            SetupBehaviour(behaviour);
        }

        protected override void SetupBehaviour(TdObjectAttackBehaviour behaviour)
        {
            base.SetupBehaviour(behaviour);

            if (behaviour is not TdObjectRangeAttackBehaviour rangeBehaviour) return;
            rangeBehaviour.ProjectileSprite = projectileSprite;
            rangeBehaviour.ProjectileDelay = projectileDelay;
        }

        public override void UnInstall(GameObject tdObject)
        {
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
                    CauseDamage(targetHealth);
                    ProjectilePool.Release(projectile);
                }, ProjectileDelay);
        }
    }
}