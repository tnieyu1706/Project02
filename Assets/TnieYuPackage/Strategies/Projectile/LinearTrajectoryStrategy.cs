using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using TnieYuPackage.GlobalExtensions;
using UnityEngine;

namespace TnieYuPackage.Strategies.Projectile
{
    [Serializable]
    public class LinearTrajectoryStrategy : IProjectileTrajectoryStrategy
    {
        [Tooltip("Tự động xoay đạn theo hướng bay")]
        public bool autoRotate = true;

        public async UniTask ExecuteTrajectory(Transform projectileTransform, Vector3 startPos, Vector3 endPos,
            float duration, CancellationToken cancellationToken)
        {
            if (autoRotate)
            {
                projectileTransform.RotateTowards2D(endPos - startPos);
            }

            await LMotion.Create(startPos, endPos, duration)
                .BindToPosition(projectileTransform)
                .ToUniTask(cancellationToken);
        }
    }
}