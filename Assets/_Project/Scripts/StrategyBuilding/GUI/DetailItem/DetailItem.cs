using TnieYuPackage.GlobalExtensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.StrategyBuilding
{
    public class DetailItem : VisualElement
    {
        private readonly Label nameText;
        private readonly Image image;
        private readonly Label costText;

        public DetailItem()
        {
            this.AddClass("detail-item");
            nameText = this.CreateChild<Label>("detail-item__name");
            
            var container = this.CreateChild<VisualElement>("detail-item__container");
            image = container.CreateChild<Image>("detail-item__container__image");
            costText = container.CreateChild<Label>("detail-item__container__text");
        }

        public void SetItem(string itemName, Sprite imageSprite, string cost)
        {
            nameText.text = itemName;
            image.sprite = imageSprite;
            costText.text = cost;
        }
    }
}