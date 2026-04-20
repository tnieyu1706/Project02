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
            if (ActualInstaller.projectileSprite != null)
            {
                var projectileObj = ProjectilePool.Get();

                Vector3 startPos = OwnerRuntime.CurrentPosition + ActualInstaller.projectileOffset;
                Vector3 endPos = target.CurrentPosition; // Vị trí mục tiêu lúc bắt đầu bắn

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
                    // Sau khi bay xong -> Trả về pool và gây sát thương
                    ProjectilePool.Release(projectileObj);
                    ApplyDamage(target);
                }
            }
            else
            {
                Debug.LogWarning($"[{GetType().Name}] Chưa có Projectile Sprite! Chạy logic mô phỏng thời gian bay...");
                // Giả lập delay đánh xa nếu không có hình ảnh đạn
                await UniTask.Delay((int)(1000f / ActualInstaller.projectileSpeed), cancellationToken: token);
                ApplyDamage(target);
            }
        }
    }
}