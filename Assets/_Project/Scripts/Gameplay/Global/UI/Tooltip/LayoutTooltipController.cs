using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Scripts.Gameplay.Global.Tooltip
{
    public class LayoutTooltipController : BaseTooltipController<LayoutTooltipController>
    {
        private Action<VisualElement> onHideLayout;

        public void Display(Action<VisualElement> displayLayoutHandler, Action<VisualElement> hideLayoutHandler,
            Vector2 screenPosition)
        {
            this.onHideLayout = hideLayoutHandler;

            displayLayoutHandler?.Invoke(TooltipContainer);
            Show();
            RefreshPosition(screenPosition);
        }

        public override void Hide()
        {
            onHideLayout?.Invoke(TooltipContainer);
            base.Hide();
        }
    }
}