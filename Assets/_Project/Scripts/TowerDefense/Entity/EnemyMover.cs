using _Project.Scripts.TowerDefense.Gameplay;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.Splines;

namespace _Project.Scripts.TowerDefense.Entity
{
    [RequireComponent(typeof(SplineAnimate))]
    [RequireComponent(typeof(EntityRuntime))]
    public class EnemyMover : MonoBehaviour
    {
        [Self] public SplineAnimate movementController;
        [Self] private EntityRuntime entityRuntime;

        void Awake()
        {
            movementController ??= GetComponent<SplineAnimate>();
            entityRuntime ??= GetComponent<EntityRuntime>();

            movementController.Completed += OnEntityMoveEnd;
        }

        public void Setup(SplineContainer pathMover)
        {
            movementController.Container = pathMover;
            movementController.MaxSpeed = entityRuntime.MoveSpeed;

            movementController.Restart(true);
        }

        private void OnEntityMoveEnd()
        {
            //cause damge
            TdGameplayController.Instance.Health -= 1;

            TdSpawnManager.Instance.RuntimePools[TdSpawnKey.Enemy].Release(gameObject);
        }
    }
}