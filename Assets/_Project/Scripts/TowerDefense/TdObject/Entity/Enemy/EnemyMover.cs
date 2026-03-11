using System;
using EditorAttributes;
using Game.Td;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.Splines;

namespace Game.Td
{
    [RequireComponent(typeof(SplineAnimate))]
    public class EnemyMover : MonoBehaviour
    {
        [Self] public SplineAnimate movementController;
        [ReadOnly] public Vector2 offset = Vector2.zero;

        void Awake()
        {
            movementController ??= GetComponent<SplineAnimate>();

            movementController.Updated += DeflectMovementEachMoving;
            movementController.Completed += OnEntityMoveEnd;
        }

        private void OnDestroy()
        {
            movementController.Updated -= DeflectMovementEachMoving;
            movementController.Completed -= OnEntityMoveEnd;
        }

        internal void Setup(SplineContainer pathMover, float movementSpeed)
        {
            movementController.Container = pathMover;
            movementController.MaxSpeed = movementSpeed;

            movementController.Restart(true);
        }

        private void DeflectMovementEachMoving(Vector3 pos, Quaternion rot)
        {
            transform.position = (Vector2)transform.position + offset;
        }

        private void OnEntityMoveEnd()
        {
            //cause damge
            TdGameplayController.Instance.Health -= 1;

            TdSpawnManager.Instance.RuntimePools[TdSpawnKey.Enemy].Release(gameObject);
        }
    }

    [Serializable]
    public class AttackMovementConfigurator : ITdObjectConfigurator
    {
        private void BeginAttack(GameObject enemy)
        {
            if (enemy.TryGetComponent(out EnemyMover mover))
            {
                mover.movementController.Pause();
            }

            if (enemy.TryGetComponent(out Animator animator))
            {
                animator.SetBool(TdConstant.TD_ENTITY_MOVE_PARAMETER, false);
            }
        }

        private void EndAttack(GameObject enemy)
        {
            if (enemy.TryGetComponent(out EnemyMover mover))
            {
                mover.movementController.Play();
            }

            if (enemy.TryGetComponent(out Animator animator))
            {
                animator.SetBool(TdConstant.TD_ENTITY_MOVE_PARAMETER, true);
            }
        }
        
        public void Config(GameObject tdObject)
        {
            if (!tdObject.TryGetComponent(out TdObjectAttackBehaviour attackBehaviour)) return;

            attackBehaviour.OnBeginAttack += BeginAttack;
            attackBehaviour.OnEndAttack += EndAttack;
        }

        public void UnConfig(GameObject tdObject)
        {
            if (!tdObject.TryGetComponent(out TdObjectAttackBehaviour attackBehaviour)) return;

            attackBehaviour.OnBeginAttack -= BeginAttack;
            attackBehaviour.OnEndAttack -= EndAttack;
        }
    }
}