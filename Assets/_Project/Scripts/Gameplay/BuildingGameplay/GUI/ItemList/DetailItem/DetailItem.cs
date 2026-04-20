using TnieYuPackage.GlobalExtensions;
using UnityEngine.UIElements;

namespace Game.BuildingGameplay
{
    public abstract class DetailItem : VisualElement
    {
        protected readonly Image Image;

        public DetailItem()
        {
            this.AddClass("detail-item");
            
            // Theo thiết kế mới, ở ngoài List ta chỉ hiển thị Image
            Image = this.CreateChild<Image>("detail-item__image");
        }
    }
}