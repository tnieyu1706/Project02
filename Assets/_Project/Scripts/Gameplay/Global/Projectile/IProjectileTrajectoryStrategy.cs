using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TnieYuPackage.Strategies.Projectile
{
    public interface IProjectileTrajectoryStrategy
    {
        UniTask ExecuteTrajectory(Transform projectileTransform, Vector3 startPos, Vector3 endPos, float duration,
            CancellationToken cancellationToken);
    }
}