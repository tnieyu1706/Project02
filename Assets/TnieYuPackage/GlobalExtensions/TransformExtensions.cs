using UnityEngine;

namespace TnieYuPackage.GlobalExtensions
{
    public static class TransformExtensions
    {
        /// <summary>
        /// Xoay Transform trong môi trường 2D (trục Z) hướng về một vector chỉ định
        /// </summary>
        /// <param name="targetTransform">Transform cần xoay</param>
        /// <param name="direction">Vector hướng đến (thường là targetPos - currentPos)</param>
        /// <param name="offsetAngle">Góc bù (trừ đi 90 độ nếu Sprite mặc định hướng lên trên thay vì hướng sang phải)</param>
        public static void RotateTowards2D(this Transform targetTransform, Vector3 direction, float offsetAngle = 0f)
        {
            if (direction == Vector3.zero) return;
            
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            targetTransform.rotation = Quaternion.Euler(0, 0, angle + offsetAngle);
        }
    }
}