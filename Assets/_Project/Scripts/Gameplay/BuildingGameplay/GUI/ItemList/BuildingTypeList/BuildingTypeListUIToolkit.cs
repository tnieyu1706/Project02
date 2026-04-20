using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Game.StrategyBuilding;
using TnieYuPackage.GlobalExtensions;
using TnieYuPackage.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.BuildingGameplay
{
    [RequireComponent(typeof(UIDocument))]
    public class
        BuildingTypeListUIToolkit : BaseItemListUIToolkit<BuildingPresetSo, BuildingDetailItem,
        BuildingTypeListUIToolkit>
    {
        protected override void SetupTitle(Label title)
        {
            title.text = "Building Type";
        }

        protected override List<BuildingPresetSo> GetItemsSource()
        {
            return BuildingPresetManager.Instance.data.Dictionary.Values.ToList();
        }

        // OVERRIDE HÀM NÀY ĐỂ RENDER THEO TỪNG CATEGORY THAY VÌ 1 LIST DÀI
        protected override void PopulateItems(VisualElement parentContainer)
        {
            activeItems.Clear();
            var allItems = GetItemsSource();

            // Nhóm các công trình lại theo Category
            var groupedItems = allItems
                .GroupBy(x => x.buildingCategory)
                .Where(g => g.Key != BuildingCategory.None); // Bỏ qua những công trình không có category

            foreach (var group in groupedItems)
            {
                // 1. Tạo Tiêu đề cho nhóm (Category Name)
                var categoryLabel = parentContainer.CreateChild<Label>("category-title");
                // Thêm dấu cách vào các từ viết hoa (VD: ResourceProduction -> Resource Production)
                categoryLabel.text = Regex.Replace(group.Key.ToString(), "(\\B[A-Z])", " $1");

                // 2. Tạo Grid View chứa các công trình của nhóm đó
                var gridView = parentContainer.CreateChild<VisualElement>("grid-view");

                foreach (var item in group)
                {
                    var itemElement = CreateItem();
                    HandleObject(itemElement, item);
                    gridView.Add(itemElement);
                    activeItems.Add((itemElement, item));
                }
            }
        }

        protected override BuildingDetailItem CreateItem()
        {
            return new BuildingDetailItem();
        }

        protected override void HandleObject(BuildingDetailItem item, BuildingPresetSo data)
        {
            item.SetItem(data);

            item.RegisterCallback<ClickEvent>(
                _ =>
                {
                    if (!SbGameplayController.ValidateCost(data.costBuilding.Data)) return;

                    Hide();
                    BlurBackground.CloseManual();
                    SbSpawnBuildingSystem.StartBuilding(data);
                }
            );
        }

        public override void Show()
        {
            base.Show();
            if (SbGameplayController.Instance != null)
            {
                SbGameplayController.OnResourceChanged += ValidateItems;
                ValidateItems(); // Kiểm tra ngay lập tức khi vừa mở giao diện
            }
        }

        public override void Hide()
        {
            base.Hide();
            if (SbGameplayController.Instance != null)
            {
                SbGameplayController.OnResourceChanged -= ValidateItems;
            }
        }

        private void ValidateItems()
        {
            foreach (var tuple in activeItems)
            {
                bool isValid = SbGameplayController.ValidateCost(tuple.itemData.costBuilding.Data);
                tuple.itemElement.SetEnabled(isValid);
            }
        }
    }
}