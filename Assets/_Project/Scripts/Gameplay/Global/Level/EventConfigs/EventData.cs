using System;
using System.Collections.Generic;
using Game.BuildingGameplay;
using Game.Global;
using Newtonsoft.Json.Linq;
using TnieYuPackage.DesignPatterns;
using TnieYuPackage.GlobalExtensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.BaseGameplay
{
    public enum EventType
    {
        Defense,
        Attack
    }

    [Serializable]
    public class SerializableEventData
    {
        public EventData data;
        public Sprite icon;
    }

    [Serializable]
    public class EventData : ISaveLoadData<JObject>
    {
        public string eventName;
        public bool isCompleted;
        public bool shouldChange;

        public EventType eventType;
        public LevelType levelType;

        public Dictionary<ResourceType, float> Awards { get; } = new();

        private static readonly Dictionary<ResourceType, Vector2> RandomizedAwards = new()
        {
            { ResourceType.Coin, new Vector2(20, 120f) },
            { ResourceType.Wood, new Vector2(20, 80f) },
            { ResourceType.Stone, new Vector2(20, 80f) },
            { ResourceType.Food, new Vector2(20, 80f) },
        };

        public static void SetupAwardsRandomized(EventData eventData)
        {
            var generatedAwards = new Dictionary<ResourceType, float>()
            {
                { ResourceType.Coin, RandomizedAwards[ResourceType.Coin].GetRandom() },
                { ResourceType.Wood, RandomizedAwards[ResourceType.Wood].GetRandom() },
                { ResourceType.Stone, RandomizedAwards[ResourceType.Stone].GetRandom() },
                { ResourceType.Food, RandomizedAwards[ResourceType.Food].GetRandom() },
            };

            eventData.Awards.Clear();
            foreach (var award in generatedAwards)
            {
                eventData.Awards[award.Key] = award.Value;
            }
        }

        public void ApplyEventResult()
        {
            if (isCompleted)
            {
                foreach (var award in Awards)
                {
                    SbGameplayController.AddResourceAndRefresh(award.Key, award.Value);
                }

                SbGameplayController.RefreshEvents();

                return;
            }

            if (eventType == EventType.Defense)
            {
                SbGameplayController.Instance.currentHealth.Value--;
            }
        }

        public BaseGameplayLevel GetGameplayLevel()
        {
            if (!LevelTypeManager.Refs.TryGetValue(levelType, out var levelRef)) return null;

            return levelRef.GetGameplayLevelBy(eventType);
        }

        public string GetEventHandlerName() => eventType.ToString();

        #region SaveLoad

        public void BindData(JObject data)
        {
            if (data.TryGetValue("EventName", out var eventNameToken))
            {
                this.eventName = eventNameToken.Value<string>();
            }

            if (data.TryGetValue("IsCompleted", out var isCompletedToken))
            {
                this.isCompleted = isCompletedToken.Value<bool>();
            }

            if (data.TryGetValue("ShouldChange", out var shouldChangeToken))
            {
                this.shouldChange = shouldChangeToken.Value<bool>();
            }

            if (data.TryGetValue("EventType", out var eventTypeToken))
            {
                this.eventType = Enum.Parse<EventType>(eventTypeToken.Value<string>(), true);
            }

            if (data.TryGetValue("LevelType", out var levelTypeToken))
            {
                this.levelType = Enum.Parse<LevelType>(levelTypeToken.Value<string>(), true);
            }

            if (data.TryGetValue("Awards", out var awardsToken) && awardsToken is JObject awardsObj)
            {
                foreach (var award in awardsObj)
                {
                    Awards[Enum.Parse<ResourceType>(award.Key.ToString())] = award.Value.Value<float>();
                }
            }
        }

        public JObject SaveData()
        {
            JObject result = new JObject()
            {
                ["EventName"] = eventName,
                ["IsCompleted"] = isCompleted,
                ["ShouldChange"] = shouldChange,
                ["EventType"] = eventType.ToString(),
                ["LevelType"] = levelType.ToString(),
            };

            JObject awards = new JObject();

            foreach (var award in Awards)
            {
                awards[award.Key.ToString()] = award.Value;
            }

            result["Awards"] = awards;

            return result;
        }

        #endregion
    }
}