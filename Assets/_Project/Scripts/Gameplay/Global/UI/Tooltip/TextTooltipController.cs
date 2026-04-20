using TnieYuPackage.GlobalExtensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Scripts.Gameplay.Global.Tooltip
{
    public class TextTooltipController : BaseTooltipController<TextTooltipController>
    {
        private Label textLabel;

        protected override void SetupUI()
        {
            base.SetupUI();

            textLabel = TooltipContainer.CreateChild<Label>("text-tooltip");
        }

        public void Display(string text, Vector2 screenPosition)
        {
            textLabel.text = text;
            Show();
            RefreshPosition(screenPosition);
        }
    }
}