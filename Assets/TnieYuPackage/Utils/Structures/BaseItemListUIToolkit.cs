using System.Collections.Generic;
using KBCore.Refs;
using TnieYuPackage.GlobalExtensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace TnieYuPackage.Utils.Structures
{
    [RequireComponent(typeof(UIDocument))]
    public abstract class BaseItemListUIToolkit<TData, TItem, TSingleton> : SingletonGUIBehaviour<TSingleton>
        where TItem : VisualElement
        where TSingleton : BaseItemListUIToolkit<TData, TItem, TSingleton>
    {
        [SerializeField, Self] private UIDocument uiDocument;
        [SerializeField] private List<StyleSheet> styleSheets;

        protected void OnEnable()
        {
            Initialize(uiDocument.rootVisualElement);
        }

        protected virtual void Initialize(VisualElement root)
        {
            foreach (var styleSheet in styleSheets)
            {
                root.styleSheets.Add(styleSheet);
            }

            var container = root.CreateChild<VisualElement>("container");

            var title = container.CreateChild<Label>("title");
            SetupTitle(title);

            var list = container.CreateChild<ListView>("list");
            list.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;

            list.itemsSource = GetItemsSource();
            list.makeItem = CreateItem;
            list.bindItem = BindItem;
        }

        protected abstract void SetupTitle(Label title);

        protected abstract List<TData> GetItemsSource();

        protected abstract TItem CreateItem();

        protected abstract void HandleObject(TItem item, TData data);

        protected virtual void BindItem(VisualElement element, int index)
        {
            if (element is not TItem elementItem)
            {
                Debug.LogError($"Element {element.name} is not of type {typeof(TItem).Name}");
                return;
            }

            HandleObject(elementItem, GetItemsSource()[index]);
        }
    }
}