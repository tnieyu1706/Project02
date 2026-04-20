using EditorAttributes;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.Splines;

namespace Game.BaseGameplay
{
    [RequireComponent(typeof(Animator))]
    public class EnemyRuntime : EntityRuntime<EnemyPresetSo>
    {
        [Self] public SplineAnimate movementController;
        [ReadOnly] public Vector2 moveOffset = Vector2.zero;

        void Awake()
        {
            movementController ??= GetComponent<SplineAnimate>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            
            // ensure default animation is move-animation
            EntityAnimator.SetTrigger(BaseConstant.ENTITY_MOVE_TRIGGER);
        }

        protected override void Start()
        {
            base.Start();

            movementController.Updated += OnSplineAnimatePostProcessing;
            movementController.Completed += OnEntityMoveEnd;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            movementController.Updated -= OnSplineAnimatePostProcessing;
            movementController.Completed -= OnEntityMoveEnd;
        }

        protected override void HandleEntityDead()
        {
            var totalEarning =
                Mathf.RoundToInt(BaseGameplayCalculator.Instance.ReceivedMoneyScale * currentPreset.dropMoney);
            BaseGameplayController.Instance.money.Value += totalEarning;
            BaseGameplayPrefabSpawnManager.Instance.PoolTrackers[PrefabType.BaseEnemy].Release(gameObject);
        }

        #region SETUP METHODS

        public void Setup(EnemyPresetSo presetSo, SplineContainer pathMove, Vector2 offsetSource)
        {
            SetPreset(presetSo);
            SetupEnemyMovement(pathMove, offsetSource, currentPreset.moveSpeed);
        }

        private void SetupEnemyMovement(SplineContainer pathMover, Vector2 offsetSource, float movementSpeed)
        {
            movementController.Container = pathMover;
            movementController.MaxSpeed = movementSpeed;
            moveOffset = offsetSource;

            movementController.Restart(true);
        }

        #endregion

        private Vector3 prePos;

        private void OnSplineAnimatePostProcessing(Vector3 pos, Quaternion rot)
        {
            transform.position = (Vector2)transform.position + moveOffset; // new pos
            var direction = (Vector2)(transform.position - prePos);
            SetFaceDir(direction.normalized);
            prePos = transform.position; // old pos
        }

        private void OnEntityMoveEnd()
        {
            BaseGameplayController.Instance.baseHealth.Value -= currentPreset.baseCausingDmg;
            BaseGameplayPrefabSpawnManager.Instance.PoolTrackers[PrefabType.BaseEnemy].Release(gameObject);
        }
    }
}