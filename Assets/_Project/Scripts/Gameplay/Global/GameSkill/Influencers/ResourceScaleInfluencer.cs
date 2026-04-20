using System;
using Game.BuildingGameplay;
using UnityEngine;

namespace Game.Global
{
    /// <summary>
    /// Increases resource gain scale (Money, People, Wood, Stone, etc.)
    /// </summary>
    [Serializable]
    public class ResourceScaleInfluencer : ISkillInfluencer
    {
        public ResourceType resourceType;

        [Tooltip("Value to add to the current scale. E.g., 0.1 increases scale by 10%")]
        public float addValue;

        public void ApplyAffect()
        {
            if (GamePropertiesRuntime.Instance.ResourceReceivedScaleDict.ContainsKey(resourceType))
            {
                GamePropertiesRuntime.Instance.ResourceReceivedScaleDict[resourceType] += addValue;
            }
        }
    }
}