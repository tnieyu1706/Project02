using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TnieYuPackage.CustomAttributes;
using TnieYuPackage.Strategies.Projectile;
using UnityEngine;
using UnityEngine.Pool;

namespace Game.BaseGameplay.Strategies
{
    [CreateAssetMenu(fileName = "UndirectlyCauseDamage_Strategy",
        menuName = "Gameplay/Interact Strategies/Undirectly Cause Damage (Range)")]
    public class UndirectlyCauseDamageInstaller : CauseDamageInteractStrategyInstaller
    {
        [Header("Projectile Settings")] public float projectileSpeed = 6f;
        public Vector3 projectileOffset;

        [Tooltip("Prefab của viên đạn sẽ được bắn ra")]
        public Sprite projectileSprite;

        [Header("Trajectory Settings")]
        [SerializeReference]
        [AbstractSupport(
            abstractTypes: typeof(IProjectileTrajectoryStrategy)
        )]
        public IProjectileTrajectoryStrategy trajectoryStrategy = new LinearTrajectoryStrategy();

        public override IBaseObjectInteractStrategy CreateInteractStrategy() => new UndirectlyCauseDamageStrategy(this);
    }

    public class UndirectlyCauseDamageStrategy : CauseDamageInteractStrategy<UndirectlyCauseDamageInstaller>
    {
        private IObjectPool<GameObject> ProjectilePool =>
            BaseGameplayPrefabSpawnManager.Instance.PoolTrackers[PrefabType.BaseProjectile];

        public UndirectlyCauseDamageStrategy(UndirectlyCauseDamageInstaller installer) : base(installer)
        {
        }

        protected override async UniTask PerformAttackAction(IObjectInteractable target, CancellationToken token)
        {
            Vector3 startPos = OwnerRuntime.CurrentPosition + ActualInstaller.projectileOffset;
            Vector3 endPos = target.CurrentPosition; // Vị trí mục tiêu lúc bắt đầu bắn

            if (ActualInstaller.projectileSprite != null)
            {
                var projectileObj = ProjectilePool.Get();
                projectileObj.transform.position = startPos;

                if (projectileObj.TryGetComponent(out SpriteRenderer projectileRenderer))
                {
                    projectileRenderer.sprite = ActualInstaller.projectileSprite;
                }

                float distance = Vector3.Distance(startPos, endPos);
                float duration = distance / ActualInstaller.projectileSpeed;

                try
                {
                    if (ActualInstaller.trajectoryStrategy != null)
                    {
                        await ActualInstaller.trajectoryStrategy.ExecuteTrajectory(projectileObj.transform, startPos,
                            endPos, duration, token);
                    }
                }
                catch (OperationCanceledException)
                {
                }
                finally
                {
                    // Sau khi bay xong -> Trả về pool
                    if (BaseGameplayController.HasInstance)
                        ProjectilePool.Release(projectileObj);

                    // [BẢO VỆ POOLING 2]: Trong lúc đạn đang bay, Enemy có thể đã chết và hồi sinh ở chỗ khác.
                    // Kiểm tra vị trí hiện tại của Enemy so với điểm rơi của đạn (endPos).
                    // Nếu Enemy cách xa điểm đạn rớt (ví dụ > 2 units) thì đó chắc chắn là Enemy "mới" đã được reset từ Pool.
                    if (target != null && target.Hp.Value > 0)
                    {
                        float distFromImpact = Vector3.Distance(endPos, target.CurrentPosition);
                        // Giả định enemy có thể né tránh di chuyển nhẹ, nhưng nếu > interactRange thì chắc chắn là teleport/respawn
                        if (distFromImpact <= ActualInstaller.interactRange)
                        {
                            ApplyDamage(target);
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[{GetType().Name}] Chưa có Projectile Sprite! Chạy logic mô phỏng thời gian bay...");

                await UniTask.Delay((int)(1000f / ActualInstaller.projectileSpeed), cancellationToken: token);

                // Tương tự, giả lập cũng phải check lại khoảng cách phòng trường hợp bay/chờ xong thì enemy respawn
                if (target != null && target.Hp.Value > 0)
                {
                    float dist = Vector3.Distance(OwnerRuntime.CurrentPosition, target.CurrentPosition);
                    if (dist <= ActualInstaller.interactRange + 1f)
                    {
                        ApplyDamage(target);
                    }
                }
            }
        }
    }
}