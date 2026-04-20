using TnieYuPackage.GlobalExtensions;
using UnityEngine.UIElements;

namespace TnieYuPackage.Utils
{
    public class TooltipVisualElement : BaseElement
    {
        private const string ELEMENT_PREFIX = "tooltip-visual-element";
        protected override string Prefix => ELEMENT_PREFIX;

        public TooltipVisualElement()
        {
            this.AddClass(ELEMENT_PREFIX);
            this.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            this.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
        }

        private void OnMouseEnter(MouseEnterEvent evt)
        {
            // refactor code UI game for TnieYuPackage to use.
            // using tooltip to display
        }

        private void OnMouseLeave(MouseLeaveEvent evt)
        {
        }
    }
}