using TnieYuPackage.GlobalExtensions;
using UnityEngine.UIElements;

namespace TnieYuPackage.Utils
{
    public sealed class ContainerElement : BaseElement
    {
        private const string ELEMENT_PREFIX = "container-element";
        protected override string Prefix => ELEMENT_PREFIX;

        public VisualElement Container;
        public VisualElement Header;
        public VisualElement Content;
        public VisualElement Footer;

        public ContainerElement()
        {
            this.AddClass(ELEMENT_PREFIX);
            this.pickingMode = PickingMode.Ignore;

            Container = this.CreateChild(GetStyleName("container"));
            Header = Container.CreateChild(GetStyleName("header"));
            Content = Container.CreateChild(GetStyleName("content"));
            Footer = Container.CreateChild(GetStyleName("footer"));
        }
    }
}