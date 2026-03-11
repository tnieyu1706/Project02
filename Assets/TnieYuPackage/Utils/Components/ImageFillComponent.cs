using EditorAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace TnieYuPackage.Utils.Components
{
    [RequireComponent(typeof(Image))]
    public class ImageFillComponent : MonoBehaviour
    {
        [SerializeField] private Image image;

        void Awake()
        {
            image ??= GetComponent<Image>();
        }

        [Button]
        public void OnFillImage(float fillAmount)
        {
            image.fillAmount = fillAmount;
        } 
    }
}