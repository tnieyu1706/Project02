using System;
using System.Collections.Generic;
using System.Linq;
using BackboneLogger;
using Game.Global;
using TnieYuPackage.DictionaryUtilities;
using TnieYuPackage.GlobalExtensions;
using UnityEngine;
using ZLinq;

namespace Game.BaseGameplay
{
    [Serializable]
    public class WaveDataConfigDto
    {
        [SerializeField] private SerializableDictionary<ArmyType, int> military;

        public Dictionary<ArmyType, int> MilitaryClone
        {
            get
            {
                var clone = new Dictionary<ArmyType, int>();
                foreach (var kvp in military.Dictionary)
                {
                    clone[kvp.Key] = kvp.Value;
                }

                return clone;
            }
        }
    }

    public readonly struct WaveDataConfig
    {
        public WaveDataConfig(Dictionary<ArmyType, int> military)
        {
            Military = military;
        }

        public Dictionary<ArmyType, int> Military { get; }

        public static bool IsNoneData(Dictionary<ArmyType, int> military)
        {
            return military.Count == 0 || military.Values.All(v => v == 0);
        }

        public static void Ensure(Dictionary<ArmyType, int> military)
        {
            military = military.Where(kvp => kvp.Value != 0)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }

    public static class SmartWaveConverter
    {
        //keyframe rule
        private static readonly Vector2Int MinMaxKeyframe = new(2, 5);
        private static readonly Vector2Int MinMaxKeyframeTimeError = new(-2, 2);
        private const float AVERAGE_KEYFRAME_INTERVAL = 10f;

        //entity rule
        private static readonly Dictionary<ArmyType, int> AverageEntityGroupMax = new()
        {
            { ArmyType.Melee, 5 },
            { ArmyType.Range, 3 },
            { ArmyType.Strong, 1 },
            { ArmyType.Quick, 2 }
        };

        //path decision: using score algorithm => path (min score)

        private static PathSpawn GetPathMinScore(Dictionary<PathSpawn, float> pathScoresSource)
        {
            PathSpawn path = null;
            float minScore = 999;
            foreach (var kvp in pathScoresSource)
            {
                if (kvp.Value < minScore)
                {
                    minScore = kvp.Value;
                    path = kvp.Key;
                }
            }

            return path;
        }

        // runtime methods
        public static WaveSpawn Convert(WaveDataConfig config)
        {
            // handle: keyframe
            if (WaveDataConfig.IsNoneData(config.Military))
            {
                return new WaveSpawn()
                {
                    keyframes = new List<KeyframeSpawn>()
                };
            }

            var keyframeNumber = MinMaxKeyframe.GetRandom();
            float timeline = 0;

            List<KeyframeSpawn> keyframeSpawns = new();
            Dictionary<ArmyType, int> military = new();

            Dictionary<ArmyType, int> armySlices = new();
            foreach (var kvp in config.Military)
            {
                armySlices[kvp.Key] = kvp.Value / keyframeNumber;
            }


            for (int i = 0; i < keyframeNumber && !WaveDataConfig.IsNoneData(config.Military); i++)
            {
                military.Clear();
                foreach (var armySource in config.Military)
                {
                    var armyValue = armySlices[armySource.Key];

                    if (i == 0)
                    {
                        armyValue += armySource.Value % keyframeNumber;
                    }

                    military[armySource.Key] = armyValue;
                }

                WaveDataConfig.Ensure(military);

                foreach (var army in military)
                {
                    config.Military[army.Key] -= army.Value;
                }

                WaveDataConfig.Ensure(config.Military);
                if (WaveDataConfig.IsNoneData(military)) continue;

                //handle: path
                var paths = PathManager.Instance.Paths.Keys
                    .AsValueEnumerable()
                    .Select(k =>
                        new PathSpawn()
                        {
                            pathId = k,
                            entitySpawns = new List<EntitySpawn>()
                        })
                    .ToList();

                Dictionary<PathSpawn, float> pathScores = paths
                    .AsValueEnumerable()
                    .ToDictionary(p => p, _ => 0f);

                foreach (var armyStorage in military)
                {
                    int armyStorageValue = armyStorage.Value;
                    var groupQuantity = AverageEntityGroupMax[armyStorage.Key];

                    while (armyStorageValue > 0)
                    {
                        int quantity = armyStorageValue > groupQuantity
                            ? groupQuantity
                            : armyStorageValue;

                        armyStorageValue -= quantity;

                        var path = GetPathMinScore(pathScores);

                        var score = 0f;
                        //handle: entity
                        var armyData = armyStorage.Key.GetArmyTypeData();
                        path.entitySpawns.Add(new EntitySpawn()
                        {
                            entityId = armyData.entityId,
                            quantity = quantity,
                            spawnInterval = armyData.spawnInterval,
                        });

                        score += armyData.pathScore * quantity;
                        //end-handle: entity

                        pathScores[path] += score;
                    }
                }
                //end-handle: path

                keyframeSpawns.Add(new KeyframeSpawn()
                {
                    time = timeline,
                    pathWaves = paths
                });

                timeline += AVERAGE_KEYFRAME_INTERVAL + MinMaxKeyframeTimeError.GetRandom();
            }
            //end-handle: keyframe

            WaveSpawn result = new WaveSpawn()
            {
                keyframes = keyframeSpawns,
            };

            BLogger.Log($"[SmartWaveConverter]: Keyframe number: {keyframeNumber}", category: "AI");

            return result;
        }
    }
}