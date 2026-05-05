using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using TnieYuPackage.GlobalExtensions;
using UnityEngine;

namespace TnieYuPackage.Strategies.Projectile
{
    [Serializable]
    public class ParabolaTrajectoryStrategy : IProjectileTrajectoryStrategy
    {
        [Tooltip("Tự động xoay đạn theo hướng bay")]
        public bool autoRotate = true;

        [Tooltip("Độ cao của vòng cung")] public float parabolaHeight = 2f;

        public async UniTask ExecuteTrajectory(Transform projectileTransform, Vector3 startPos, Vector3 endPos,
            float duration, CancellationToken cancellationToken)
        {
            Vector3 previousPos = startPos;

            await LMotion.Create(0f, 1f, duration)
                .Bind(projectileTransform, (t, targetTransform) =>
                {
                    if (targetTransform == null) return;

                    // 1. Tính toán vị trí tịnh tiến thẳng (Linear)
                    Vector3 linearPos = Vector3.Lerp(startPos, endPos, t);

                    // 2. Tính toán độ cao nhô lên dùng hàm Sin
                    float arcOffset = Mathf.Sin(t * Mathf.PI) * parabolaHeight;

                    // 3. Vị trí thực tế = Vị trí tịnh tiến + Độ cao
                    Vector3 currentPos = linearPos + new Vector3(0, arcOffset, 0);

                    // 4. Xoay hướng mũi tên
                    if (autoRotate && t > 0)
                    {
                        Vector3 direction = currentPos - previousPos;
                        targetTransform.RotateTowards2D(direction);
                    }

                    // Cập nhật vị trí
                    targetTransform.position = currentPos;
                    previousPos = currentPos;
                })
                .ToUniTask(cancellationToken);
        }
    }
}