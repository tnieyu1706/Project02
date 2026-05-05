using System;
using System.Collections.Generic;
using TnieYuPackage.DesignPatterns;
using TnieYuPackage.DictionaryUtilities;
using UnityEngine;

namespace Game.BuildingGameplay
{
    public enum ResourceType
    {
        Coin,
        Wood,
        Stone,
        Food
    }

    public enum LimitResourceType
    {
        MaxWood,
        MaxStone,
        MaxFood,
        None
    }

    public static class ResourceTypeExtensions
    {
        public static LimitResourceType GetLimitResourceType(this ResourceType resourceType)
        {
            return resourceType switch
            {
                ResourceType.Wood => LimitResourceType.MaxWood,
                ResourceType.Stone => LimitResourceType.MaxStone,
                ResourceType.Food => LimitResourceType.MaxFood,
                _ => LimitResourceType.None
            };
        }
    }

    [Serializable]
    public struct ResourceTypeData
    {
        public Sprite icon;
        public string emoji;
    }

    [CreateAssetMenu(fileName = "ResourceTypeDataManager",
        menuName = "Game/StrategyBuilding/Resource/ResourceTypeDataManager")]
    public class ResourceTypeDataManager : ScriptableObject
    {
        [SerializeField] private SerializableDictionary<ResourceType, ResourceTypeData> resources = new();
        [SerializeField] private SerializableDictionary<LimitResourceType, ResourceTypeData> limitResources = new();
        [SerializeField] private ResourceTypeData convenientData;
        [SerializeField] private ResourceTypeData adverseData;

        public Dictionary<ResourceType, ResourceTypeData> Resources => resources.Dictionary;
        public Dictionary<LimitResourceType, ResourceTypeData> LimitResources => limitResources.Dictionary;

        // public static ResourceTypeData ConvenientData => Instance.convenientData;
        // public static ResourceTypeData AdverseData => Instance.adverseData;
    }
}