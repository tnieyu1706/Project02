using System;
using System.Collections.Generic;
using TnieYuPackage.DesignPatterns;
using TnieYuPackage.GlobalExtensions;
using TnieYuPackage.Utils;
using UnityEngine;

namespace Game.StrategyBuilding
{
    [Serializable]
    public class TileContextDisplay : SingletonBehavior<TileContextDisplay>
    {
        [SerializeField] private GameObject contextPrefab;
        [SerializeField] private Transform transformOffset;
        [SerializeField] private int zLevel;

        private SpriteRenderer centerContext;
        private Dictionary<Vector2Int, SpriteRenderer> neighborContexts = new();

        protected override void Awake()
        {
            base.Awake();
            centerContext = CreateDirectionObject(Vector2Int.zero);

            foreach (var neighborDir in Vector2IntUtils.Get8DirectionalVectors())
            {
                var spriteRenderer = CreateDirectionObject(neighborDir);
                neighborContexts[neighborDir] = spriteRenderer;
            }
        }

        private SpriteRenderer CreateDirectionObject(Vector2Int neighborDir)
        {
            var gObj = Instantiate(contextPrefab, transformOffset);
            gObj.transform.localPosition = neighborDir.ToVector3Int(zLevel);
            var spriteRenderer = gObj.GetComponent<SpriteRenderer>();
            return spriteRenderer;
        }

        private static void HideNeighbors()
        {
            foreach (var neighbor in Instance.neighborContexts.Values)
            {
                neighbor.sprite = null;
            }
        }

        public static void Display(Vector2Int pos, Action<SpriteRenderer> centerHandler,
            Dictionary<Vector2Int, Sprite> neighbors = null)
        {
            Instance.transform.position = pos.ToVector3Int(Instance.zLevel);
            centerHandler?.Invoke(Instance.centerContext);

            HideNeighbors();
            if (neighbors == null) return;
            foreach (var neighbor in neighbors)
            {
                if (!Instance.neighborContexts.TryGetValue(neighbor.Key, out var spriteRenderer)) continue;

                spriteRenderer.sprite = neighbor.Value;
            }
        }
    }
}