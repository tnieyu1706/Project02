using UnityEngine;

namespace TnieYuPackage.GlobalExtensions
{
    public static class Vector2IntExtensions
    {
        public static int GetLineLength(this Vector2Int vector)
        {
            return vector.y - vector.x;
        }

        public static int GetRandom(this Vector2Int vector)
        {
            return Random.Range(vector.x, vector.y);
        }

        public static bool IsBelongFrom(this Vector2Int vector, int value)
        {
            return vector.x >= value && value <= vector.y;
        }

        public static Vector3Int ToVector3Int(this Vector2Int vector, int z)
        {
            return new Vector3Int(vector.x, vector.y, z);
        }
    }
}