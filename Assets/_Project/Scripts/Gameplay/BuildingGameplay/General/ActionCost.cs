using System;
using System.Collections.Generic;
using Reflex.Attributes;
using Reflex.Core;
using TnieYuPackage.DictionaryUtilities;
using UnityEngine;

namespace Game.BuildingGameplay
{
    //write ActionCostUIElement. setup by ActionCost.
    //each cost: [image]: value
    //[image.sprite] get through ResourceTypeDataManager

    [Serializable]
    public struct SerializableActionCost
    {
        [SerializeField] private SerializableDictionary<ResourceType, float> resourceCosts;

        private ActionCost actionCost;

        public ActionCost Data
        {
            get
            {
                if (actionCost == null)
                {
                    var container = Container.RootContainer;
                    actionCost = container.Instantiate<ActionCost>();
                    actionCost.Setup(resourceCosts.Dictionary);
                }

                return actionCost;
            }
        }

        public ActionCost CloneData
        {
            get
            {
                var container = Container.RootContainer;
                var newActionCost = container.Instantiate<ActionCost>();
                newActionCost.Setup(resourceCosts.Dictionary);

                return newActionCost;
            }
        }
    }

    public class ActionCost
    {
        [Inject] ResourceTypeDataManager resourceTypeDataManager;

        public Dictionary<ResourceType, float> ResourceCosts;

        [ReflexConstructor]
        public ActionCost(ResourceTypeDataManager resourceTypeDataManager)
        {
            this.resourceTypeDataManager = resourceTypeDataManager;
        }

        public void Setup(Dictionary<ResourceType, float> resourceCosts)
        {
            this.ResourceCosts = resourceCosts;
        }

        public static ActionCost operator +(ActionCost cost, ActionCost otherCost)
        {
            foreach (var resourceCost in otherCost.ResourceCosts)
            {
                if (cost.ResourceCosts.ContainsKey(resourceCost.Key))
                {
                    cost.ResourceCosts[resourceCost.Key] += resourceCost.Value;
                }
                else
                {
                    cost.ResourceCosts[resourceCost.Key] = resourceCost.Value;
                }
            }

            return cost;
        }

        public static ActionCost operator *(ActionCost cost, float multiplier)
        {
            // Tạo ra một Dictionary mới để không làm thay đổi cost gốc của Preset
            var newResourceCosts = new Dictionary<ResourceType, float>();

            foreach (var kvp in cost.ResourceCosts)
            {
                newResourceCosts[kvp.Key] = kvp.Value * multiplier;
            }

            var container = Container.RootContainer;
            var newCost = container.Instantiate<ActionCost>();
            newCost.Setup(newResourceCosts);
            return newCost;
        }

        /// <summary>
        /// Trả về chuỗi chi phí đã được format xuống dòng để dễ dàng hiển thị lên UI / Tooltip.
        /// </summary>
        public string GetTextVertical()
        {
            var parts = new List<string>();

            foreach (var kvp in ResourceCosts)
            {
                if (kvp.Value > 0)
                {
                    parts.Add($"{resourceTypeDataManager.Resources[kvp.Key].emoji} {kvp.Value}");
                }
            }

            return string.Join("\n", parts);
        }

        public string GetTextHorizontal()
        {
            var parts = new List<string>();

            foreach (var kvp in ResourceCosts)
            {
                if (kvp.Value > 0)
                {
                    parts.Add($"{resourceTypeDataManager.Resources[kvp.Key].emoji} {kvp.Value}");
                }
            }

            return string.Join(" | ", parts);
        }
    }
}