using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.TowerDefense.Entity;
using _Project.Scripts.TowerDefense.Gameplay;
using KBCore.Refs;
using PrimeTween;
using TnieYuPackage.Core;
using TnieYuPackage.CustomAttributes.Runtime;
using TnieYuPackage.GlobalExtensions;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace _Project.Scripts.TowerDefense.Tower
{
    [Serializable]
    public class TowerAttackBehaviourInstaller : ITowerBehaviourInstaller
    {
        public Sprite projectileSprite;
        public float projectileSpeed;
        public float projectileDamage;
        public float projectileRange;
        [LayerMaskDropdown] public int targetLayer;

        public void Install(GameObject towerObject)
        {
            var behaviour = towerObject.AddComponent<TowerAttackBehaviour>();

            SetupBehaviour(behaviour);

            var collider = towerObject.AddComponent<CircleCollider2D>();

            SetupCollider(collider);
        }

        private void SetupCollider(CircleCollider2D collider)
        {
            collider.isTrigger = true;
            collider.radius = projectileRange;
        }

        private void SetupBehaviour(TowerAttackBehaviour behaviour)
        {
            behaviour.ProjectileSprite = projectileSprite;
            behaviour.ProjectileSpeed = projectileSpeed;
            behaviour.ProjectileDamage = projectileDamage;
            behaviour.LayerTarget = targetLayer;
        }

        public void UnInstall(GameObject towerObject)
        {
            if (towerObject.TryGetComponent(out TowerAttackBehaviour behaviour))
            {
                Object.DestroyImmediate(behaviour);
            }

            if (towerObject.TryGetComponent(out CircleCollider2D collider))
            {
                Object.DestroyImmediate(collider);
            }
        }
    }

    public class TowerAttackBehaviour : MonoBehaviour, ITowerBehaviour
    {
        [Self] private Animator animator;

        private List<GameObject> enemies = new();

        public Sprite ProjectileSprite { get; set; }
        public float ProjectileSpeed { get; set; }
        public float ProjectileDamage { get; set; }
        public int LayerTarget { get; set; }

        private ObjectPool<GameObject> ProjectilePool => TdSpawnManager.Instance.RuntimePools[TdSpawnKey.Projectile];

        private WaitForSeconds attackWait;

        void Awake()
        {
            animator ??= GetComponent<Animator>();
        }

        void Start()
        {
            attackWait = new WaitForSeconds(TdConstant.ATTACK_CONSTANT / ProjectileSpeed);
            StartCoroutine(ChannelingAttack());
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        private void BeginAttack()
        {
            animator.SetTrigger(TdConstant.TD_TOWER_ATTACK_PARAMETER);
        }

        private void CloseAttack()
        {
            animator.SetTrigger(TdConstant.TD_TOWER_IDLE_PARAMETER);
        }

        private IEnumerator ChannelingAttack()
        {
            while (true)
            {
                ValidateTargets();

                if (enemies.Count > 0)
                {
                    GameObject target = FilterNestedEnemy(transform, enemies);

                    var projectile = ProjectilePool.Get();
                    Tween.LocalPosition(
                        projectile.transform,
                        transform.position,
                        target.transform.position,
                        TdConstant.TD_PROJECTILE_ATTACK_DELAY
                    );
                    //calling delay
                    EventManager.Instance.RegistryDelay(() => CauseDamage(target, projectile),
                        TdConstant.TD_PROJECTILE_ATTACK_DELAY);
                }

                yield return attackWait;
            }
            // ReSharper disable once IteratorNeverReturns
        }

        private void ValidateTargets()
        {
            enemies = enemies.Where(e => !e.activeSelf).ToList();

            if (enemies.Count == 0)
            {
                CloseAttack();
            }
        }

        private void CauseDamage(GameObject target, GameObject projectile)
        {
            if (target.TryGetComponent(out IDamageable damageable))
            {
                damageable.Hp -= ProjectileDamage;
            }

            ProjectilePool.Release(projectile);
        }

        private GameObject FilterNestedEnemy(Transform origin, List<GameObject> list)
        {
            float nestedDistance = 0;
            GameObject nestedGameObject = list[0];

            foreach (var e in list)
            {
                float distance = Vector3.Distance(origin.position, e.transform.position);
                if (distance < nestedDistance)
                {
                    nestedDistance = distance;
                    nestedGameObject = e;
                }
            }

            return nestedGameObject;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (LayerTarget.ContainLayer(other.gameObject.layer))
                enemies.Add(other.gameObject);

            BeginAttack();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (LayerTarget.ContainLayer(other.gameObject.layer))
                enemies.Remove(other.gameObject);

            if (enemies.Count == 0)
            {
                CloseAttack();
            }
        }
    }
}