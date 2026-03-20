using System.Collections.Generic;
using System.Linq;
using EditorAttributes;
using TnieYuPackage.Utils.Structures;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.StrategyBuilding
{
    [RequireComponent(typeof(UIDocument))]
    public class BuildingListUIToolkit : BaseItemListUIToolkit<BuildingPresetSo, DetailItem, BuildingListUIToolkit>
    {
        private BuildingPresetSo currentBuildingPreset;

        protected override void SetupTitle(Label title)
        {
            title.text = "Building List";
        }

        protected override List<BuildingPresetSo> GetItemsSource()
        {
            return BuildingPresetManager.Instance.data.Dictionary.Values.ToList();
        }

        protected override DetailItem CreateItem()
        {
            return new DetailItem();
        }

        protected override void HandleObject(DetailItem item, BuildingPresetSo data)
        {
            currentBuildingPreset = data;
            var buildingTypeInstaller = data.behaviourInstaller;

            item.SetItem(
                buildingTypeInstaller.BuildingName,
                data.buildingTile.sprite,
                data.costBuilding.ToString()
            );
            
            item.RegisterCallback<ClickEvent>(HandleItemClicked);
        }

        private void HandleItemClicked(ClickEvent _)
        {
            Hide();
            BlurBackground.CloseManual();
                
            SbMapController.Instance.StartBuilding(currentBuildingPreset);
        }

        [Button]
        public void Show()
        {
            BlurBackground.Open();
            Instance.gameObject.SetActive(true);
        }

        public override void Hide()
        {
            Instance.gameObject.SetActive(false);
        }
    }
}