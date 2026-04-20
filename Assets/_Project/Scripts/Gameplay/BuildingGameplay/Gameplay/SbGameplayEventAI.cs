using System.Collections.Generic;
using Game.BaseGameplay;
using TnieYuPackage.GlobalExtensions;
using UnityEngine;
using EventType = Game.BaseGameplay.EventType;

namespace Game.BuildingGameplay
{
    public static class SbGameplayEventAI
    {
        // test
        private static readonly Vector2 MinMaxNextEventTime = new Vector2(300f, 500f);

        private static readonly Dictionary<ResourceType, Vector2> RandomizedAwards = new()
        {
            { ResourceType.Coin, new Vector2(20, 120f) },
            { ResourceType.Wood, new Vector2(20, 80f) },
            { ResourceType.Stone, new Vector2(20, 80f) },
        };

        public static Dictionary<ResourceType, float> GetAwardsRandomized()
        {
            return new Dictionary<ResourceType, float>()
            {
                { ResourceType.Coin, RandomizedAwards[ResourceType.Coin].GetRandom() },
                { ResourceType.Wood, RandomizedAwards[ResourceType.Wood].GetRandom() },
                { ResourceType.Stone, RandomizedAwards[ResourceType.Stone].GetRandom() },
            };
        }

        /// <summary>
        /// AI quyết định thời gian diễn ra sự kiện tiếp theo và trả về trực tiếp EventConfig đã được cấu hình.
        /// </summary>
        public static (float raisedTime, EventData eventConfig) GenerateNextEvent(float currentTime,
            int eventRaiseThTime)
        {
            // 1. Tính toán thời gian Raise
            var raisedTime = Random.Range(MinMaxNextEventTime.x, MinMaxNextEventTime.y) + currentTime;

            // current: default Easy, DefenseEvent only
            LevelType eventLevelType = LevelType.Easy;
            EventType eventType = EventType.Defense;

            EventData nextEvent = new EventData()
            {
                eventName = $"Event_{eventType}_{eventLevelType}_{eventRaiseThTime}",
                levelType = eventLevelType,
                eventType = eventType,
            };
            EventData.SetupAwardsRandomized(nextEvent);

            Debug.Log($"[SbGameplayEventAI] Decided next event at {raisedTime}: {nextEvent.eventName}");

            return (raisedTime, nextEvent);
        }

        // Tương lai bạn có thể viết thêm hàm:
        // private static EnemyBaseEventConfig CreateWaveAttackEvent(int eventRaiseThTime) { ... }
    }
}