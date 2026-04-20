using System.Collections.Generic;
using System.Linq;
using Game.BaseGameplay;
using TnieYuPackage.GlobalExtensions;
using UnityEngine;
using ZLinq;

namespace Game.WaveAttack
{
    public class TowerDefenseAI
    {
        //rules: Loop tower_place 0 -> n
        //Priority:
        //1. tower_value.
        //2. near the start -> end.

        //action
        //(expected weight): by context.
        private static readonly Dictionary<TowerLevel, float> ExpectedWeightActions = new()
        {
            { TowerLevel.Level0, 1f },
            { TowerLevel.Level1, 1.18f },
            { TowerLevel.Level2, 1.46f },
            { TowerLevel.Level3, 1.96f },
        };

        // weights
        private const float EACH_TOWER_MOVE_WEIGHT_INCREASE = 0.0134f;
        private const float TOWER_VALUE_WEIGHT = 0.434f;

        // private const float TOWER_TYPE_WEIGHT = 0.234f;

        // runtime weights
        private readonly float stepMoveWeight; // follow tower counts. current standard for 10.

        private float slope;
        private readonly float growthSlopeFactor; // follow wave context.
        public float Intercept; // follow map context. current standard for 2 path.

        public TowerDefenseAI(float defaultSlope = 1f, float growthSlopeFactor = 0.128f, int mapPathCount = 2,
            int towerCount = 10)
        {
            Intercept = 0.2f + (2 - mapPathCount) * TOWER_VALUE_WEIGHT * 0.8f;
            slope = defaultSlope;
            this.growthSlopeFactor = growthSlopeFactor;
            stepMoveWeight = 0.1f + (10 - towerCount) * EACH_TOWER_MOVE_WEIGHT_INCREASE;
        }

        // algorithm
        private float Algorithm(float x)
        {
            return slope * x + Intercept;
        }

        // TowerValue around == 1.5

        /// separate problem => 2 math
        /// 1. tower position <=> TowerRuntime [TowerValue, StepMove, Algorithm]
        /// 2. tower upgrade <=> TowerPresetSO (temp) [Random]
        /// <summary>
        /// Get tower need to handler. AI Handle for smart tower upgrade.
        /// Result: TowerRuntime -> Tower, TowerPresetSo -> Upgrade preset
        /// </summary>
        /// <param name="towerPlaces"></param>
        /// <returns>[Can be null]</returns>
        public (TowerRuntime, TowerPresetSo) GetTower(List<TowerRuntime> towerPlaces)
        {
            for (int i = 0; i < towerPlaces.Count; i++)
            {
                var towerPlace = towerPlaces[i];

                if (!CanConsiderTower(towerPlace, i)) continue;

                var affordablePresets = GetAffordablePresets(towerPlace);

                if (affordablePresets.Count == 0) continue;

                var selectTowerIndex = PickPreset(affordablePresets);
                return (towerPlace, affordablePresets[selectTowerIndex]);
            }

            return (null, null);
        }

        private static int PickPreset(List<TowerPresetSo> affordablePresets)
        {
            return Random.Range(0, affordablePresets.Count);
        }

        private static List<TowerPresetSo> GetAffordablePresets(TowerRuntime towerPlace)
        {
            var currentMoney = BaseGameplayController.Instance.money.Value;

            var formattedTowers = TowerUpgradeTree
                .Tree[towerPlace.currentPreset.objectId]
                .nextUpgradeTowers
                .AsValueEnumerable()
                .Where(p =>
                    TowerPresetSo.CalculateCost(towerPlace.currentPreset, p) <= currentMoney
                ).ToList();
            return formattedTowers;
        }

        private bool CanConsiderTower(TowerRuntime towerPlace, int i)
        {
            var expectedWeight = ExpectedWeightActions[towerPlace.currentPreset.towerLevel];

            var x = towerPlace.towerScore * TOWER_VALUE_WEIGHT + i * stepMoveWeight;
            var y = Algorithm(x);

            return y > expectedWeight;
        }

        public void GrowthSlope()
        {
            slope += growthSlopeFactor;
        }
    }
}