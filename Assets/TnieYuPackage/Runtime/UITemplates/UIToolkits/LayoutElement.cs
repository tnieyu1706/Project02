using TnieYuPackage.GlobalExtensions;
using UnityEngine.UIElements;

namespace TnieYuPackage.Utils
{
    public sealed class LayoutElement : BaseElement
    {
        private const string ELEMENT_PREFIX = "layout-element";
        protected override string Prefix => ELEMENT_PREFIX;

        public VisualElement Container;
        public VisualElement Header;
        public VisualElement Content;

        public LayoutElement()
        {
            this.AddClass(ELEMENT_PREFIX);
            this.pickingMode = PickingMode.Ignore;
            Container = this.CreateChild(GetStyleName("container"));
            Header = Container.CreateChild(GetStyleName("header"));
            Content = Container.CreateChild(GetStyleName("content"));
        }
    }
}