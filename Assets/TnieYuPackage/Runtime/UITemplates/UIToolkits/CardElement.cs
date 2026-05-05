using TnieYuPackage.GlobalExtensions;
using UnityEngine.UIElements;

namespace TnieYuPackage.Utils
{
    public class CardElement : BaseElement
    {
        private const string ELEMENT_PREFIX = "card-element";
        protected override string Prefix => ELEMENT_PREFIX;

        public VisualElement MainCard;
        public Image CardImage;
        public Label CardTitle;

        public CardElement()
        {
            this.AddToClassList("card-element");
            MainCard = this.CreateChild(GetStyleName("main-card"));
            CardImage = MainCard.CreateChild<Image>(GetStyleName("card-image"));
            CardTitle = MainCard.CreateChild<Label>(GetStyleName("card-title"));
        }
    }
}