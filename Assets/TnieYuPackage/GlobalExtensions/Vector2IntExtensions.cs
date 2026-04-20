using System;
using UnityEngine;
using Random = UnityEngine.Random;

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

        public static Vector2Int[] GetNeighbors(this Vector2Int vector)
        {
            return new Vector2Int[]
            {
                new Vector2Int(vector.x, vector.y + 1),
                new Vector2Int(vector.x + 1, vector.y + 1),
                new Vector2Int(vector.x + 1, vector.y),
                new Vector2Int(vector.x + 1, vector.y - 1),
                new Vector2Int(vector.x, vector.y - 1),
                new Vector2Int(vector.x - 1, vector.y - 1),
                new Vector2Int(vector.x - 1, vector.y),
                new Vector2Int(vector.x - 1, vector.y + 1)
            };
        }
    }
}