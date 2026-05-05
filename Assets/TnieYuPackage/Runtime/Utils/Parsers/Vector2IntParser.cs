using UnityEngine;

namespace TnieYuPackage.Utils.Parsers
{
    public class Vector2IntParser
    {
        public static Vector2Int ParseVector2Int(string input)
        {
            // Remove parentheses
            input = input.Trim('(', ')');

            // Split by comma
            var parts = input.Split(',');

            int x = int.Parse(parts[0]);
            int y = int.Parse(parts[1]);

            return new Vector2Int(x, y);
        }
    }
}