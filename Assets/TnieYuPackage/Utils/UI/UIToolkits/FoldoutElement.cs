using TnieYuPackage.GlobalExtensions;
using UnityEngine.UIElements;

namespace TnieYuPackage.Utils
{
    /// <summary>
    /// UIToolkit fold-in/fold-out based on visible/hidden style & mouse event.
    /// Mouse Enter -> Visible
    /// Mouse Leave -> Hidden
    /// </summary>
    public sealed class FoldoutElement : BaseElement
    {
        private const string ELEMENT_PREFIX = "foldout-element";
        protected override string Prefix => ELEMENT_PREFIX;

        public VisualElement Container;
        public VisualElement Header;
        public VisualElement Content;

        /// <summary>
        /// Get Hidden Style for Container (Fold-in) in .uss
        /// </summary>
        public string ContainerHiddenStyle { get; }

        /// <summary>
        /// Get Visible Style for Container (Fold-out) in .uss
        /// </summary>
        public string ContainerVisibleStyle { get; }

        public FoldoutElement()
        {
            this.AddClass(ELEMENT_PREFIX);
            this.pickingMode = PickingMode.Ignore;
            Container = this.CreateChild(GetStyleName("container"));
            Header = Container.CreateChild(GetStyleName("header"));
            Content = Container.CreateChild(GetStyleName("content"));

            ContainerHiddenStyle = "container--hidden";
            ContainerVisibleStyle = "container--visible";
            Container.style.position = Position.Absolute;
            Container.AddClass(GetStyleName(ContainerHiddenStyle));

            Container.RegisterCallback<MouseEnterEvent>(HandleMouseEnterContainer);
            Container.RegisterCallback<MouseLeaveEvent>(HandleMouseLeaveContainer);
        }

        private void HandleMouseEnterContainer(MouseEnterEvent evt)
        {
            Container.RemoveClass(GetStyleName(ContainerHiddenStyle));
            Container.AddClass(GetStyleName(ContainerVisibleStyle));
        }

        private void HandleMouseLeaveContainer(MouseLeaveEvent evt)
        {
            Container.RemoveClass(GetStyleName(ContainerVisibleStyle));
            Container.AddClass(GetStyleName(ContainerHiddenStyle));
        }
    }
}