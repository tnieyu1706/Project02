using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using TnieYuPackage.Utils;

namespace Game.BaseGameplay.Strategies
{
    [CreateAssetMenu(fileName = "RallySpawn_Strategy", menuName = "Gameplay/Interact Strategies/Spawn/Rally Point")]
    public class RallyTowerSpawnInteractStrategyInstaller : TowerSpawnInteractStrategyInstaller
    {
        [Header("Rally Point Settings")] [Tooltip("Bán kính vòng tròn quanh điểm cờ để lính xếp đội hình")]
        public float rallyRadius = 0.5f;

        [Tooltip("Tốc độ di chuyển của lính tới điểm cờ")]
        public float moveSpeed = 3f;

        [Header("Path Finding Settings")] [Tooltip("Layer của đường đi (Path) để tháp tự động tìm vị trí cắm cờ")]
        public LayerMask pathLayerMask;

        public override IBaseObjectInteractStrategy CreateInteractStrategy() =>
            new RallyTowerSpawnInteractStrategy(this);
    }

    public class RallyTowerSpawnInteractStrategy : TowerSpawnInteractStrategy<RallyTowerSpawnInteractStrategyInstaller>
    {
        private Vector2 flagPosition;
        private CancellationTokenSource moveCts;

        public RallyTowerSpawnInteractStrategy(RallyTowerSpawnInteractStrategyInstaller installer) : base(installer)
        {
        }

        protected override void OnDefaultSetup()
        {
            moveCts ??= new CancellationTokenSource();
            // Lần đầu chạy sẽ tự tìm đường, sau đó được gán cứng theo SetFlagPosition
            SetFlagPosition(CalculateInitialFlagPosition(OwnerRuntime.CurrentPosition));

            base.OnDefaultSetup();
        }

        public override void OnDestroyBehaviour()
        {
            base.OnDestroyBehaviour();

            if (moveCts != null)
            {
                moveCts.Cancel();
                moveCts.Dispose();
                moveCts = null;
            }
        }

        private Vector3 CalculateInitialFlagPosition(Vector3 centerPosition)
        {
            float step = 0.5f;

            for (float r = step; r <= ActualInstaller.interactRange; r += step)
            {
                Collider2D pathCollider = Physics2D.OverlapCircle(centerPosition, r, ActualInstaller.pathLayerMask);

                if (pathCollider == null) continue;

                foreach (var dir in Vector2IntUtils.Get8DirectionalVectors())
                {
                    RaycastHit2D hit = Physics2D.Raycast(centerPosition, dir, ActualInstaller.interactRange,
                        ActualInstaller.pathLayerMask);

                    if (hit.collider != null) return hit.point;
                    return pathCollider.ClosestPoint(centerPosition);
                }
            }

            return centerPosition + (Vector3.down * 2f);
        }

        // validate & set flag position
        public void SetFlagPosition(Vector2 targetPosition)
        {
            Vector2 center = OwnerRuntime.CurrentPosition;
            Vector2 direction = targetPosition - center;

            // 1. Validate vị trí: Nếu vượt quá phạm vi maxSearchRadius thì lấy viền max
            if (direction.magnitude > ActualInstaller.interactRange)
            {
                targetPosition = center + direction.normalized * ActualInstaller.interactRange;
            }

            flagPosition = targetPosition;

            if (moveCts != null)
            {
                moveCts.Cancel();
                moveCts.Dispose();
                moveCts = new CancellationTokenSource();
            }

            // 2. Cập nhật vị trí và lệnh cho tất cả lính đang sống di chuyển tới điểm mới
            if (ActiveSoldiers != null)
            {
                for (int i = 0; i < ActiveSoldiers.Length; i++)
                {
                    if (ActiveSoldiers[i] != null)
                    {
                        DispatchSoldier(ActiveSoldiers[i], i);
                    }
                }
            }
        }

        protected override void DispatchSoldier(SoldierRuntime soldier, int soldierIndex)
        {
            float angleStep = 360f / ActualInstaller.maxSpawnQuantity;
            float currentAngle = angleStep * soldierIndex;

            Vector2 circleOffset = new Vector2(
                Mathf.Cos(currentAngle * Mathf.Deg2Rad),
                Mathf.Sin(currentAngle * Mathf.Deg2Rad)
            ) * ActualInstaller.rallyRadius;

            Vector3 targetRallyPoint = flagPosition + circleOffset;

            RallyAt(soldier, targetRallyPoint).Forget();
        }

        private async UniTaskVoid RallyAt(SoldierRuntime soldier, Vector3 targetPosition)
        {
            if (cts == null || cts.IsCancellationRequested) return;

            Vector2 dirVector = targetPosition - soldier.transform.position;
            float distance = dirVector.magnitude;
            float duration = distance / ActualInstaller.moveSpeed;

            soldier.EntityAnimator.SetTrigger(BaseConstant.ENTITY_MOVE_TRIGGER);
            try
            {
                soldier.SetFaceDir(dirVector.normalized);
                // Sử dụng LitMotion để Tween vị trí với UniTask + Cancellation
                await LMotion.Create(soldier.transform.position, targetPosition, duration)
                    .BindToPosition(soldier.transform)
                    .ToUniTask(cancellationToken: moveCts.Token);

                // TODO: Gọi hàm báo cho Soldier biết đã đến nơi (Ví dụ: Set trạng thái sang Idle/Attack)
            }
            catch (System.OperationCanceledException)
            {
                // Bỏ qua lỗi an toàn khi Strategy bị destroy và task bị hủy
            }
            finally
            {
                soldier.EntityAnimator.SetTrigger(BaseConstant.ENTITY_IDLE_TRIGGER);
            }
        }
    }
}