using UnityEngine;

namespace TnieYuPackage.Utils
{
    public static class Vector2IntUtils
    {
        public static Vector2Int[] Get4DirectionalVectors()
        {
            return new Vector2Int[]
            {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right
            };
        }
        
        public static Vector2Int[] Get8DirectionalVectors()
        {
            return new Vector2Int[]
            {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right,
                new Vector2Int(1, 1),   // Up-Right
                new Vector2Int(-1, 1),  // Up-Left
                new Vector2Int(1, -1),  // Down-Right
                new Vector2Int(-1, -1)  // Down-Left
            };
        }
    }
}