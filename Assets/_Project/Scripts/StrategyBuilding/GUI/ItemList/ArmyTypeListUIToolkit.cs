using System.Collections.Generic;
using System.Linq;
using TnieYuPackage.Utils.Structures;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.StrategyBuilding
{
    [RequireComponent(typeof(UIDocument))]
    public class ArmyTypeListUIToolkit : BaseItemListUIToolkit<ArmyTypePresetSo, DetailItem, ArmyTypeListUIToolkit>
    {
        private ArmyBuildingBehaviour currentArmyBuilding;
        private int slotIndex;

        public void Display(ArmyBuildingBehaviour armyBuilding, int index)
        {
            currentArmyBuilding = armyBuilding;
            slotIndex = index;
            Show();
        }
        
        protected override void SetupTitle(Label title)
        {
            title.text = "Army List";
        }

        protected override List<ArmyTypePresetSo> GetItemsSource()
        {
            return ArmyTypePresetManager.Instance.data.Dictionary.Values.ToList();
        }

        protected override DetailItem CreateItem()
        {
            return new DetailItem();
        }

        protected override void HandleObject(DetailItem item, ArmyTypePresetSo data)
        {
            string itemDetailCost = $"{data.cost.ToString()}\n" +
                                    $"time: {data.delaySpawn}";
            item.SetItem(data.armyType.ToString(), data.icon, itemDetailCost);

            item.RegisterCallback<ClickEvent>(_ =>
            {
                Hide();
                BlurBackground.CloseManual();

                currentArmyBuilding.SetGeneratedArmy(data, slotIndex);
            });
        }

        private void Show()
        {
            BlurBackground.Open();
            gameObject.SetActive(true);
        }
        
        public override void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}