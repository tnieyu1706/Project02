using System.Collections.Generic;
using UnityEngine;

namespace Game.Global
{
    public class GameSkillTreeRadialLayout
    {
        public static GameSkillTreeRadialLayout Instance;

        public struct NodeLayoutData
        {
            public Vector2 Position;
            public int LeafCount;
            public int Depth;

            public NodeLayoutData(int leafCount, int depth)
            {
                Position = Vector2.zero;
                LeafCount = leafCount;
                Depth = depth;
            }
        }

        public float RadiusStep { get; set; }
        public float AngleSpread { get; set; }
        public float Rotation { get; set; }

        public GameSkillTreeRadialLayout(float radiusStep = 150f, float angleSpread = 360f, float rotation = 0f)
        {
            RadiusStep = radiusStep;
            AngleSpread = angleSpread;
            Rotation = rotation;
        }

        private readonly Dictionary<string, NodeLayoutData> layoutCache = new();

        public Dictionary<string, NodeLayoutData> CalculateLayout(GameSkillTree tree)
        {
            layoutCache.Clear();

            if (tree == null)
                return layoutCache;

            // Pass 1: Bottom-up to calculate leaf counts (O(N))
            PreCalculateLeafCount(tree.Root, 0);

            // Pass 2: Top-down to calculate Cartesian positions
            CalculatePositionsRecursive(tree.Root, 0f, AngleSpread);

            return layoutCache;
        }

        private int PreCalculateLeafCount(GameSkillTreeNode node, int depth)
        {
            int currentLeafCount = 0;

            if (node.Children == null || node.Children.Count == 0)
            {
                currentLeafCount = 1;
            }
            else
            {
                foreach (var child in node.Children)
                {
                    currentLeafCount += PreCalculateLeafCount(child, depth + 1);
                }
            }

            layoutCache[node.Data.skillId] = new NodeLayoutData(currentLeafCount, depth);
            return currentLeafCount;
        }

        private void CalculatePositionsRecursive(GameSkillTreeNode node, float startAngle, float endAngle)
        {
            string skillId = node.Data.skillId;
            if (!layoutCache.TryGetValue(skillId, out NodeLayoutData data)) return;

            float midAngle = (startAngle + endAngle) / 2f;
            float currentRadius = data.Depth * RadiusStep;

            // Polar to Cartesian conversion
            float finalAngleRad = (midAngle + Rotation) * Mathf.Deg2Rad;
            data.Position = new Vector2(
                currentRadius * Mathf.Cos(finalAngleRad),
                currentRadius * Mathf.Sin(finalAngleRad)
            );

            layoutCache[skillId] = data;

            if (node.Children is { Count: > 0 })
            {
                float currentChildStartAngle = startAngle;
                int totalLeaves = data.LeafCount;

                foreach (var child in node.Children)
                {
                    int childLeafCount = layoutCache[child.Data.skillId].LeafCount;

                    // Share the angle wedge proportionally based on subtree size
                    float angleShare = ((float)childLeafCount / totalLeaves) * (endAngle - startAngle);

                    CalculatePositionsRecursive(child, currentChildStartAngle, currentChildStartAngle + angleShare);
                    currentChildStartAngle += angleShare;
                }
            }
        }

        public Vector2 GetNodePosition(string skillId)
        {
            return layoutCache.TryGetValue(skillId, out NodeLayoutData data) ? data.Position : Vector2.zero;
        }
    }
}