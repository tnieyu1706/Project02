using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EditorAttributes;
using KBCore.Refs;
using TnieYuPackage.GlobalExtensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace TnieYuPackage.Utils
{
    [RequireComponent(typeof(UIDocument))]
    public abstract class BaseItemListUIToolkit<TData, TItem, TSingleton> : SingletonDisplayUI<TSingleton>
        where TItem : VisualElement
        where TSingleton : BaseItemListUIToolkit<TData, TItem, TSingleton>
    {
        [SerializeField, Self] private UIDocument uiDocument;
        [SerializeField] private List<StyleSheet> styleSheets;

        protected VisualElement Root;
        protected List<(TItem itemElement, TData itemData)> activeItems = new();

        async void Start()
        {
            await UniTask.Yield();

            Root = uiDocument.rootVisualElement;
            Initialize();
            Hide();
        }

        protected virtual void Initialize()
        {
            Root.Clear();
            Root.styleSheets.Clear();
            foreach (var styleSheet in styleSheets)
            {
                Root.styleSheets.Add(styleSheet);
            }

            var container = Root.CreateChild<VisualElement>("container");

            var title = container.CreateChild<Label>("title");
            SetupTitle(title);

            // BỌC TRONG SCROLLVIEW ĐỂ CÓ THỂ CUỘN ĐƯỢC
            var scrollView = container.CreateChild<ScrollView>("scroll-view");
            
            // Tách logic tạo item ra hàm riêng để class con (BuildingType) có thể override (phân nhóm)
            PopulateItems(scrollView);
        }

        protected virtual void PopulateItems(VisualElement parentContainer)
        {
            var gridView = parentContainer.CreateChild<VisualElement>("grid-view");
            activeItems.Clear();

            foreach (var item in GetItemsSource())
            {
                var itemElement = CreateItem();
                HandleObject(itemElement, item);
                gridView.Add(itemElement);
                activeItems.Add((itemElement, item));
            }
        }

        protected abstract void SetupTitle(Label title);

        protected abstract List<TData> GetItemsSource();

        protected abstract TItem CreateItem();

        protected abstract void HandleObject(TItem item, TData data);

        [Button]
        public virtual void Show()
        {
            Root.style.display = DisplayStyle.Flex;
            BlurBackground.Show();
        }

        public override void Hide()
        {
            Root.style.display = DisplayStyle.None;
        }
    }
}