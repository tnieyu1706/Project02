using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using Game.Global;
using Reflex.Attributes;
using TnieYuPackage.GlobalExtensions;
using TnieYuPackage.Utils;
using UnityEngine;
using UnityEngine.UIElements;
using SerializeButton = EditorAttributes.ButtonAttribute;

namespace Game.BuildingGameplay
{
    [RequireComponent(typeof(UIDocument))]
    public class ArmyTypeListUIToolkit : BaseItemListUIToolkit<ArmyTypePresetSo, ArmyDetailItem, ArmyTypeListUIToolkit>
    {
        [Inject] private ArmyTypePresetManager armyTypePresetManager;

        private List<ArmyType> filteredArmyTypes = new List<ArmyType>();

        /// <summary>
        /// Task CompletionSource to get result when player select an army type preset.
        /// </summary>
        public UniTaskCompletionSource<ArmyTypePresetSo> ResultTcs { get; private set; }

        /// <summary>
        /// Waiting for player to select an army type preset, then return the selected preset.
        /// </summary>
        /// <returns></returns>
        public UniTask<ArmyTypePresetSo> DisplayAndWait()
        {
            Show();
            return StartWaitingHandler();
        }

        /// <summary>
        /// Provider: create new Task CompletionSource and return Task, also register cancellation when this component destroyed.
        /// </summary>
        /// <returns></returns>
        private UniTask<ArmyTypePresetSo> StartWaitingHandler()
        {
            ResultTcs = new UniTaskCompletionSource<ArmyTypePresetSo>();
            this.GetCancellationTokenOnDestroy().Register(HandleTcsCancellation);

            return ResultTcs.Task;
        }

        private void HandleTcsCancellation()
        {
            if (ResultTcs == null) return;

            ResultTcs.TrySetCanceled();
            ResultTcs = null;
        }

        /// <summary>
        /// Resolver: confirm Task CompletionSource by SetValue
        /// </summary>
        /// <returns></returns>
        private void ConfirmTcs(ArmyTypePresetSo result)
        {
            if (ResultTcs == null) return;

            ResultTcs.TrySetResult(result);
            ResultTcs = null;
        }

        protected override void SetupTitle(Label title)
        {
            title.text = "Army Type";
        }

        protected override List<ArmyTypePresetSo> GetItemsSource()
        {
            return armyTypePresetManager.presets;
        }

        protected override ArmyDetailItem CreateItem()
        {
            return new ArmyDetailItem();
        }

        protected override void PopulateItems(VisualElement parentContainer)
        {
            activeItems.Clear();
            var allItems = GetItemsSource();

            // Nhóm các quân lính lại theo ArmyCategory
            var groupedItems = allItems.GroupBy(x => x.armyCategory);

            foreach (var group in groupedItems)
            {
                // 1. Tạo Tiêu đề cho nhóm
                var categoryLabel = parentContainer.CreateChild<Label>("category-title");
                categoryLabel.text = Regex.Replace(group.Key.ToString(), "(\\B[A-Z])", " $1");

                // 2. Tạo Grid View chứa các item của nhóm đó
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

        /// <summary>
        /// Handle Object Data (async) UI
        /// Register ClickEvent: once click, hide this UI, close blur background, and confirm Task CompletionSource with selected data.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="data"></param>
        protected override void HandleObject(ArmyDetailItem item, ArmyTypePresetSo data)
        {
            item.SetItem(data);

            item.RegisterCallback<ClickEvent>(_ =>
            {
                if (!SbGameplayController.ValidateCost(data.cost.Data)) return;

                ConfirmTcs(data);

                Hide();
                BlurBackground.CloseManual();
            });
        }

        [SerializeButton]
        public override void Hide()
        {
            base.Hide();

            if (SbGameplayController.HasInstance)
            {
                SbGameplayController.OnResourceChanged -= ValidateItems;
            }

            if (ResultTcs == null) return;

            ResultTcs.TrySetResult(null);
            ResultTcs = null;
        }

        public override void Show()
        {
            filteredArmyTypes.Clear();
            filteredArmyTypes = GamePropertiesRuntime.Instance
                .UnlockArmyTypeDict
                .Where(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var item in activeItems)
            {
                item.itemElement.style.display =
                    filteredArmyTypes.Contains(item.itemData.armyType)
                        ? DisplayStyle.Flex
                        : DisplayStyle.None;
            }

            base.Show();

            if (!SbGameplayController.HasInstance) return;
            SbGameplayController.OnResourceChanged += ValidateItems;
            ValidateItems(); // Cập nhật trạng thái item ngay khi hiển thị lên màn hình
        }

        private void ValidateItems()
        {
            foreach (var tuple in activeItems)
            {
                bool isValid = SbGameplayController.ValidateCost(tuple.itemData.cost.Data);
                tuple.itemElement.SetEnabled(isValid);
            }
        }
    }
}